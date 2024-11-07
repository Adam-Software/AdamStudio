using AdamStudio.Services.TcpClientDependency;
using AdamStudio.Services.UdpClientServiceDependency;
using AdamStudio.Services.UdpServerServiceDependency;
using AdamStudio.Services.WebSocketClientDependency;

namespace AdamStudio.Services.Interfaces
{
    public interface IServiceSettings
    {
        public TcpCllientSettings TcpCllientSettings { get; }
        public UdpClientSettings UdpClientSettings { get; }
        public UdpServerSettings UdpServerSettings { get; }
        public WebSocketClientSettings WebSocketClientSettings { get; }
    }
}
