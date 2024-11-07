using AdamStudio.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using NetCoreServer;
using System;
using System.Net;

namespace AdamStudio.Services
{
    public class UdpServerService : UdpServer, IUdpServerService
    {
        #region Events

        public event UdpServerReceivedEventHandler RaiseUdpServerReceivedEvent;

        #endregion

        #region ~

        public UdpServerService(IServiceProvider serviceProvider) : base(serviceProvider.GetService<IServiceSettings>().UdpServerSettings.IpAddress, serviceProvider.GetService<IServiceSettings>().UdpServerSettings.Port)
        {
            OptionDualMode = true;
            OptionReuseAddress = true;
        }

        #endregion

        #region Private methods

        protected override void OnStarted()
        {
            ReceiveAsync();
        }

        protected override void OnReceived(EndPoint endpoint, byte[] buffer, long offset, long size)
        {
            OnRaiseUdpServerReceivedEvent(endpoint, buffer, offset, size);
            ReceiveAsync();
        }

        protected override void OnSent(EndPoint endpoint, long sent)
        {
            ReceiveAsync();
        }

        #endregion

        #region OnRaiseEvents

        protected virtual void OnRaiseUdpServerReceivedEvent(EndPoint endpoint, byte[] buffer, long offset, long size)
        {
            UdpServerReceivedEventHandler raiseEvent = RaiseUdpServerReceivedEvent;
            raiseEvent?.Invoke(this, endpoint, buffer, offset, size);
        }

        #endregion
    }
}
