using AdamStudio.Controls.CustomControls.Mvvm.FlyoutContainer;
using System.Windows;

namespace AdamStudio.Controls.CustomControls.Services
{
    public interface IFlyoutManager
    {
        public IRegionManager RegionManager { get; }

        public void SetDefaultFlyoutRegion(string regionName);

        public void RegisterFlyoutWithDefaultRegion<TView>(string flyoutKey) where TView : FrameworkElement;

        public void RegisterFlyoutWithDefaultRegion<TView>(string flyoutKey, IFlyout viewModel) where TView : FrameworkElement;

        public void RegisterFlyout<T>(string flyoutKey, string regionName) where T : FrameworkElement;

        public void RegisterFlyout<T>(string flyoutKey, string regionName, IFlyout flyoutViewModel) where T : FrameworkElement;

        public void UnRegisterFlyout(string key);

        public void UnRegisterFlyout(IFlyout flyout);

        public bool OpenFlyout(string key);

        public bool OpenFlyout(string key, FlyoutParameters flyoutParameters);

        public bool OpenFlyout(string key, bool forceOpen);

        public bool OpenFlyout(string key, FlyoutParameters flyoutParameters, bool forceOpen);

        public bool CloseFlyout(string key);

        public bool CloseFlyout(string key, FlyoutParameters flyoutParameters);

        public bool CloseFlyout(string key, bool forceClose);

        public bool CloseFlyout(string key, FlyoutParameters flyoutParameters, bool forceClose);
    }
}
