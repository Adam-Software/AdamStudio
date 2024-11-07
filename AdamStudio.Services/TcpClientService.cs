using AdamStudio.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Sockets;
using System.Threading;

namespace AdamStudio.Services
{
    public class TcpClientService : NetCoreServer.TcpClient, ITcpClientService
    {
        #region Events

        public event TcpCientConnectedEventHandler RaiseTcpCientConnectedEvent;
        public event TcpCientSentEventHandler RaiseTcpCientSentEvent;
        public event TcpClientDisconnectEventHandler RaiseTcpClientDisconnectedEvent;
        public event TcpClientErrorEventHandler RaiseTcpClientErrorEvent;
        public event TcpClientReceivedEventHandler RaiseTcpClientReceivedEvent;
        public event TcpClientReconnectedEventHandler RaiseTcpClientReconnectedEvent;

        #endregion

        #region Private variable

        private int mReconnectTimeout;
        private int mReconnectCount;
        private bool mStop;
        private bool mDisconnectAlreadyInvoke = false;
        private CancellationTokenSource mTokenSource;

        #endregion

        #region ~

        /*public TcpClientService(string ip, int port, TcpClientOption option) : base(ip, port) 
        {
            ReconnectCount = option.ReconnectCount;
            ReconnectTimeout = option.ReconnectTimeout;
            
            RenewVariable(true);
            //it must be in renew variable method, but this called status wrong update
            //mReconnectCount = ReconnectCount;
        }*/

        public TcpClientService(IServiceProvider serviceProvider) : base(serviceProvider.GetService<IServiceSettings>().TcpCllientSettings.Ip, serviceProvider.GetService<IServiceSettings>().TcpCllientSettings.Port)
        {
            var option = serviceProvider.GetService<IServiceSettings>().TcpCllientSettings.Option;

            ReconnectCount = option.ReconnectCount;
            ReconnectTimeout = option.ReconnectTimeout;

            RenewVariable(true);
        }

        #endregion

        #region Public field

        /// <summary>
        /// The number of reconnections when the connection is lost
        /// </summary>
        public int ReconnectCount { get; }

        /// <summary>
        /// Reconnection timeout
        /// </summary>
        public int ReconnectTimeout { get; }

        #endregion

        #region Public methods

        public void DisconnectAndStop()
        {
            mTokenSource.Cancel();
            mStop = true;

            if (IsConnected)
            {
                _ = DisconnectAsync();
            }
            
            while (IsConnected)
            {
                _ = Thread.Yield();
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Need update varible on connected because clients in helper class static
        /// </summary>
        private void RenewVariable(bool updateReconnect)
        {
            mTokenSource = new CancellationTokenSource();
            mStop = false;
            mDisconnectAlreadyInvoke = false;
           
            mReconnectTimeout = ReconnectTimeout;

            if (!updateReconnect) return;
            //it must be in renew variable  in all method, but this called while connecting create inifinity update variable on reconnecting
            mReconnectCount = ReconnectCount;
        }

        protected override void OnDisconnected()
        {
            if (mStop) 
            {
                if (!mDisconnectAlreadyInvoke)
                {
                    mDisconnectAlreadyInvoke = true;
                    OnRaiseTcpClientDisconnectedEvent();
                }
                
                return; 
            }
            
            if (mReconnectCount == 0)
            {
                mStop = true;
                
                if (!mDisconnectAlreadyInvoke)
                {
                    mDisconnectAlreadyInvoke = true;
                    OnRaiseTcpClientDisconnectedEvent();
                }
                
                RenewVariable(true);

                //it must be in renew variable method, but this called status wrong update while reconnecting
                //mReconnectCount = ReconnectCount;

            }
            else
            {
                Reconnect(mTokenSource);
            }
        }

        private void Reconnect(CancellationTokenSource tokenSource)
        {          
            if (!tokenSource.IsCancellationRequested)
            {
                OnRaiseTcpClientReconnectedEvent(mReconnectCount--);

                _ = tokenSource.Token.WaitHandle.WaitOne(TimeSpan.FromSeconds(mReconnectTimeout));
                _ = ConnectAsync();
            }
            else
            {
                tokenSource.Dispose();
            }
        }

        protected override void OnConnected()
        {
            OnRaiseTcpCientConnectedEvent();
            RenewVariable(false);
            
            base.OnConnected();
        }

        protected override void OnSent(long sent, long pending)
        {
            OnRaiseTcpCientSentEvent(sent, pending);
        }

        protected override void OnError(SocketError error)
        {
            OnRaiseTcpClientErrorEvent(error);
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            OnRaiseTcpClientReceivedEvent(buffer, offset, size);
        }

        #endregion

        #region OnRaiseEvents

        protected virtual void OnRaiseTcpCientConnectedEvent()
        {
            TcpCientConnectedEventHandler raiseEvent = RaiseTcpCientConnectedEvent;
            raiseEvent?.Invoke(this);
        }

        protected virtual void OnRaiseTcpCientSentEvent(long sent, long pending)
        {
            TcpCientSentEventHandler raiseEvent = RaiseTcpCientSentEvent;
            raiseEvent?.Invoke(this, sent, pending);
        }

        protected virtual void OnRaiseTcpClientDisconnectedEvent()
        {
            TcpClientDisconnectEventHandler raiseEvent = RaiseTcpClientDisconnectedEvent;
            raiseEvent?.Invoke(this);
        }

        protected virtual void OnRaiseTcpClientErrorEvent(SocketError socketError)
        {
            TcpClientErrorEventHandler raiseEvent = RaiseTcpClientErrorEvent;
            raiseEvent?.Invoke(this, socketError);
        }

        protected virtual void OnRaiseTcpClientReceivedEvent(byte[] buffer, long offset, long size)
        {
            TcpClientReceivedEventHandler raiseEvent = RaiseTcpClientReceivedEvent;
            raiseEvent?.Invoke(this, buffer, offset, size);
        }
        
        protected virtual void OnRaiseTcpClientReconnectedEvent(int reconnectCount)
        {
            TcpClientReconnectedEventHandler raiseEvent = RaiseTcpClientReconnectedEvent;
            raiseEvent?.Invoke(this, reconnectCount);
        }

        #endregion
    }
}
