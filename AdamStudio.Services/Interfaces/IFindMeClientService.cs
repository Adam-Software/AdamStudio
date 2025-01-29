using System.Collections.Generic;
using System.Net;

namespace AdamStudio.Services.Interfaces
{
    public delegate void FindStartedEventHandler(object sender);
    public delegate void FindEndedEventHandler(object sender, List<IPAddress> findIpAddresses);

    public interface IFindMeClientService
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
