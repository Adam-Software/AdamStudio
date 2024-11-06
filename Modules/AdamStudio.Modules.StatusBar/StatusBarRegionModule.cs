using AdamStudio.Core;
using AdamStudio.Modules.StatusBarRegion.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace AdamStudio.Modules.StatusBarRegion
{
    public class StatusBarRegionModule : IModule
    {
        private readonly IRegionManager mRegionManager;

        public StatusBarRegionModule(IRegionManager regionManager)
        {
            mRegionManager = regionManager;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            mRegionManager.RequestNavigate(RegionNames.StatusBarRegion, nameof(StatusBarView));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<StatusBarView>(nameof(StatusBarView));
        }
    }
}