using AdamStudio.Core.Mvvm;
using Microsoft.Extensions.Logging;
using Prism.Regions;

namespace AdamStudio.Modules.ToolBarRegion.ViewModels
{
    public class ToolBarViewModel : RegionViewModelBase
    {
        private readonly ILogger<ToolBarViewModel> mLogger;
        public ToolBarViewModel(IRegionManager regionManager, ILogger<ToolBarViewModel> logger) : base(regionManager)
        {
            mLogger = logger;

            mLogger.LogTrace("Load ~");
        }
    }
}
