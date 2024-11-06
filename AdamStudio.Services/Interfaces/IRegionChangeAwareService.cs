using System;

namespace AdamStudio.Services.Interfaces
{
    #region Delegate

    public delegate void RegionChangeEventHandler(object sender);

    #endregion

    public interface IRegionChangeAwareService : IDisposable
    {
        #region Events

        public event RegionChangeEventHandler RaiseRegionChangeEvent;

        #endregion

        #region Public fields

        public string RegionNavigationTargetName { get; set; }

        #endregion
    }
}
