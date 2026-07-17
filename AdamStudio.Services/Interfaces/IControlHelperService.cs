using AdamStudio.Services.ControlHelperServiceDependency;
using System;

namespace AdamStudio.Services
{

    public delegate void BlocklyColumnWidthChangeEventHandler(object sender);

    public delegate void IsVideoShowChangeEventHandler(object sender);

    public interface IControlHelperService : IDisposable
    {

        public event BlocklyColumnWidthChangeEventHandler RaiseBlocklyColumnWidthChangeEvent;

        public event IsVideoShowChangeEventHandler IsVideoShowChangeEvent;

        public double MainGridActualWidth { get; set; }
        public double BlocklyColumnActualWidth { get; set; }
        public double BlocklyColumnWidth { get; set; }
        public BlocklyViewMode CurrentBlocklyViewMode { get; set; }
        public bool IsShowVideo { get; set; }
    }
}
