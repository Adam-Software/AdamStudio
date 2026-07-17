using System;
using System.Net.Sockets;

namespace AdamStudio.Services.Interfaces
{
    /// <summary>
    /// Event args for TCP client data sent events.
    /// Carries the number of bytes sent and the number of bytes still
    /// pending in the send buffer.
    /// </summary>
    public class TcpClientSentEventArgs : EventArgs
    {
        public long Sent { get; init; }
        public long Pending { get; init; }
    }

    /// <summary>
    /// Event args for TCP client socket errors.
    /// </summary>
    public class TcpClientErrorEventArgs : EventArgs
    {
        public SocketError Error { get; init; }
    }

    /// <summary>
    /// Event args for TCP client data received events.
    /// The buffer may be reused by the underlying NetCoreServer — consume
    /// the data immediately or copy it before returning from the handler.
    /// </summary>
    public class TcpClientReceivedEventArgs : EventArgs
    {
        public byte[] Buffer { get; init; } = Array.Empty<byte>();
        public long Offset { get; init; }
        public long Size { get; init; }
    }

    /// <summary>
    /// Event args for TCP client reconnection events.
    /// ReconnectCount is the remaining number of reconnection attempts
    /// (decrements on each attempt).
    /// </summary>
    public class TcpClientReconnectedEventArgs : EventArgs
    {
        public int ReconnectCount { get; init; }
    }
}
