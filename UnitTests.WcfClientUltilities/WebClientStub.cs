using System;
using System.Diagnostics;
using System.Threading.Tasks;

using WCFClientUtilities;

namespace UnitTests.WcfClientUltilities {
    internal class WebClientStub : IWebClient {
        private DateTime serverDateTime;
        private string contractNamespace;

        private const string Response = @"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
  <s:Header />
  <s:Body>
    <PingResponse xmlns=""{0}"">
      <PingResult>{1}</PingResult>
    </PingResponse>
  </s:Body>
</s:Envelope>";

        internal WebClientStub(DateTime serverDateTime, string contractNamespace) {
            this.serverDateTime = serverDateTime;
            this.contractNamespace = contractNamespace;
        }

        public void Dispose() {
            Trace.WriteLine("Dispose");
        }

        public void AddHeader(string name, string value) {
            Trace.WriteLine("AddHeader");
        }

        public string PostMessage(string address, string message) {
            Trace.WriteLine("PostMessage");
            return string.Format(Response, contractNamespace, serverDateTime.ToString("o"));
        }

        public Task<string> PostMessageAsync(string address, string message) {
            Trace.WriteLine("PostMessageAsync");
            throw new NotImplementedException();
        }
    }
}
