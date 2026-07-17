using System;
using System.Net.Sockets;

namespace AdamStudio.Services.Interfaces
{
    public interface ITcpClientService : IDisposable
    {
        public event EventHandler RaiseTcpCientConnectedEvent;
        public event EventHandler RaiseTcpClientDisconnectedEvent;
        public event EventHandler<TcpClientErrorEventArgs> RaiseTcpClientErrorEvent;
        public event EventHandler<TcpClientReceivedEventArgs> RaiseTcpClientReceivedEvent;
        public event EventHandler<TcpClientReconnectedEventArgs> RaiseTcpClientReconnectedEvent;

        /// <summary>
        /// The number of reconnections when the connection is lost
        /// </summary>
        public int ReconnectCount { get; }

        /// <summary>
        /// Reconnection timeout
        /// </summary>
        public int ReconnectTimeout { get; }

        public void DisconnectAndStop();

        /// <summary>
        /// This method is implemented in NetCoreServer.TcpClient
        /// </summary>
        public bool ConnectAsync();
    }
}
