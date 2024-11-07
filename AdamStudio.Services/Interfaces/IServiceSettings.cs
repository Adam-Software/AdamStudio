using AdamStudio.Services.TcpClientDependency;
using AdamStudio.Services.UdpClientServiceDependency;

namespace AdamStudio.Services.Interfaces
{
    public interface IServiceSettings
    {
        public TcpCllientSettings TcpCllientSettings { get; }
        public UdpClientSettings UdpClientSettings { get; }
    }
}
