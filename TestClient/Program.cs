using System;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Xml.Linq;

using TestClient.TestService;

namespace TestClient {
    class Program {
        static void Main() {
            CallServiceUsingGeneratedProxy();

            CallWcfServiceUsingGenericProxy();

            CallWorkflowServiceUsingGenericProxy();

            var asyncTask = CallWcfServiceUsingGenericProxyAsync();

            asyncTask.ContinueWith(task =>
            {
                Console.WriteLine("Tests complete, press any key to exit.");
            });
            
            var asyncTask2 = CallWcfServiceUsingGenericProxy2Async();

            asyncTask2.ContinueWith(task => {
                Console.WriteLine("Tests complete, press any key to exit.");
            });

            Console.ReadKey();
        }

        private static async Task CallWcfServiceUsingGenericProxyAsync() {
            const string address = @"http://localhost/TestService/Service1.svc";
            const string serviceContractType = "IService1";
            const string serviceContractNamespace = @"http://aderant.com/expert/contract/TestServiceContract";

            Console.WriteLine("Pinging WCF service using the string-based common proxy asynchronously...");
            DateTime utcStart = DateTime.UtcNow;
            var response = await PingServiceAsync(serviceContractType, serviceContractNamespace, address);
            DateTime utcFinished = DateTime.UtcNow;

            DateTime serverPingTimeUtc = ProcessPingResponse(response, serviceContractNamespace);
            WriteTimingMessage(utcStart, serverPingTimeUtc, utcFinished);
        }

        private static async Task CallWcfServiceUsingGenericProxy2Async() {
            const string address = @"http://localhost/TestService/Service1.svc";
            
            Console.WriteLine("Pinging WCF service using the generic common proxy asynchronously...");
            DateTime utcStart = DateTime.UtcNow;
            var response = await PingServiceAsync<IService1>(address);
            DateTime utcFinished = DateTime.UtcNow;

            DateTime serverPingTimeUtc = ProcessPingResponse(response, GetContractNamespace<IService1>());
            WriteTimingMessage(utcStart, serverPingTimeUtc, utcFinished);
        }

        private static void CallWorkflowServiceUsingGenericProxy() {
            const string address = @"http://localhost/TestWFService/Service1.xamlx";
            const string serviceContractType = "IFooService";
            const string serviceContractNamespace = @"http://tempuri.org/";

            Console.WriteLine("Pinging workflow service using the generic proxy...");
            DateTime utcStart = DateTime.UtcNow;
            string response = PingService(serviceContractType, serviceContractNamespace, address);
            DateTime utcFinished = DateTime.UtcNow;

            DateTime serverPingTimeUtc = ProcessPingResponse(response, serviceContractNamespace);
            WriteTimingMessage(utcStart, serverPingTimeUtc, utcFinished);
        }

        private static void CallWcfServiceUsingGenericProxy() {
            const string address = @"http://localhost/TestService/Service1.svc";
            const string serviceContractType = "IService1";
            const string serviceContractNamespace = @"http://aderant.com/expert/contract/TestServiceContract";

            Console.WriteLine("Pinging WCF service using the generic proxy...");
            DateTime utcStart = DateTime.UtcNow;
            string response = PingService(serviceContractType, serviceContractNamespace, address);
            DateTime utcFinished = DateTime.UtcNow;

            DateTime serverPingTimeUtc = ProcessPingResponse(response, serviceContractNamespace);
            WriteTimingMessage(utcStart, serverPingTimeUtc, utcFinished);
        }

        private static void CallServiceUsingGeneratedProxy() {
            Console.WriteLine("Pinging service using the generated proxy...");
            Service1Client client = new Service1Client();

            DateTime utcStart = DateTime.UtcNow;
            DateTime pingedAt = client.Ping();
            DateTime utcFinished = DateTime.UtcNow;

            WriteTimingMessage(utcStart, pingedAt, utcFinished);
        }

        private static DateTime ProcessPingResponse(string response, string contractNamespace) {
            XDocument responseXml = XDocument.Parse(response);
            XElement pingTime = responseXml.Descendants(XName.Get("PingResult", contractNamespace)).Single();
            DateTime serverPingTimeUtc = DateTime.Parse(pingTime.Value).ToUniversalTime();
            return serverPingTimeUtc;
        }

        private static string PingService(string serviceContractType, string contractNamespace, string address) {
            string pingSoapMessage =
                $@"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/""><s:Body><Ping xmlns=""{contractNamespace}""/></s:Body></s:Envelope>";

            if (!contractNamespace.EndsWith("/")) { contractNamespace = contractNamespace + "/";}
            
            WebClient pingClient = new WebClient();
            pingClient.Headers.Add("Content-Type", "text/xml; charset=utf-8");
            pingClient.Headers.Add("SOAPAction", $@"""{contractNamespace}{serviceContractType}/Ping""");
            string response = pingClient.UploadString(address, pingSoapMessage);
            return response;
        }

        private static Task<string> PingServiceAsync(string serviceContractType, string contractNamespace, string address) {
            string pingSoapMessage =
                $@"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/""><s:Body><Ping xmlns=""{contractNamespace}""/></s:Body></s:Envelope>";

            if (!contractNamespace.EndsWith("/")) { contractNamespace = contractNamespace + "/"; }

            WebClient pingClient = new WebClient();
            pingClient.Headers.Add("Content-Type", "text/xml; charset=utf-8");
            pingClient.Headers.Add("SOAPAction", $@"""{contractNamespace}{serviceContractType}/Ping""");
            return pingClient.UploadStringTaskAsync(address, pingSoapMessage);
        }

        private static Task<string> PingServiceAsync<TServiceContract>(string address)
        {
            string contractNamespace = GetContractNamespace<TServiceContract>();

            string pingSoapMessage =
                $@"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/""><s:Body><Ping xmlns=""{contractNamespace}""/></s:Body></s:Envelope>";

            if (!contractNamespace.EndsWith("/")) { contractNamespace = contractNamespace + "/"; }
            
            WebClient pingClient = new WebClient();
            pingClient.Headers.Add("Content-Type", "text/xml; charset=utf-8");
            pingClient.Headers.Add("SOAPAction", $@"""{contractNamespace}{typeof(TServiceContract).Name}/Ping""");
            return pingClient.UploadStringTaskAsync(address, pingSoapMessage);
        }


        private static void WriteTimingMessage(DateTime utcStart, DateTime pingedAt, DateTime utcFinished) {
            Console.WriteLine("Completed at {0}, service contacted at {1}, time taken: {2} ms.",
                              utcFinished.ToLongTimeString(),
                              pingedAt.ToLongTimeString(),
                              utcFinished.Subtract(utcStart).Milliseconds);
        }

        private static string GetContractNamespace<TServiceContract>()
        {
            return (
                (ServiceContractAttribute)typeof(TServiceContract).GetCustomAttributes(typeof(ServiceContractAttribute), true).Single()
                ).Namespace;
        }
    }
}