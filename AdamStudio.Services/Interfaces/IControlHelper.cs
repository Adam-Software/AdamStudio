using AdamStudio.Services.ControlHelperServiceDependency;
using System;

namespace AdamStudio.Services
{
    #region Delegates

    public delegate void BlocklyColumnWidthChangeEventHandler(object sender);

    public delegate void IsVideoShowChangeEventHandler(object sender);

    #endregion

    public interface IControlHelper : IDisposable
    {
        #region Events

        public event BlocklyColumnWidthChangeEventHandler RaiseBlocklyColumnWidthChangeEvent;

        public event IsVideoShowChangeEventHandler IsVideoShowChangeEvent;

        #endregion

        public double MainGridActualWidth { get; set; }
        public double BlocklyColumnActualWidth { get; set; }
        public double BlocklyColumnWidth { get; set; }
        public BlocklyViewMode CurrentBlocklyViewMode { get; set; }
        public bool IsShowVideo { get; set; }
    }
}
