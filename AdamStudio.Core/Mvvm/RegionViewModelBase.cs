using Microsoft.Extensions.DependencyInjection;
using Prism.Navigation.Regions;
using System;

namespace AdamStudio.Core.Mvvm
{
    public class RegionViewModelBase : ViewModelBase, INavigationAware, IConfirmNavigationRequest
    {
        

        protected IRegionManager RegionManager { get; }

        public RegionViewModelBase(IServiceProvider serviceProvider)
        {
            RegionManager = serviceProvider.GetService<IRegionManager>();
        }

        /// <summary>
        /// Occurs when the navigation area is called
        /// </summary>
        public virtual void ConfirmNavigationRequest(NavigationContext navigationContext, Action<bool> continuationCallback)
        {
            continuationCallback?.Invoke(true);
        }

        
        public virtual bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        /// <summary>
        /// On close region
        /// </summary>
        public virtual void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        /// <summary>
        /// On load region
        /// </summary>
        public virtual void OnNavigatedTo(NavigationContext navigationContext)
        {

        }
    }
}
