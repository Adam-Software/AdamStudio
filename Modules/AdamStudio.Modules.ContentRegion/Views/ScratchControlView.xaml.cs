using AdamStudio.Controls.CustomControls.Services;
using AdamStudio.Core.Properties;
using AdamStudio.Services.Interfaces;
using AdamStudio.Services.WebViewProviderDependency;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Unosquare.FFME.Common;

namespace AdamStudio.Modules.ContentRegion.Views
{
    public partial class ScratchControlView : UserControl
    {
        #region Services

        private readonly IWebViewProvider mWebViewProvider;
        private readonly IStatusBarNotificationDeliveryService mStatusBarNotification;
        private readonly IControlHelper mControlHelper;
        private readonly IWebSocketClientService mWebSocketClient;
        private readonly IVideoViewProvider mVideoViewProvider;
        private readonly ILogger<ScratchControlView> mLogger;

        #endregion

        #region Var

        private readonly string mPathToSource;

        #endregion

        public ScratchControlView(ILogger<ScratchControlView> logger, IWebViewProvider webViewProvider, IStatusBarNotificationDeliveryService statusBarNotification, 
                        IFolderManagmentService folderManagment, IControlHelper controlHelper, IWebSocketClientService webSocketClient, IVideoViewProvider videoViewProvider)
        {
            InitializeComponent();
            InitializeWebViewCore();

            mLogger = logger;
            mWebViewProvider = webViewProvider;
            mStatusBarNotification = statusBarNotification;
            mControlHelper = controlHelper;

            mPathToSource = Path.Combine(folderManagment.CommonDirAppData, "BlocklySource");

            WebView.CoreWebView2InitializationCompleted += WebViewCoreWebView2InitializationCompleted;
            WebView.NavigationCompleted += WebViewNavigationCompleted;
            WebView.WebMessageReceived += WebViewWebMessageReceived;

            mWebViewProvider.RaiseExecuteJavaScriptEvent += RaiseExecuteJavaScriptEvent;
            mWebViewProvider.RaiseExecuteReloadWebViewEvent += RaiseExecuteReloadWebViewEvent;

            /*element event */
            //TextResulEditor.TextChanged += TextResulEditorTextChanged;
            MainGrid.SizeChanged += MainGridSizeChanged;
            SourceEditor.SizeChanged += TextResulEditorSizeChanged;

            /* service event */
            mControlHelper.RaiseBlocklyColumnWidthChangeEvent += RaiseBlocklyColumnWidthChangeEvent;

            /* video */
            mWebSocketClient = webSocketClient;
            mVideoViewProvider = videoViewProvider;

            VideoView.MediaOpening += VideoViewMediaOpening;
            VideoView.VideoFrameDecoded += VideoViewVideoFrameDecoded;
            VideoView.IsVisibleChanged += VideoViewIsVisibleChanged;
        }

        private async void VideoViewIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (VideoView.IsVisible)
            {
                string ip = Settings.Default.ServerIP;
                string port = Settings.Default.VideoDataExchangePort;
                var uri = new Uri($"http://{ip}:{port}/stream/0.mjpeg");
                await VideoView.Open(uri);
                return;
            }

            await VideoView.Close();
            mVideoViewProvider.ClearFrameRate();
        }

        private void VideoViewVideoFrameDecoded(object sender, FrameDecodedEventArgs e)
        {
            mVideoViewProvider.FrameRate = VideoView.VideoFrameRate;
        }

        private void VideoViewMediaOpening(object sender, MediaOpeningEventArgs e)
        {
            e.Options.IsTimeSyncDisabled = true;
            e.Options.IsAudioDisabled = true;
            e.Options.MinimumPlaybackBufferPercent = 0;

            e.Options.DecoderParams.EnableFastDecoding = true;
        }

        private void RaiseBlocklyColumnWidthChangeEvent(object sender)
        {
            double doubleWidth = mControlHelper.BlocklyColumnWidth;
            GridLength width = new(doubleWidth);
            BlocklyColumn.Width = width;
        }

        private void TextResulEditorSizeChanged(object sender, SizeChangedEventArgs e)
        {
            mControlHelper.BlocklyColumnActualWidth = BlocklyColumn.ActualWidth;
        }

        private void MainGridSizeChanged(object sender, SizeChangedEventArgs e)
        {
            mControlHelper.MainGridActualWidth = MainGrid.ActualWidth;   
        }

        private void RaiseExecuteReloadWebViewEvent(object sender)
        {
            if (mWebViewProvider.NeedReloadOnLoad)
            {
                WebView?.CoreWebView2?.Reload();
                mWebViewProvider.NeedReloadOnLoad = false;
            } 
        }

        private async Task<string> RaiseExecuteJavaScriptEvent(object sender, string script, bool deserializeResultToString = false)
        {
            string result = await WebView.ExecuteScriptAsync(script);

            if (deserializeResultToString)
            {
                string deserealizeString = JsonSerializer.Deserialize<string>(result);
                return deserealizeString;
            }

            return result;
        }

        private void WebViewNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            mWebViewProvider.NavigationComplete();
        }

        /*private void TextResulEditorTextChanged(object sender, EventArgs e)
        {
            //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(TextResulEditor.ScrollToEnd));
        }*/

        private async void InitializeWebViewCore()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), "AdamBrowser");
            CoreWebView2Environment env = await CoreWebView2Environment.CreateAsync(userDataFolder: tempPath);
            await WebView?.EnsureCoreWebView2Async(env);
        }

        private void WebViewCoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            WebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = !Settings.Default.DontShowBrowserMenuInBlockly;
            WebView?.CoreWebView2?.SetVirtualHostNameToFolderMapping("localhost", mPathToSource, CoreWebView2HostResourceAccessKind.Allow);
            WebView?.CoreWebView2?.Navigate("https://localhost/index.html");
        }

        private static readonly JsonSerializerOptions jsonSerializerOptions = new()
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString
        };

        private readonly JsonSerializerOptions mOptions = jsonSerializerOptions;

        private void WebViewWebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            WebMessageJsonReceived receivedResult;

            try
            {
                string receivedString = e.TryGetWebMessageAsString();
                receivedResult = JsonSerializer.Deserialize<WebMessageJsonReceived>(receivedString, mOptions);
            }
            catch
            {
                mLogger.LogError("Error reading blokly code");
                receivedResult = new WebMessageJsonReceived { Action = string.Empty, Data = string.Empty };
            }

            mWebViewProvider.WebViewMessageReceived(receivedResult);
        }
    }
}
