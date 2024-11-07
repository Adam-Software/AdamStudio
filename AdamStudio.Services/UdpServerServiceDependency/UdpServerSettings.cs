using System.Net;

namespace AdamStudio.Services.UdpServerServiceDependency
{
    public class UdpServerSettings
    {
        public UdpServerSettings(IPAddress ipAddress, int port)
        {
            IpAddress = ipAddress;
            Port = port;
        }

        public IPAddress IpAddress { get; }

        public int Port { get; }
    }
}
