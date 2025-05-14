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
    public class FindMeClientService : IFindMeClientService
    {
        #region Sevices

        private readonly ILogger<FindMeClientService> mLogger;

        #endregion

        #region Var

        private readonly IPAddress[] mLocalIps = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
        private readonly UdpClient mClient = new(new IPEndPoint(IPAddress.Any, 12000));
        byte[] mSendBuffer = Encoding.UTF8.GetBytes("ping");

        #endregion

        #region Events

        public event FindStartedEventHandler RaiseFindStartedEvent;
        public event FindEndedEventHandler RaiseFindEndedEvent;

        #endregion

        #region ~

        public FindMeClientService(IServiceProvider serviceProvider) 
        {
            mLogger = serviceProvider.GetService<ILogger<FindMeClientService>>();
            FindAdresses = [];
        }

        #endregion

        #region Public methods

        public void SendBroadcastPing(bool useLocalServer)
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
                ResultParser(result, useLocalServer);
            }

            OnRaiseFindEndedEvent(FindAdresses);
        }

        #endregion

        #region Public fields

        public List<IPAddress> FindAdresses {  get;  }

        #endregion

        #region PrivateMethods

        private void ResultParser(Task<UdpReceiveResult> receiveResult, bool useLocalServer)
        {
            Task.Run(() =>
            {
                UdpReceiveResult result = receiveResult.Result;
                string reply = Encoding.UTF8.GetString(result.Buffer);
                
                if(reply != "pong")
                    return;

                if (useLocalServer)
                {
                    FindAdresses.Add(result.RemoteEndPoint.Address);
                    return;
                }

               

                if (!mLocalIps.Contains(result.RemoteEndPoint.Address))
                {
                    FindAdresses.Add(result.RemoteEndPoint.Address);
                }    
            });
        }

        private static List<IPAddress> GetBroadcasIPAddress()
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(x => x.OperationalStatus == OperationalStatus.Up)
                    .Where(x => x.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    .ToList();

            List<IPAddress> broadcactAddresses = [];

            foreach (NetworkInterface Interface in interfaces)
            {
                UnicastIPAddressInformationCollection UnicastIPInfoCol = Interface.GetIPProperties().UnicastAddresses;

                foreach (UnicastIPAddressInformation UnicatIPInfo in UnicastIPInfoCol.Where(static x => x.IsDnsEligible == true))
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

        protected virtual void OnRaiseFindEndedEvent(List<IPAddress> findIpAddresses)
        {
            FindEndedEventHandler raiseEvent = RaiseFindEndedEvent;
            raiseEvent?.Invoke(this, findIpAddresses);
        }

        #endregion
    }
}
