
namespace AdamStudio.Services.TcpClientDependency
{
    public class TcpCllientSettings
    {
        public TcpCllientSettings(string ip, int port, TcpClientOption option) 
        {
            Ip = ip;
            Port = port;
            Option = option;
        }

        public string Ip { get; }
        public int Port { get; }
        public TcpClientOption Option { get; }


    }
}
