using AdamStudio.Services.Interfaces;
using Prism.Mvvm;

namespace AdamStudio.Services
{
    public class RegionChangeAwareService : BindableBase, IRegionChangeAwareService
    {

        public event RegionChangeEventHandler RaiseRegionChangeEvent;

        public RegionChangeAwareService() { }

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

        protected virtual void OnRaiseRegionChangeEvent()
        {
            RegionChangeEventHandler raiseEvent = RaiseRegionChangeEvent;
            raiseEvent?.Invoke(this);
        }

    }
}
