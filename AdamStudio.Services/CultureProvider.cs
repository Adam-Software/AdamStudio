using AdamStudio.Services.Interfaces;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;

namespace AdamStudio.Services
{
    public class CultureProvider : BindableBase, ICultureProvider
    {
        public event EventHandler RaiseCurrentAppCultureLoadOrChangeEvent;

        private const string cEnString = "en-EN";
        private const string cRuString = "ru-RU";

        private readonly Application mCurrentApp = Application.Current;

        public CultureProvider() {}

        public List<CultureInfo> SupportAppCultures { get { return GetSupportAppCultures(); } }

        private CultureInfo currentAppCulture;

        public CultureInfo CurrentAppCulture 
        {  
            get => currentAppCulture;
            private set
            {
                bool isNewValue = SetProperty(ref currentAppCulture, value);

                if (isNewValue)
                    OnRaiseCurrentAppCultureLoadOrChangeEvent();
            }
            
        }

        public void ChangeAppCulture(CultureInfo culture)
        {
            string resourceName = $"pack://application:,,,/AdamStudio.Core;component/LocalizationDictionary/{culture.TwoLetterISOLanguageName}.xaml";
            Uri uri = new(resourceName);
            ResourceDictionary resources = new()
            {
                Source = uri
            };

            RemoveLoadedDictonary();

            mCurrentApp.Resources.MergedDictionaries.Add(resources);
            UpdateCurrentCulture(culture);
        }

        public void Dispose()
        {

        }

        public string FindResource(string resource)
        {
            var @string = mCurrentApp.TryFindResource(resource) as string;
            return @string;
        }

        private void UpdateCurrentCulture(CultureInfo culture)
        {
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            CurrentAppCulture = culture;
        }

        private void RemoveLoadedDictonary()
        {
            List<CultureInfo> supportedCultures = SupportAppCultures;
            ResourceDictionary currentResourceDictionary = null;

            foreach (var culture in supportedCultures)
            {
                string resourceName = $"pack://application:,,,/AdamController.Core;component/LocalizationDictionary/{culture.TwoLetterISOLanguageName}.xaml"; 
                currentResourceDictionary = mCurrentApp.Resources.MergedDictionaries.FirstOrDefault(x => x?.Source?.OriginalString == resourceName);
            }

            if (currentResourceDictionary == null || currentResourceDictionary?.MergedDictionaries.Count == 0) 
                return;
           
            foreach (ResourceDictionary dictionary in currentResourceDictionary.MergedDictionaries)
                mCurrentApp.Resources.MergedDictionaries.Remove(dictionary);
        }

        private static List<CultureInfo> GetSupportAppCultures()
        {
            CultureInfo en = new(cEnString);
            CultureInfo ru = new(cRuString);

            List<CultureInfo> cultureInfos =
            [
                ru, en
            ];

            return cultureInfos;
        }

        protected virtual void OnRaiseCurrentAppCultureLoadOrChangeEvent()
        {
            
            RaiseCurrentAppCultureLoadOrChangeEvent?.Invoke(this, EventArgs.Empty);
        }

    }
}
