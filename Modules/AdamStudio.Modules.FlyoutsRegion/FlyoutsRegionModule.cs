using AdamStudio.Controls.CustomControls.Services;
using AdamStudio.Core;
using AdamStudio.Modules.FlyoutsRegion.Views;
using Prism.Ioc;
using Prism.Modularity;

namespace AdamStudio.Modules.FlyoutsRegion
{
    public class FlyoutsRegionModule : IModule
    {
        private readonly IFlyoutManager mFlyoutManager;

        public FlyoutsRegionModule(IFlyoutManager flyoutManager)
        {
            mFlyoutManager = flyoutManager;
        }

        public void OnInitialized(IContainerProvider containerProvider){}

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            mFlyoutManager.SetDefaultFlyoutRegion(RegionNames.FlyoutsRegion);

            mFlyoutManager.RegisterFlyoutWithDefaultRegion<NotificationView>(FlyoutNames.FlyoutNotification);
            mFlyoutManager.RegisterFlyoutWithDefaultRegion<PortSettingsView>(FlyoutNames.FlyoutPortSettings);
            mFlyoutManager.RegisterFlyoutWithDefaultRegion<UserFoldersSettingsView>(FlyoutNames.FlyoutUserFoldersSettings);
            mFlyoutManager.RegisterFlyoutWithDefaultRegion<WebApiSettingsView>(FlyoutNames.FlyoutWebApiSettings);
        }
    }
}