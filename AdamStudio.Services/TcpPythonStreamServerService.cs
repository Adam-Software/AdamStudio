using AdamStudio.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TcpSharp;

namespace AdamStudio.Services
{
    public class TcpPythonStreamServerService : BackgroundService
    {
        private readonly ILogger<TcpPythonStreamServerService> mLoggerService;
        private readonly TcpSharpSocketServer mTcpSocketServer;
        private readonly Encoding mSystemEncoding = Console.OutputEncoding;

        #region ~

        public TcpPythonStreamServerService(IServiceProvider serviceProvider) 
        { 
            mLoggerService = serviceProvider.GetService<ILogger<TcpPythonStreamServerService>>();
            mTcpSocketServer = new TcpSharpSocketServer
            {
                KeepAlive = false,  
                Port = 18000,
            };

            Subscribe();
        }

        #endregion

        #region Subscribe/Unsubscribe

        private void Subscribe()
        {
            mTcpSocketServer.OnConnected += OnConnected;
            mTcpSocketServer.OnConnectionRequest += OnConnectionRequest;
            mTcpSocketServer.OnDataReceived += OnDataReceived;
            mTcpSocketServer.OnDisconnected += OnDisconnected;
        }

        private void UnSubscribe()
        {
            mTcpSocketServer.OnConnected -= OnConnected;
            mTcpSocketServer.OnConnectionRequest -= OnConnectionRequest;
            mTcpSocketServer.OnDataReceived -= OnDataReceived;
            mTcpSocketServer.OnDisconnected -= OnDisconnected;
        }

        private void OnConnectionRequest(object sender, OnServerConnectionRequestEventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Events

        private void OnDisconnected(object sender, OnServerDisconnectedEventArgs e)
        {
            mLoggerService.LogInformation("Client with ConnectionId {ConnectionId} disconnected ", e.ConnectionId);
        }

        private void OnDataReceived(object sender, OnServerDataReceivedEventArgs e)
        {
            //here incoming data
        }

        private void OnConnected(object sender, OnServerConnectedEventArgs e)
        {
            mLoggerService.LogInformation("Client with ConnectionId {ConnectionId} connected to Port {Port}", e.ConnectionId, e.Port);
        }

        #endregion

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            mTcpSocketServer.StartListening();
            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //here send data
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            mTcpSocketServer.StopListening();
            return base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            UnSubscribe();
        }
    }
}
