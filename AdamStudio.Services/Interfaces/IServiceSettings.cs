using AdamStudio.Services.TcpClientDependency;
using AdamStudio.Services.UdpClientServiceDependency;
using AdamStudio.Services.UdpServerServiceDependency;

namespace AdamStudio.Services.Interfaces
{
    public interface IServiceSettings
    {
        public TcpCllientSettings TcpCllientSettings { get; }
        public UdpClientSettings UdpClientSettings { get; }
        public UdpServerSettings UdpServerSettings { get; }
    }
}
