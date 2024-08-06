using AdamStudio.Core.Mvvm;
using AdamStudio.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Prism.Regions;

namespace AdamStudio.Modules.ToolBarRegion.ViewModels
{
    public class ToolBarViewModel : RegionViewModelBase
    {
        private readonly ILogger<ToolBarViewModel> mLogger;
        public ToolBarViewModel(IRegionManager regionManager, ILogger<ToolBarViewModel> logger, ILogWriteEventAwareService logWriteEventAware) : base(regionManager)
        {
            mLogger = logger;

            mLogger.LogTrace("Load ~");

            logWriteEventAware.RaiseNewLogMessageWriteEvent += RaiseNewLogMessageWriteEvent;
        }

        private void RaiseNewLogMessageWriteEvent(object sender, string message)
        {
            Logs += $"{message}\n";
        }

        private string mLogs;
        public string Logs 
        { 
            get => mLogs;
            set => SetProperty(ref mLogs, value);
        }
    }
}
