using AdamStudio.Services.FindRobotDependency;
using System.Collections.Generic;
using System.Net;

namespace AdamStudio.Services.Interfaces
{
    public delegate void FindStartedEventHandler(object sender);
    public delegate void FindEndedEventHandler(object sender, List<IpAddressInfo> findIpAddresses);

    public interface IFindRobotClientService
    {
        #region Events

        public event FindStartedEventHandler RaiseFindStartedEvent;   
        public event FindEndedEventHandler RaiseFindEndedEvent;

        #endregion

        #region Fields

        public List<IPAddress> FindAdresses { get; }

        #endregion

        #region Methods

        public void SendBroadcastPing(bool useLocalServer);

        #endregion
    }
}
