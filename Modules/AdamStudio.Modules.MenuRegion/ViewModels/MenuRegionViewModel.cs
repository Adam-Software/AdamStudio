using AdamStudio.Core;
using AdamStudio.Core.Mvvm;
using AdamStudio.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Prism.Commands;
using Prism.Navigation.Regions;
using System;
using System.Windows;

namespace AdamStudio.Modules.MenuRegion.ViewModels
{
    public class MenuRegionViewModel : RegionViewModelBase
    {

        public DelegateCommand CloseAppCommand { get; }    
        public DelegateCommand<string> ShowRegionCommand { get; }

        private readonly IRegionChangeAwareService mRegionChangeAware;

        public MenuRegionViewModel(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            mRegionChangeAware = serviceProvider.GetService<IRegionChangeAwareService>(); 

            CloseAppCommand = new DelegateCommand(CloseApp);
            ShowRegionCommand = new DelegateCommand<string>(ShowRegion);
        }

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

        private void Subscribe()
        {
            mRegionChangeAware.RaiseRegionChangeEvent += RaiseSubRegionChangeEvent;
        }

        private void Unsubscribe() 
        {
            mRegionChangeAware.RaiseRegionChangeEvent -= RaiseSubRegionChangeEvent;
        }

        private void RaiseSubRegionChangeEvent(object sender)
        {
            ChangeCheckedMenuItem(mRegionChangeAware.RegionNavigationTargetName);
        }

        private void ShowRegion(string regionName)
        {
            RegionManager.RequestNavigate(RegionNames.ContentRegion, regionName);
        }

        private void CloseApp()
        {
            Application.Current.Shutdown();
        }

    }
}
