using System;

namespace AdamStudio.Services.Interfaces
{

    public delegate void RegionChangeEventHandler(object sender);

    public interface IRegionChangeAwareService : IDisposable
    {

        public event RegionChangeEventHandler RaiseRegionChangeEvent;

        public string RegionNavigationTargetName { get; set; }

    }
}
