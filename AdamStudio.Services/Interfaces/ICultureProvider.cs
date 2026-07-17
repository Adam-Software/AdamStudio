using System;
using System.Collections.Generic;
using System.Globalization;

namespace AdamStudio.Services.Interfaces
{


    public interface ICultureProvider : IDisposable
    {

        public event EventHandler RaiseCurrentAppCultureLoadOrChangeEvent;

        public List<CultureInfo> SupportAppCultures { get; }
        public CultureInfo CurrentAppCulture { get; }

        public string FindResource(string resourcePath);
        public void ChangeAppCulture(CultureInfo culture);

    }
}
