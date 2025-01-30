using System.Net;

namespace AdamStudio.Services.FindRobotDependency
{
    public class IpAddressInfo
    {
        public IPAddress IPAddress {  get; set; }

        public bool IsLocal  { get; set;}
    }
}
