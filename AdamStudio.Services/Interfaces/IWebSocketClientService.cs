using System;
using System.Threading.Tasks;

namespace AdamStudio.Services.Interfaces
{
    public interface IWebSocketClientService : IDisposable
    {
        public event EventHandler<WebSocketClientReceivedEventArgs> RaiseWebSocketClientReceivedEvent;
        public event EventHandler RaiseWebSocketConnectedEvent;
        public event EventHandler RaiseWebSocketClientDisconnectEvent;

        public bool IsStarted { get; }

        public bool IsRunning { get; }

        public Task ConnectAsync();

        public Task<bool> DisconnectAsync();

        public Task SendTextAsync(string text);
    }
}
