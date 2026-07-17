using System;
using System.Threading.Tasks;

namespace AdamStudio.Services.Interfaces
{

    public delegate void WebSocketClientReceivedEventHandler(object sender,  string text);
    public delegate void WebSocketConnectedEventHandler(object sender);
    public delegate void WebSocketClientDisconnectEventHandler(object sender);

    public interface IWebSocketClientService : IDisposable
    {

        public event WebSocketClientReceivedEventHandler RaiseWebSocketClientReceivedEvent;
        public event WebSocketConnectedEventHandler RaiseWebSocketConnectedEvent;
        public event WebSocketClientDisconnectEventHandler RaiseWebSocketClientDisconnectEvent;

        public bool IsStarted { get; }

        public bool IsRunning { get; }

        public Task ConnectAsync();

        public Task<bool> DisconnectAsync();

        public Task SendTextAsync(string text);

    }
}
