using System;
using System.Net;

namespace AdamStudio.Services.Interfaces
{

    public delegate void UdpServerReceivedEventHandler(object sender, EndPoint endpoint, byte[] buffer, long offset, long size);

    public interface IUdpServerService : IDisposable
    {

        public event UdpServerReceivedEventHandler RaiseUdpServerReceivedEvent;

        public bool IsStarted { get; }

        public bool Start();

        public bool Stop();

    }
}
