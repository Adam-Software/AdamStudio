using AdamStudio.Controls.CustomControls.Mvvm.FlyoutContainer;
using AdamStudio.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace AdamStudio.Modules.FlayoutsRegion.ViewModels
{
    public class PortSettingsViewModel : FlyoutBase
    {

        #region Services

        private readonly ICultureProvider mCultureProvider;

        #endregion

        public PortSettingsViewModel(IServiceProvider serviceProvider) 
        {
            BorderThickness = 1;
            mCultureProvider = serviceProvider.GetService<ICultureProvider>();
        }

        protected override void OnChanging(bool isOpening)
        {
            if (isOpening)
            {
                Header = mCultureProvider.FindResource("PortSettingsView.ViewModel.Flyout.Header");
                BorderBrush = Application.Current.TryFindResource("MahApps.Brushes.Text").ToString();
                return;
            }
        }
    }
}
