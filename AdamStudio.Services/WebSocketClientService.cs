using AdamStudio.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Security.Policy;
using System.Threading.Tasks;
using Websocket.Client;

namespace AdamStudio.Services
{
    public class WebSocketClientService : IWebSocketClientService
    {
        #region Events

        public event WebSocketClientReceivedEventHandler RaiseWebSocketClientReceivedEvent;
        public event WebSocketConnectedEventHandler RaiseWebSocketConnectedEvent;
        public event WebSocketClientDisconnectEventHandler RaiseWebSocketClientDisconnectEvent;

        #endregion

        #region var 

        private readonly WebsocketClient mWebsocketClient;

        #endregion

        #region ~

        public WebSocketClientService(IServiceProvider serviceProvider)
        {
            var uri = serviceProvider.GetService<IServiceSettings>().WebSocketClientSettings.Uri;

            mWebsocketClient = new(uri)
            {
                ReconnectTimeout = null
            };

            Subscribe();
        }

        #endregion

        #region Public field

        public bool IsStarted => mWebsocketClient.IsStarted;

        public bool IsRunning => mWebsocketClient.IsRunning;

        #endregion

        #region Public method

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

        #endregion

        #region Subscriptions

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

        #endregion

        #region OnRaiseEvents

        protected virtual void OnRaiseWebSocketClientReceivedEvent(string text)
        {
            WebSocketClientReceivedEventHandler raiseEvent = RaiseWebSocketClientReceivedEvent;
            raiseEvent?.Invoke(this, text);
        }

        protected virtual void OnRaiseWebSocketConnectedEvent()
        {
            WebSocketConnectedEventHandler raiseEvent = RaiseWebSocketConnectedEvent;
            raiseEvent?.Invoke(this);
        }

        protected virtual void OnRaiseWebSocketClientDisconnectEvent()
        {
            WebSocketClientDisconnectEventHandler raiseEvent = RaiseWebSocketClientDisconnectEvent;
            raiseEvent?.Invoke(this);
        }

        #endregion
    }
}



 
