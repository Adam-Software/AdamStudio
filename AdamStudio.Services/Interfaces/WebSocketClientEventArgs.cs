using System;

namespace AdamStudio.Services.Interfaces
{
    /// <summary>
    /// Event args for WebSocket text message received events.
    /// </summary>
    public class WebSocketClientReceivedEventArgs : EventArgs
    {
        public string Text { get; init; } = string.Empty;
    }
}
