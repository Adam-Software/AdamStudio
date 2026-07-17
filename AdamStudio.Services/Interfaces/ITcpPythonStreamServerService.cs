using System;
using System.Threading.Tasks;
using System.Threading;

namespace AdamStudio.Services.Interfaces
{

    public delegate void ClientConnectedEventHandler(object sender);
    public delegate void ClientDisconnectedEventHandler(object sender);
    public delegate void ClientDataReceivedEventHandler(object sender, string data);

    public interface ITcpPythonStreamServerService : IDisposable
    {

        public event ClientConnectedEventHandler RaiseClientConnectedEvent;
        public event ClientDisconnectedEventHandler RaiseClientDisconnectedEvent;
        public event ClientDataReceivedEventHandler RaiseClientDataReceivedEvent;

        public Task ExecuteAsync(CancellationToken stoppingToken = default);
        public Task StopAsync(CancellationToken stoppingToken = default);
        public void SendAsync(string data, CancellationToken cancellationToken = default);
    }
}
