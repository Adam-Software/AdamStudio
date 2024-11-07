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
        #region Events

        public event CurrentAppCultureLoadOrChangeEventHandler RaiseCurrentAppCultureLoadOrChangeEvent;

        #endregion

        #region Const

        private const string cEnString = "en-EN";
        private const string cRuString = "ru-RU";

        #endregion

        #region Var

        private readonly Application mCurrentApp = Application.Current;

        #endregion

        #region ~

        public CultureProvider() {}

        #endregion

        #region Public fields

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

        #endregion

        #region Public methods

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

        #endregion

        #region Private method

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

        #endregion

        #region OnRaise events

        protected virtual void OnRaiseCurrentAppCultureLoadOrChangeEvent()
        {
            CurrentAppCultureLoadOrChangeEventHandler raiseEvent = RaiseCurrentAppCultureLoadOrChangeEvent;
            raiseEvent?.Invoke(this);
        }

        #endregion
    }
}
