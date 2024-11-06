using AdamStudio.Core;
using AdamStudio.Core.Mvvm;
using Prism.Commands;
using Prism.Regions;
using System.Windows;
using System;
using AdamStudio.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AdamStudio.Modules.MenuRegion.ViewModels
{
    public class MenuRegionViewModel : RegionViewModelBase
    {
        #region DelegateCommands

        public DelegateCommand CloseAppCommand { get; }    
        public DelegateCommand<string> ShowRegionCommand { get; }

        #endregion

        #region Services

        private readonly IRegionChangeAwareService mRegionChangeAware;

        #endregion

        #region ~

        public MenuRegionViewModel(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            mRegionChangeAware = serviceProvider.GetService<IRegionChangeAwareService>(); 

            CloseAppCommand = new DelegateCommand(CloseApp);
            ShowRegionCommand = new DelegateCommand<string>(ShowRegion);
        }

        #endregion

        #region Navigation

        public override void ConfirmNavigationRequest(NavigationContext navigationContext, Action<bool> continuationCallback)
        {
            base.ConfirmNavigationRequest(navigationContext, continuationCallback);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            Subscribe();

            base.OnNavigatedTo(navigationContext);
        }


        public override void Destroy()
        {
            Unsubscribe();
        }

        #endregion

        #region Public fields

        private bool isCheckedScratchMenuItem;
        public bool IsCheckedScratchMenuItem
        {
            get => isCheckedScratchMenuItem;   
            set => SetProperty(ref isCheckedScratchMenuItem, value);
        }

        private bool isCheckedVisualSettingsMenuItem;
        public bool IsCheckedVisualSettingsMenuItem
        {
            get => isCheckedVisualSettingsMenuItem;
            set => SetProperty(ref isCheckedVisualSettingsMenuItem, value);
        }

        #endregion

        #region Private methods

        private void ChangeCheckedMenuItem(string selectedRegionName)
        {
            ResetIsCheckedMenuItem();

            switch (selectedRegionName)
            {
                case ViewNames.ScratchView:
                    IsCheckedScratchMenuItem = true;
                    break;
                case ViewNames.SettingsView:    
                    IsCheckedVisualSettingsMenuItem = true;
                    break;
            }
        }

        private void ResetIsCheckedMenuItem()
        {
            IsCheckedScratchMenuItem = false;
            IsCheckedVisualSettingsMenuItem = false;
        }

        #endregion

        #region Subscription

        private void Subscribe()
        {
            mRegionChangeAware.RaiseRegionChangeEvent += RaiseSubRegionChangeEvent;
        }

        private void Unsubscribe() 
        {
            mRegionChangeAware.RaiseRegionChangeEvent -= RaiseSubRegionChangeEvent;
        }

        #endregion

        #region Event methods

        private void RaiseSubRegionChangeEvent(object sender)
        {
            ChangeCheckedMenuItem(mRegionChangeAware.RegionNavigationTargetName);
        }

        #endregion

        #region Command methods

        private void ShowRegion(string regionName)
        {
            RegionManager.RequestNavigate(RegionNames.ContentRegion, regionName);
        }

        private void CloseApp()
        {
            Application.Current.Shutdown();
        }

        #endregion
    }
}
