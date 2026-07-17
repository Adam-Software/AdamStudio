using AdamStudio.Services.Interfaces;
using Prism.Mvvm;
using System;

namespace AdamStudio.Services
{
    public class RegionChangeAwareService : BindableBase, IRegionChangeAwareService
    {
        public event EventHandler RaiseRegionChangeEvent;

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
            
            RaiseRegionChangeEvent?.Invoke(this, EventArgs.Empty);
        }

    }
}
