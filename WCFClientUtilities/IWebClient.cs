using System;
using System.Threading.Tasks;

namespace WCFClientUtilities {
    /// <summary>
    /// Provides web access methods
    /// </summary>
    public interface IWebClient : IDisposable {
        void AddHeader(string name, string value);
        string PostMessage(string address, string message);
        Task<string> PostMessageAsync(string address, string message);
    }
}
