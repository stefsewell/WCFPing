using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WCFClientUtilities;

namespace UnitTests.WcfClientUltilities {
    [TestClass]
    public class WCFPingTests {
        [TestMethod]
        public void PingServiceReturnsValidServerDateTime_ContractNamespaceHasNoTrailingSlash() {

            const string  ContractNamespace = "http://test.com/contract/TestServiceContract";
            DateTime expectedServerDateTime = DateTime.UtcNow;

            var ping = new WcfPing(() => new WebClientStub(expectedServerDateTime, ContractNamespace));
            var serverDateTime = ping.PingService("TestTypename", ContractNamespace, "http://foo.bar.wak/service.svc");

            Assert.AreEqual(expectedServerDateTime, serverDateTime);
        }

        [TestMethod]
        public void PingServiceReturnsValidServerDateTime_ContractNamespaceHasTrailingSlash() {

            const string ContractNamespace = "http://test.com/contract/TestServiceContract/";
            DateTime expectedServerDateTime = DateTime.UtcNow;

            var ping = new WcfPing(() => new WebClientStub(expectedServerDateTime, ContractNamespace));
            var serverDateTime = ping.PingService("TestTypename", ContractNamespace, "http://foo.bar.wak/service.svc");

            Assert.AreEqual(expectedServerDateTime, serverDateTime);
        }
    }
}
