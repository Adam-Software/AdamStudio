using AdamStudio.Services.Interfaces;
using AdamStudio.Services.UdpClientServiceDependency;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Threading.Tasks;

namespace AdamStudio.Services
{
    public class UdpClientService : NetCoreServer.UdpServer, IUdpClientService
    {
        public event UdpClientMessageEnqueueEventHandler RaiseUdpClientMessageEnqueueEvent;

        #region Var

        private readonly QueueWithEvent<ReceivedData> mMessageQueue = new();

        #endregion

        public UdpClientService(IServiceProvider serviceProvider) : base(serviceProvider.GetService<IServiceSettings>().UdpClientSettings.IpAddress, serviceProvider.GetService<IServiceSettings>().UdpClientSettings.Port)
        {
            OptionDualMode = true;
            OptionReuseAddress = true;

            mMessageQueue.RaiseEnqueueEvent += RaiseEnqueueEvent;
        }

        private void RaiseEnqueueEvent(object sender, EventArgs e)
        {
            var messages = mMessageQueue.Dequeue();

            OnRaiseUdpClientMessageEnqueueEvent(messages);
        }

        protected override void OnReceived(EndPoint endpoint, byte[] buffer, long offset, long size)
        {
            Task.Run(() => 
            {
                mMessageQueue.Enqueue(new(endpoint, buffer, offset, size));
                ReceiveAsync();
            });
        }

        protected override void OnStarted()
        {
            ReceiveAsync();
        }

        #region OnRaiseEvents

        protected virtual void OnRaiseUdpClientMessageEnqueueEvent(ReceivedData data)
        {
            UdpClientMessageEnqueueEventHandler raiseEvent = RaiseUdpClientMessageEnqueueEvent;
            raiseEvent?.Invoke(this, data);
        }

        #endregion
    }
}
