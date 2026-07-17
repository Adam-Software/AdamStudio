using AdamStudio.Services.WebViewProviderDependency;
using System;
using System.Threading.Tasks;

namespace AdamStudio.Services.Interfaces
{

    /*event in view model*/
    public delegate void WebViewNavigationCompleteEventHandler(object sender);
    public delegate void WebViewbMessageReceivedEventHandler(object sender, WebMessageJsonReceived webMessageReceived);

    /*event in view */
    public delegate Task<string> ExecuteJavaScriptEventHandler(object sender, string script, bool deserializeResultToString = false);
    public delegate void ExecuteReloadWebViewEventHandler(object sender);

    public interface IWebViewProvider : IDisposable
    {
        /*event in view model*/
        public event WebViewNavigationCompleteEventHandler RaiseWebViewNavigationCompleteEvent;
        public event WebViewbMessageReceivedEventHandler RaiseWebViewMessageReceivedEvent;

        /*event in view */
        public event ExecuteJavaScriptEventHandler RaiseExecuteJavaScriptEvent;
        public event ExecuteReloadWebViewEventHandler RaiseExecuteReloadWebViewEvent;

        /// <summary>
        /// Default false. Set to true for WebView reload 
        /// </summary>
        public bool NeedReloadOnLoad { get; set; }

        /// <summary>
        /// Execute JS script
        /// </summary>
        /// <returns>Json objects as string if deserializeResultToString false, deserialize object as string otherwise</returns>
        public Task<string> ExecuteJavaScript(string script, bool deserializeResultToString = false);

        /* in view model */
        public void ReloadWebView();
        public void NavigationComplete();
        public void WebViewMessageReceived(WebMessageJsonReceived receivedResult);
    }
}
