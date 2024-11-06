using AdamStudio.Services.Interfaces;
using Prism.Mvvm;

namespace AdamStudio.Services
{
    public class RegionChangeAwareService : BindableBase, IRegionChangeAwareService
    {
        #region Events

        public event RegionChangeEventHandler RaiseRegionChangeEvent;

        #endregion

        #region ~
        public RegionChangeAwareService() { }

        #endregion

        #region Public fields

        private string regionNavigationRequestName;
        public string RegionNavigationTargetName
        {
            get { return regionNavigationRequestName; }
            set
            {
                bool isNewValue = SetProperty(ref regionNavigationRequestName, value);
                
                if (isNewValue) 
                    OnRaiseRegionChangeEvent();
            }
        }

        public void Dispose() { }

        #endregion

        #region OnRaise events

        protected virtual void OnRaiseRegionChangeEvent()
        {
            RegionChangeEventHandler raiseEvent = RaiseRegionChangeEvent;
            raiseEvent?.Invoke(this);
        }

        #endregion

    }
}
