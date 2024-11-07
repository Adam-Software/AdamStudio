using AdamStudio.Services.TcpClientDependency;

namespace AdamStudio.Services.Interfaces
{
    public interface IServiceSettings
    {
        public TcpCllientSettings TcpCllientSettings { get; }
    }
}
