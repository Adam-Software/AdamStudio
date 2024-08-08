using System;
using System.Collections.Generic;
using System.Globalization;

namespace AdamStudio.Services.Interfaces
{
    #region Delegates

    public delegate void CurrentAppCultureLoadOrChangeEventHandler(object sender);

    #endregion

    public interface ICultureProvider : IDisposable
    {
        #region Events

        public event CurrentAppCultureLoadOrChangeEventHandler RaiseCurrentAppCultureLoadOrChangeEvent;

        #endregion

        #region Public fields

        public List<CultureInfo> SupportAppCultures { get; }
        public CultureInfo CurrentAppCulture { get; }

        #endregion

        #region Public methods
        public string FindResource(string resourcePath);
        public void ChangeAppCulture(CultureInfo culture);

        #endregion
    }
}
