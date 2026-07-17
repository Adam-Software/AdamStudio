using AdamStudio.Core;
using AdamStudio.Modules.SettingsRegion.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace AdamStudio.Modules.SettingsRegion
{
    public class SettingsRegionModule : IModule
    {
        private readonly IRegionManager mRegionManager;
        
        public SettingsRegionModule(IRegionManager regionManager) 
        {
            mRegionManager = regionManager;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            //mRegionManager.RequestNavigate(RegionNames.ContentRegion, nameof(SettingsControlView));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<SettingsControlView>(nameof(SettingsControlView));
        }
    }
}
