using AdamStudio.Services.Interfaces;
using AdamStudio.Services.TcpClientDependency;
using AdamStudio.Services.UdpClientServiceDependency;
using System.Net;

namespace AdamStudio.Core.Properties
{
    public class DefaultServiceSettings : IServiceSettings
    {
        public DefaultServiceSettings() 
        {
        }

        #region Fields

        public TcpCllientSettings TcpCllientSettings 
        { 
            get 
            {
                TcpClientOption option = new()
                {
                    ReconnectCount = Settings.Default.ReconnectQtyComunicateTcpClient,
                    ReconnectTimeout = Settings.Default.ReconnectTimeoutComunicateTcpClient
                };

                string ip = Settings.Default.ServerIP;

                if (string.IsNullOrEmpty(ip)) 
                    ip = "127.0.0.1";

                int port = Settings.Default.TcpConnectStatePort;

                TcpCllientSettings tcpCllientSettings = new(ip, port, option);

                return tcpCllientSettings;
            } 
        }

        public UdpClientSettings UdpClientSettings
        {
            get
            {
                IPAddress ip = IPAddress.Any;
                int port = int.Parse(Settings.Default.MessageDataExchangePort);

                UdpClientSettings udpClientSettings = new(ip, port);

                return udpClientSettings;
            }
        }

        #endregion

    }
}
