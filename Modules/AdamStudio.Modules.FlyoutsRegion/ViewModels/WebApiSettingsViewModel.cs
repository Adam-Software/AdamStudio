using AdamStudio.Controls.CustomControls.Mvvm.FlyoutContainer;
using AdamStudio.Controls.CustomControls.Services;
using AdamStudio.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace AdamStudio.Modules.FlyoutsRegion.ViewModels
{
    public class WebApiSettingsViewModel : FlyoutBase
    {

        private readonly ICultureProvider mCultureProvider;
        private readonly IFlyoutStateChecker mFlyoutState;

        public WebApiSettingsViewModel(IServiceProvider serviceProvider) 
        {
            mCultureProvider = serviceProvider.GetService<ICultureProvider>();
            mFlyoutState = serviceProvider.GetService<IFlyoutStateChecker>();
            
            BorderThickness = 1;   
        }

        protected override void OnChanging(bool isOpening)
        {
            if(isOpening)
            {
                Header = mCultureProvider.FindResource("WebApiSettingsView.ViewModel.Flyout.Header");
                BorderBrush = Application.Current.TryFindResource("MahApps.Brushes.Text").ToString();
                mFlyoutState.IsFlyoutsOpened = true;
                return;
            }

            if (!isOpening)
            {
                mFlyoutState.IsFlyoutsOpened = false;
                return;
            }
        }

    }
}
