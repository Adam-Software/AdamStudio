using System.Net;

namespace AdamStudio.Services.UdpClientServiceDependency
{
    public class UdpClientSettings
    {
        public UdpClientSettings(IPAddress ipAddress, int port)
        {
            IpAddress = ipAddress;
            Port = port;
        }

        public IPAddress IpAddress { get; }
        public int Port { get; }
    }
}
