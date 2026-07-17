using AdamStudio.Services.UdpClientServiceDependency;
using System;

namespace AdamStudio.Services.Interfaces
{

    public delegate void TcpServiceCientConnectedEventHandler(object sender);
    public delegate void TcpServiceClientDisconnectEventHandler(object sender, bool isUserRequest);
    public delegate void TcpServiceClientReconnectedEventHandler(object sender, int reconnectCounter);
    public delegate void UdpServiceServerReceivedEventHandler(object sender, string message);
    public delegate void UdpServiceClientMessageEnqueueEvent(object sender, ReceivedData data);

    /// <summary>
    /// ComunicateHeleper functional
    /// </summary>
    public interface ICommunicationProviderService : IDisposable
    {

        public event TcpServiceCientConnectedEventHandler RaiseTcpServiceCientConnectedEvent;
        public event TcpServiceClientDisconnectEventHandler RaiseTcpServiceClientDisconnectEvent;
        public event TcpServiceClientReconnectedEventHandler RaiseTcpServiceClientReconnectedEvent;
        public event UdpServiceServerReceivedEventHandler RaiseUdpServiceServerReceivedEvent;
        public event UdpServiceClientMessageEnqueueEvent RaiseUdpServiceClientMessageEnqueueEvent;

        public bool IsTcpClientConnected { get; }

        public void ConnectAllAsync();
        public void DisconnectAllAsync();
        public void DisconnectAllAsync(bool isUserRequest);
        public void WebSocketSendTextMessage(string message);

    }
}
