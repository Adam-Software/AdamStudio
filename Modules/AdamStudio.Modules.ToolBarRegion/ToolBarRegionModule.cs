using AdamStudio.Core;
using AdamStudio.Modules.ToolBarRegion.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace AdamStudio.Modules.ToolBarRegion
{
    public class ToolBarRegionModule : IModule
    {
        private readonly IRegionManager mRegionManager;

        public ToolBarRegionModule(IRegionManager regionManager) 
        {
            mRegionManager = regionManager;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            mRegionManager.RequestNavigate(RegionNames.ToolBarRegion, nameof(ToolBarView));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ToolBarView>(nameof(ToolBarView));
        }
    }
}