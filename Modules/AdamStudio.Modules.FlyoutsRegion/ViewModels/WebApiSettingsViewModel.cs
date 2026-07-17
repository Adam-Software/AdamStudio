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
        #region Services

        private readonly ICultureProvider mCultureProvider;
        private readonly IFlyoutStateChecker mFlyoutState;

        #endregion

        #region ~

        public WebApiSettingsViewModel(IServiceProvider serviceProvider) 
        {
            mCultureProvider = serviceProvider.GetService<ICultureProvider>();
            mFlyoutState = serviceProvider.GetService<IFlyoutStateChecker>();
            
            BorderThickness = 1;   
        }

        #endregion

        #region Navigation

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

        #endregion
    }
}
