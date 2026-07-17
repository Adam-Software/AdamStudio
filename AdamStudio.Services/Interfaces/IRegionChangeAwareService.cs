using System;

namespace AdamStudio.Services.Interfaces
{


    public interface IRegionChangeAwareService : IDisposable
    {

        public event EventHandler RaiseRegionChangeEvent;

        public string RegionNavigationTargetName { get; set; }

    }
}
