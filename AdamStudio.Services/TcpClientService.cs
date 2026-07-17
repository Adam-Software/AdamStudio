using AdamStudio.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Sockets;
using System.Threading;

namespace AdamStudio.Services
{
    public class TcpClientService : NetCoreServer.TcpClient, ITcpClientService
    {

        public event EventHandler RaiseTcpCientConnectedEvent;
        public event EventHandler<TcpClientSentEventArgs> RaiseTcpCientSentEvent;
        public event EventHandler RaiseTcpClientDisconnectedEvent;
        public event EventHandler<TcpClientErrorEventArgs> RaiseTcpClientErrorEvent;
        public event EventHandler<TcpClientReceivedEventArgs> RaiseTcpClientReceivedEvent;
        public event EventHandler<TcpClientReconnectedEventArgs> RaiseTcpClientReconnectedEvent;

        private int mReconnectTimeout;
        private int mReconnectCount;
        private bool mStop;
        private bool mDisconnectAlreadyInvoke = false;
        private CancellationTokenSource mTokenSource;

        public TcpClientService(IServiceProvider serviceProvider) : base(serviceProvider.GetService<IServiceSettings>().TcpCllientSettings.Ip, serviceProvider.GetService<IServiceSettings>().TcpCllientSettings.Port)
        {
            var option = serviceProvider.GetService<IServiceSettings>().TcpCllientSettings.Option;

            ReconnectCount = option.ReconnectCount;
            ReconnectTimeout = option.ReconnectTimeout;

            RenewVariable(true);
        }

        /// <summary>
        /// The number of reconnections when the connection is lost
        /// </summary>
        public int ReconnectCount { get; }

        /// <summary>
        /// Reconnection timeout
        /// </summary>
        public int ReconnectTimeout { get; }

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

        protected virtual void OnRaiseTcpCientConnectedEvent()
        {
            RaiseTcpCientConnectedEvent?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnRaiseTcpCientSentEvent(long sent, long pending)
        {
            RaiseTcpCientSentEvent?.Invoke(this, new TcpClientSentEventArgs { Sent = sent, Pending = pending });
        }

        protected virtual void OnRaiseTcpClientDisconnectedEvent()
        {
            RaiseTcpClientDisconnectedEvent?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnRaiseTcpClientErrorEvent(SocketError socketError)
        {
            RaiseTcpClientErrorEvent?.Invoke(this, new TcpClientErrorEventArgs { Error = socketError });
        }

        protected virtual void OnRaiseTcpClientReceivedEvent(byte[] buffer, long offset, long size)
        {
            RaiseTcpClientReceivedEvent?.Invoke(this, new TcpClientReceivedEventArgs { Buffer = buffer, Offset = offset, Size = size });
        }

        protected virtual void OnRaiseTcpClientReconnectedEvent(int reconnectCount)
        {
            RaiseTcpClientReconnectedEvent?.Invoke(this, new TcpClientReconnectedEventArgs { ReconnectCount = reconnectCount });
        }

    }
}
