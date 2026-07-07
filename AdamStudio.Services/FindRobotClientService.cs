using AdamStudio.Services.FindRobotDependency;
using AdamStudio.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AdamStudio.Services
{
    public class FindRobotClientService : IFindRobotClientService
    {
        #region Sevices

        private readonly ILogger<FindRobotClientService> mLogger;

        #endregion

        #region Var

        private readonly IPAddress[] mLocalIps = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
        private readonly UdpClient mClient = new(new IPEndPoint(IPAddress.Any, 12000));
        private readonly byte[] mSendBuffer = Encoding.UTF8.GetBytes("ping");

        #endregion

        #region Events

        public event FindStartedEventHandler RaiseFindStartedEvent;
        public event FindEndedEventHandler RaiseFindEndedEvent;

        #endregion

        #region ~

        public FindRobotClientService(IServiceProvider serviceProvider) 
        {
            mLogger = serviceProvider.GetService<ILogger<FindRobotClientService>>();
        }

        #endregion

        #region Public methods

        public async void SendBroadcastPing(bool useLocalServer)
        {
            List<IPAddress> broadcastAddress = GetBroadcasIPAddress();
            FindAdresses.Clear();

            OnRaiseFindStartedEvent();
            
            foreach (var address in broadcastAddress)
            {
                IPEndPoint endPoint = new(address, 11000);
                mLogger.LogTrace("Send broadcast ping {ip}", endPoint.Address);
                
                mClient.Send(mSendBuffer, endPoint);
                
                Task<UdpReceiveResult> result = mClient.ReceiveAsync();
                
                await Task.Delay(500); 
                
                ResultParser(result, useLocalServer);
            }

            OnRaiseFindEndedEvent(FindAdresses);
        }

        #endregion

        #region Public fields

        public List<IpAddressInfo> FindAdresses { get; } = [];

        List<IPAddress> IFindRobotClientService.FindAdresses => throw new NotImplementedException();

        #endregion

        #region PrivateMethods

        private void ResultParser(Task<UdpReceiveResult> receiveResult, bool useLocalServer)
        {
            Task.Run(() =>
            {
                try
                {
                    UdpReceiveResult result = receiveResult.Result;
                    string reply = Encoding.UTF8.GetString(result.Buffer);

                    if (reply != "pong")
                        return;

                    IpAddressInfo ip = new()
                    {
                        IPAddress = result.RemoteEndPoint.Address,
                        IsLocal = mLocalIps.Contains(result.RemoteEndPoint.Address)
                    };

                    if (useLocalServer)
                    { 
                        FindAdresses.Add(ip);
                        return;
                    }

                    if (!mLocalIps.Contains(result.RemoteEndPoint.Address))
                    {
                        FindAdresses.Add(ip);
                    }
                }
                catch (Exception ex) 
                { 
                    mLogger.LogWarning("Error parse result {exception}", ex.Message);
                }
            });
        }

        private static List<IPAddress> GetBroadcasIPAddress()
        {
            List<NetworkInterface> interfaces = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(x => x.OperationalStatus == OperationalStatus.Up)
                    .Where(x => x.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    .ToList();

            List<IPAddress> broadcactAddresses = [];

            foreach (NetworkInterface @interface in interfaces)
            {
                UnicastIPAddressInformationCollection unicastIPInfoCol = @interface.GetIPProperties().UnicastAddresses;

                foreach (UnicastIPAddressInformation UnicatIPInfo in unicastIPInfoCol.Where(static x => x.IsDnsEligible == true))
                {
                    IPAddress broadcast = GetBroadcastAddress(UnicatIPInfo.Address, UnicatIPInfo.IPv4Mask);
                    broadcactAddresses.Add(broadcast);
                }
            }

            return broadcactAddresses;
        }

        private static IPAddress GetBroadcastAddress(IPAddress address, IPAddress mask)
        {
            uint ipAddress = BitConverter.ToUInt32(address.GetAddressBytes(), 0);
            uint ipMaskV4 = BitConverter.ToUInt32(mask.GetAddressBytes(), 0);
            uint broadCastIpAddress = ipAddress | ~ipMaskV4;

            return new IPAddress(BitConverter.GetBytes(broadCastIpAddress));
        }

        #endregion

        #region OnRaise events

        protected virtual void OnRaiseFindStartedEvent()
        {
            FindStartedEventHandler raiseEvent = RaiseFindStartedEvent;
            raiseEvent?.Invoke(this);
        }

        protected virtual void OnRaiseFindEndedEvent(List<IpAddressInfo> findIpAddresses)
        {
            FindEndedEventHandler raiseEvent = RaiseFindEndedEvent;
            raiseEvent?.Invoke(this, findIpAddresses);
        }

        #endregion
    }
}
