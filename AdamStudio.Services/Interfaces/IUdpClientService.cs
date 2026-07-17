using AdamStudio.Services.UdpClientServiceDependency;
using System;

namespace AdamStudio.Services.Interfaces
{

    public delegate void UdpClientMessageEnqueueEventHandler(object sender, ReceivedData data);

    public interface IUdpClientService :  IDisposable
    {

        public event UdpClientMessageEnqueueEventHandler RaiseUdpClientMessageEnqueueEvent;

        public bool IsStarted { get; }

        public bool Stop();

        public bool Start();

    }
}
