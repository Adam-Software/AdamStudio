using AdamStudio.Services.Interfaces;
using AdamStudio.Services.UdpClientServiceDependency;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AdamStudio.Services
{
    public class CommunicationProviderService : ICommunicationProviderService
    {

        public event TcpServiceCientConnectedEventHandler RaiseTcpServiceCientConnectedEvent;
        public event TcpServiceClientDisconnectEventHandler RaiseTcpServiceClientDisconnectEvent;
        public event TcpServiceClientReconnectedEventHandler RaiseTcpServiceClientReconnectedEvent;
        public event UdpServiceServerReceivedEventHandler RaiseUdpServiceServerReceivedEvent;
        public event UdpServiceClientMessageEnqueueEvent RaiseUdpServiceClientMessageEnqueueEvent;

        private readonly ITcpClientService mTcpClientService;
        private readonly IUdpClientService mUdpClientService;
        private readonly IUdpServerService mUdpServerService;
        private readonly IWebSocketClientService mWebSocketClientService;

        private bool mIsDisconnectByUserRequest = false;

        public CommunicationProviderService(IServiceProvider serviceProvider)
        {
            mTcpClientService = serviceProvider.GetService<ITcpClientService>(); 
            mUdpClientService = serviceProvider.GetService<IUdpClientService>();
            mUdpServerService = serviceProvider.GetService<IUdpServerService>();
            mWebSocketClientService = serviceProvider.GetService<IWebSocketClientService>();

            Subscribe();
        }

        public bool IsTcpClientConnected { get; private set; }

        public void ConnectAllAsync()
        {
            _ = Task.Run(mTcpClientService.ConnectAsync);
            _ = Task.Run(mUdpClientService.Start);
            _ = Task.Run(mUdpServerService.Start);
            _ = Task.Run(mWebSocketClientService.ConnectAsync);
        }

        public void DisconnectAllAsync()
        {
            mIsDisconnectByUserRequest = false;

            _ = Task.Run(mTcpClientService.DisconnectAndStop);
            _ = Task.Run(mUdpClientService.Stop);
            _ = Task.Run(mUdpServerService.Stop);
            _ = Task.Run(mWebSocketClientService.DisconnectAsync);
        }

        public void DisconnectAllAsync(bool isUserRequest)
        {
            mIsDisconnectByUserRequest = isUserRequest;

            _ = Task.Run(mTcpClientService.DisconnectAndStop);
            _ = Task.Run(mUdpClientService.Stop);
            _ = Task.Run(mUdpServerService.Stop);
            _ = Task.Run(mWebSocketClientService.DisconnectAsync);
        }

        public void WebSocketSendTextMessage(string message)
        {
            Task.Run(() => mWebSocketClientService.SendTextAsync(message));
        }

        public void Dispose()
        {
            DisconnectAllAsync();
            Unsubscribe();

            mTcpClientService.Dispose();
            mUdpClientService.Dispose();
            mUdpServerService.Dispose();
            mWebSocketClientService.Dispose();
        }

        private void Subscribe()
        {
            mTcpClientService.RaiseTcpClientReconnectedEvent += RaiseServiceTcpClientReconnected;
            mTcpClientService.RaiseTcpCientConnectedEvent += RaiseServiceTcpCientConnected;
            mTcpClientService.RaiseTcpClientDisconnectedEvent += RaiseTcpClientDisconnected;
            
            mUdpClientService.RaiseUdpClientMessageEnqueueEvent += RaiseUdpClientMessageEnqueueEvent;
            mUdpServerService.RaiseUdpServerReceivedEvent += RaiseServiceUdpServerReceived;
        }

        private void Unsubscribe()
        {
            mTcpClientService.RaiseTcpClientReconnectedEvent -= RaiseServiceTcpClientReconnected;
            mTcpClientService.RaiseTcpCientConnectedEvent -= RaiseServiceTcpCientConnected;
            mTcpClientService.RaiseTcpClientDisconnectedEvent -= RaiseTcpClientDisconnected;

            mUdpClientService.RaiseUdpClientMessageEnqueueEvent -= RaiseUdpClientMessageEnqueueEvent;
            mUdpServerService.RaiseUdpServerReceivedEvent -= RaiseServiceUdpServerReceived;
        }

        private void RaiseServiceTcpCientConnected(object sender, EventArgs e)
        {
            IsTcpClientConnected = true;

            OnRaiseTcpServiceCientConnectedEvent();

            mUdpClientService.Start();
            mUdpServerService.Start();
            mWebSocketClientService.ConnectAsync();
        }

        private void RaiseTcpClientDisconnected(object sender, EventArgs e)
        {
            IsTcpClientConnected = false;

            OnRaiseTcpServiceClientDisconnectEvent();

            mUdpClientService.Stop();
            mUdpServerService.Stop();
            mWebSocketClientService.DisconnectAsync();
        }

        private void RaiseServiceTcpClientReconnected(object sender, TcpClientReconnectedEventArgs e)
        {
            OnRaiseTcpServiceClientReconnectedEvent(e.ReconnectCount);
        }

        private void RaiseUdpClientMessageEnqueueEvent(object sender, ReceivedData data)
        {
            OnRaiseUdpServiceClientMessageEnqueueEvent(data);
        }

        private void RaiseServiceUdpServerReceived(object sender, EndPoint endpoint, byte[] buffer, long offset, long size)
        {
            string encodedMessage = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
            OnRaiseUdpServiceServerReceivedEvent(encodedMessage);
        }

        protected virtual void OnRaiseTcpServiceCientConnectedEvent()
        {
            TcpServiceCientConnectedEventHandler raiseEvent = RaiseTcpServiceCientConnectedEvent;
            raiseEvent?.Invoke(this);
        }

        protected virtual void OnRaiseTcpServiceClientDisconnectEvent()
        {
            TcpServiceClientDisconnectEventHandler raiseEvent = RaiseTcpServiceClientDisconnectEvent;
            raiseEvent?.Invoke(this, mIsDisconnectByUserRequest);

            mIsDisconnectByUserRequest = false;
        }

        public virtual void OnRaiseTcpServiceClientReconnectedEvent(int reconnectCounter)
        {
            TcpServiceClientReconnectedEventHandler raiseEvent = RaiseTcpServiceClientReconnectedEvent;
            raiseEvent?.Invoke(this, reconnectCounter);
        }

        protected virtual void OnRaiseUdpServiceServerReceivedEvent(string message)
        {
            UdpServiceServerReceivedEventHandler raiseEvent = RaiseUdpServiceServerReceivedEvent;
            raiseEvent?.Invoke(this, message);
        }

        protected virtual void OnRaiseUdpServiceClientMessageEnqueueEvent(ReceivedData data)
        {
            UdpServiceClientMessageEnqueueEvent raiseEvent = RaiseUdpServiceClientMessageEnqueueEvent;
            raiseEvent?.Invoke(this, data);
        }

    }
}
