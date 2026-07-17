using AdamStudio.Services.Interfaces;
using AdamStudio.Services.WebViewProviderDependency;
using System.Threading.Tasks;

namespace AdamStudio.Services
{
    public class WebViewProvider : IWebViewProvider
    {

        /*event in view model*/
        public event WebViewNavigationCompleteEventHandler RaiseWebViewNavigationCompleteEvent;
        public event WebViewbMessageReceivedEventHandler RaiseWebViewMessageReceivedEvent;

        /*event in view */
        public event ExecuteJavaScriptEventHandler RaiseExecuteJavaScriptEvent;
        public event ExecuteReloadWebViewEventHandler RaiseExecuteReloadWebViewEvent;

        public WebViewProvider(){}

        public bool NeedReloadOnLoad { get; set; } = false;

        public void WebViewMessageReceived(WebMessageJsonReceived receivedResult)
        {
            OnRaiseWebViewbMessageReceivedEvent(receivedResult);
        }

        public Task<string> ExecuteJavaScript(string script, bool deserializeResultToString = false)
        {
            return OnRaiseExecuteJavaScriptEvent(script, deserializeResultToString);
        }

        public void NavigationComplete()
        {
            OnRaiseWebViewNavigationCompleteEvent();
        }

        public virtual void ReloadWebView()
        {
            OnRaiseExecuteReloadWebViewEvent();
        }

        public void Dispose(){}

        protected virtual void OnRaiseWebViewNavigationCompleteEvent()
        {
            WebViewNavigationCompleteEventHandler raiseEvent = RaiseWebViewNavigationCompleteEvent;
            raiseEvent?.Invoke(this);
        }

        protected virtual void OnRaiseWebViewbMessageReceivedEvent(WebMessageJsonReceived result)
        {
            WebViewbMessageReceivedEventHandler raiseEvent = RaiseWebViewMessageReceivedEvent;
            raiseEvent?.Invoke(this, result);
        }

        protected virtual Task<string> OnRaiseExecuteJavaScriptEvent(string script, bool deserializeResultToString)
        {
            ExecuteJavaScriptEventHandler raiseEvent = RaiseExecuteJavaScriptEvent;
            return raiseEvent?.Invoke(this, script, deserializeResultToString);
        }

        protected virtual void OnRaiseExecuteReloadWebViewEvent()
        {
            ExecuteReloadWebViewEventHandler raiseEvent = RaiseExecuteReloadWebViewEvent;
            raiseEvent?.Invoke(this);
        }

    }
}
