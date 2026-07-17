using AdamStudio.Services.ControlHelperServiceDependency;
using System;

namespace AdamStudio.Services
{



    public interface IControlHelperService : IDisposable
    {

        public event EventHandler RaiseBlocklyColumnWidthChangeEvent;

        public event EventHandler IsVideoShowChangeEvent;

        public double MainGridActualWidth { get; set; }
        public double BlocklyColumnActualWidth { get; set; }
        public double BlocklyColumnWidth { get; set; }
        public BlocklyViewMode CurrentBlocklyViewMode { get; set; }
        public bool IsShowVideo { get; set; }
    }
}
