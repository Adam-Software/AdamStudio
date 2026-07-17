using AdamStudio.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Websocket.Client;

namespace AdamStudio.Services
{
    public class WebSocketClientService : IWebSocketClientService
    {

        public event EventHandler<WebSocketClientReceivedEventArgs> RaiseWebSocketClientReceivedEvent;
        public event EventHandler RaiseWebSocketConnectedEvent;
        public event EventHandler RaiseWebSocketClientDisconnectEvent;

        private readonly WebsocketClient mWebsocketClient;

        public WebSocketClientService(IServiceProvider serviceProvider)
        {
            var uri = serviceProvider.GetService<IServiceSettings>().WebSocketClientSettings.Uri;

            mWebsocketClient = new(uri)
            {
                ReconnectTimeout = null
            };

            Subscribe();
        }

        public bool IsStarted => mWebsocketClient.IsStarted;

        public bool IsRunning => mWebsocketClient.IsRunning;

        public Task ConnectAsync()
        {
            return  mWebsocketClient.StartOrFail();
        }

        public Task<bool> DisconnectAsync()
        {
            return mWebsocketClient.StopOrFail(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "Nomal close");
        }

        public Task SendTextAsync(string text)
        {
            if(!string.IsNullOrEmpty(text))
            {
                Task task = mWebsocketClient.SendInstant(text);
                return task;
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            mWebsocketClient.Dispose();
        }

        private void Subscribe()
        {
            mWebsocketClient.MessageReceived.Subscribe(message =>
            {
                OnRaiseWebSocketClientReceivedEvent(message.Text);
            });

            mWebsocketClient.DisconnectionHappened.Subscribe(eventHappened =>
            {
                OnRaiseWebSocketClientDisconnectEvent();
            });

            mWebsocketClient.ReconnectionHappened.Subscribe(eventHappened =>
            {
                OnRaiseWebSocketConnectedEvent();
            });
        }

        protected virtual void OnRaiseWebSocketClientReceivedEvent(string text)
        {
            RaiseWebSocketClientReceivedEvent?.Invoke(this, new WebSocketClientReceivedEventArgs { Text = text });
        }

        protected virtual void OnRaiseWebSocketConnectedEvent()
        {
            RaiseWebSocketConnectedEvent?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnRaiseWebSocketClientDisconnectEvent()
        {
            RaiseWebSocketClientDisconnectEvent?.Invoke(this, EventArgs.Empty);
        }

    }
}

 
