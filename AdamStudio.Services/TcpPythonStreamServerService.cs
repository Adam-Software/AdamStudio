using AdamStudio.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PHS.Networking.Enums;
using PHS.Networking.Server.Enums;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tcp.NET.Server;
using Tcp.NET.Server.Events.Args;
using Tcp.NET.Server.Models;


namespace AdamStudio.Services
{
    public class TcpPythonStreamServerService : ITcpPythonStreamServerService
    {
        private readonly ILogger<TcpPythonStreamServerService> mLoggerService;
        private readonly TcpNETServer mTcpServer;

        public event ClientConnectedEventHandler RaiseClientConnectedEvent;
        public event ClientDisconnectedEventHandler RaiseClientDisconnectedEvent;
        public event ClientDataReceivedEventHandler RaiseClientDataReceivedEvent;

        #region ~

        public TcpPythonStreamServerService(IServiceProvider serviceProvider) 
        { 
            mLoggerService = serviceProvider.GetService<ILogger<TcpPythonStreamServerService>>();

            mTcpServer = new TcpNETServer(new ParamsTcpServer(18000, "\r\n", connectionSuccessString: "Connected Successfully"));

            Subscribe();

            mLoggerService.LogInformation("Load ~");
        }

        #endregion

        #region Subscribe/Unsubscribe

        private void Subscribe()
        {
            mTcpServer.ConnectionEvent += ConnectionEvent;
            mTcpServer.MessageEvent += MessageEvent;
            mTcpServer.ServerEvent += ServerEvent;
        }

        private void ServerEvent(object sender, PHS.Networking.Server.Events.Args.ServerEventArgs args)
        {
            ServerEventType serverEvent = args.ServerEventType;

            switch (serverEvent)
            {
                case ServerEventType.Start:
                    break;
                case ServerEventType.Stop:
                    break;
            }
        }

        private void MessageEvent(object sender, TcpMessageServerEventArgs args)
        {
            MessageEventType eventType = args.MessageEventType;
            
            if(eventType == MessageEventType.Receive)
            {
                OnRaiseClientDataReceivedEvent(args.Message);
            }
        }

        private void ConnectionEvent(object sender, TcpConnectionServerEventArgs args)
        {
            ConnectionEventType connectionEvent = args.ConnectionEventType;

            switch (connectionEvent)
            {
                case ConnectionEventType.Connected:
                    {
                        mLoggerService.LogInformation("Client with ConnectionId {ConnectionId} connected", args.Connection.ConnectionId);
                        OnRaiseClientConnectedEvent();
                    }
                    
                    break;

                case ConnectionEventType.Disconnect:
                    {
                        mLoggerService.LogInformation("Client with ConnectionId {ConnectionId} disconnected ", args.Connection.ConnectionId);
                        
                        OnRaiseClientDisconnectedEvent();
                    }
                    break;
            }
        }

        private void UnSubscribe()
        {
            mTcpServer.ConnectionEvent -= ConnectionEvent;
            mTcpServer.MessageEvent -= MessageEvent;
            mTcpServer.ServerEvent -= ServerEvent;
        }


        #endregion

        #region Public methods

        public Task ExecuteAsync(CancellationToken stoppingToken = default)
        {
            return mTcpServer.StartAsync(stoppingToken);
        }

        public Task StopAsync(CancellationToken stoppingToken = default)
        {
            return mTcpServer.StopAsync(stoppingToken);
        }

        public void SendAsync(string data, CancellationToken cancellationToken = default)
        {
            mTcpServer.SendToConnectionAsync(data, mTcpServer.Connections.First(), cancellationToken);
        }

        public void Dispose()
        {
            UnSubscribe();
        }

        #endregion

        #region RaiseEvents

        protected virtual void OnRaiseClientConnectedEvent()
        {
            var raiseEvent = RaiseClientConnectedEvent;
            raiseEvent?.Invoke(this);
        }

        protected virtual void OnRaiseClientDisconnectedEvent()
        {
            var raiseEvent = RaiseClientDisconnectedEvent;
            raiseEvent?.Invoke(this);
        }

        protected virtual void OnRaiseClientDataReceivedEvent(string data)
        {
            var raiseEvent = RaiseClientDataReceivedEvent;
            raiseEvent?.Invoke(this, data);
        }

        #endregion
    }
}
