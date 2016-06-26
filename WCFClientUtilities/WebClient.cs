using System;
using System.Threading.Tasks;
using NetWebClient = System.Net.WebClient;

namespace WCFClientUtilities {
    /// <summary>
    /// Wrapper around the System.Net.WebClient class which implements IWebClient
    /// </summary>
    internal class WebClient : IWebClient {
        private readonly NetWebClient webClient = new NetWebClient();
        private bool disposed;
        private readonly object syncLock = new object();

        private const string DisposedErrorMessage =
            "The web client instance has already been disposed and is no longer available.";

        public void AddHeader(string name, string value) {
            if (disposed) {
                throw new ObjectDisposedException("WebClient", DisposedErrorMessage);
            }
            webClient.Headers.Add(name, value);
        }

        public string PostMessage(string address, string message) {
            if (disposed) {
                throw new ObjectDisposedException("WebClient", DisposedErrorMessage);
            }

            return webClient.UploadString(address, message);
        }

        public Task<string> PostMessageAsync(string address, string message) {
            if (disposed) {
                throw new ObjectDisposedException("WebClient", DisposedErrorMessage);
            }

            return webClient.UploadStringTaskAsync(address, message);
        }

        public void Dispose() {
            lock (syncLock) {
                if (!disposed) {
                    if (webClient != null) {
                        webClient.Dispose();
                    }
                    disposed = true;
                }
            }
        }
    }
}
