using AdamStudio.Controls.CustomControls.Mvvm.FlyoutContainer;
using AdamStudio.Controls.CustomControls.Services;
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
        private readonly IFlyoutStateChecker mFlyoutState;

        #endregion

        public PortSettingsViewModel(IServiceProvider serviceProvider) 
        {
            BorderThickness = 1;
            mCultureProvider = serviceProvider.GetService<ICultureProvider>();
            mFlyoutState = serviceProvider.GetService<IFlyoutStateChecker>();
        }

        protected override void OnChanging(bool isOpening)
        {
            if (isOpening)
            {
                Header = mCultureProvider.FindResource("PortSettingsView.ViewModel.Flyout.Header");
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
