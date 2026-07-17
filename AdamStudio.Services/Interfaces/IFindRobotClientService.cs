using AdamStudio.Services.FindRobotDependency;
using System.Collections.Generic;
using System.Net;

namespace AdamStudio.Services.Interfaces
{
    public delegate void FindStartedEventHandler(object sender);
    public delegate void FindEndedEventHandler(object sender, List<IpAddressInfo> findIpAddresses);

    public interface IFindRobotClientService
    {

        public event FindStartedEventHandler RaiseFindStartedEvent;   
        public event FindEndedEventHandler RaiseFindEndedEvent;

        public List<IPAddress> FindAdresses { get; }

        public void SendBroadcastPing(bool useLocalServer);

    }
}
