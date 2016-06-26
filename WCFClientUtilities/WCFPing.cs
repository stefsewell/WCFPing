using System;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WCFClientUtilities {
    /// <summary>
    /// 'Pings' a service that has the WcfPingBehaviour applied.
    /// </summary>
    public class WcfPing {
        internal delegate IWebClient WebClientFactory();
        private readonly WebClientFactory webClientFactory;

        public WcfPing() : this(() => new WebClient()) { }

        // Provided to allow unit tests to inject a factory method that will create a fake IWebClient
        internal WcfPing(WebClientFactory factory) {
            this.webClientFactory = factory;
        }

        public DateTime PingService(string serviceContractTypeName, string contractNamespace, string address) {
            string pingSoapMessage = CreatePingMessage(contractNamespace);

            using (var webClient = CreateWebClient()) {
                AddContentTypeHeader(webClient);

                string separator = contractNamespace.EndsWith("/") ? string.Empty : "/";

                webClient.AddHeader("SOAPAction", $@"""{contractNamespace}{separator}{serviceContractTypeName}/Ping""");
                var response = webClient.PostMessage(address, pingSoapMessage);
                return ProcessPingResponse(response, contractNamespace);
            }
        }

        public Task<DateTime> PingServiceAsync(string serviceContractTypeName, string contractNamespace, string address) {
            string pingSoapMessage = CreatePingMessage(contractNamespace);

            using (var webClient = CreateWebClient()) {
                AddContentTypeHeader(webClient);

                string separator = contractNamespace.EndsWith("/") ? string.Empty : "/";

                webClient.AddHeader("SOAPAction", $@"""{contractNamespace}{separator}{serviceContractTypeName}/Ping""");
                return webClient
                    .PostMessageAsync(address, pingSoapMessage)
                    .ContinueWith(task => ProcessPingResponse(task.Result, contractNamespace));
            }
        }

        public DateTime PingService<TServiceContract>(string address) {
            string pingSoapMessage = CreatePingMessage<TServiceContract>();

            using (var webClient = CreateWebClient()) {
                AddMessageHeaders<TServiceContract>(webClient);
                var response = webClient.PostMessage(address, pingSoapMessage);

                return ProcessPingResponse(response, GetContractNamespace<TServiceContract>());
            }
        }

        public Task<DateTime> PingServiceAsync<TServiceContract>(string address) {
            string pingSoapMessage = CreatePingMessage<TServiceContract>();

            using (var webClient = CreateWebClient()) {
                AddMessageHeaders<TServiceContract>(webClient);
                return webClient
                    .PostMessageAsync(address, pingSoapMessage)
                    .ContinueWith(task => ProcessPingResponse(task.Result, GetContractNamespace<TServiceContract>()));
            }
        }

        private string GetContractNamespace<TServiceContract>() {
            var serviceContractAttribute =
                (ServiceContractAttribute)
                    typeof(TServiceContract).GetCustomAttributes(typeof(ServiceContractAttribute), true).Single();

            if (serviceContractAttribute != null) {
                return serviceContractAttribute.Namespace;
            }

            return @"http://tempuri.org/";
        }

        private string CreatePingMessage<TServiceContract>() {
            string contractNamespace = GetContractNamespace<TServiceContract>();
            return CreatePingMessage(contractNamespace);
        }

        private string CreatePingMessage(string contractNamespace) {
            return $@"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/""><s:Body><Ping xmlns=""{contractNamespace}""/></s:Body></s:Envelope>";
        }

        private void AddMessageHeaders<TServiceContract>(IWebClient webClient) {
            AddContentTypeHeader(webClient);

            string contractNamespace = GetContractNamespace<TServiceContract>();
            if (!contractNamespace.EndsWith("/")) { contractNamespace = contractNamespace + "/"; }
            webClient.AddHeader("SOAPAction", $@"""{contractNamespace}{typeof(TServiceContract).Name}/Ping""");
        }

        private void AddContentTypeHeader(IWebClient webClient) {
            webClient.AddHeader("Content-Type", "text/xml; charset=utf-8");
        }

        private IWebClient CreateWebClient() {
            return this.webClientFactory.Invoke();
        }

        private DateTime ProcessPingResponse(string response, string contractNamespace) {
            XDocument responseXml = XDocument.Parse(response);
            XElement pingTime = responseXml.Descendants(XName.Get("PingResult", contractNamespace)).Single();
            DateTime serverPingTimeUtc = DateTime.Parse(pingTime.Value).ToUniversalTime();
            return serverPingTimeUtc;
        }
    }
}
