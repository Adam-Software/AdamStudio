using AdamStudio.Services.Interfaces;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Unosquare.FFME.Common;

namespace AdamStudio.Modules.ContentRegion.Views
{

    public partial class ComputerVisionControlView : UserControl
    {
        #region Service

        //private readonly IWebSocketClientService mWebSocketClient;
        private readonly IVideoViewProvider mVideoViewProvider;

        #endregion

        public ComputerVisionControlView(IWebSocketClientService webSocketClient, IVideoViewProvider videoViewProvider)
        {
            InitializeComponent();

            //mWebSocketClient = webSocketClient;
            mVideoViewProvider = videoViewProvider;

            VideoView.Loaded += VideoViewLoaded;
            VideoView.MediaOpening += VideoViewMediaOpening;
            VideoView.VideoFrameDecoded += VideoViewVideoFrameDecoded;
            Unloaded += UserControlUnloaded;
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

        private async void VideoViewLoaded(object sender, RoutedEventArgs e)
        {
            
            string ip = Core.Properties.Settings.Default.ServerIP;
            string port = Core.Properties.Settings.Default.VideoDataExchangePort;
            
            var uri = new Uri($"http://{ip}:{port}/stream/0.mjpeg");
            //var docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\test.avi";
            //var uri = new Uri($"{docPath}");

            await VideoView.Open(uri);
        }

        private async void UserControlUnloaded(object sender, RoutedEventArgs e)
        {
            await VideoView.Close();
            mVideoViewProvider.ClearFrameRate();
        }

        //?
        public string DownRightDirection { get; private set; } = "{\"move\":{\"x\": 0, \"y\": 1, \"z\": 0}}";

        //private void Button_KeyDown(object sender, KeyEventArgs e)
        //{
        //    mWebSocketClient.SendTextAsync(DownRightDirection);
        //}
    }
}
