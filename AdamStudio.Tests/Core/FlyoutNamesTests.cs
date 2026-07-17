using AdamStudio.Core;
using System.Linq;
using Xunit;

namespace AdamStudio.Tests.Core
{
    /// <summary>
    /// Tests for the Flyout name constants.
    ///
    /// Flyout names must match the view type names because FlyoutManager uses
    /// them as keys when registering and opening flyouts. A mismatch would
    /// cause OpenFlyout to silently do nothing.
    /// </summary>
    public class FlyoutNamesTests
    {
        [Theory]
        [InlineData(FlyoutNames.FlyoutNotification, "NotificationView")]
        [InlineData(FlyoutNames.FlyoutPortSettings, "PortSettingsView")]
        [InlineData(FlyoutNames.FlyoutUserFoldersSettings, "UserFoldersSettingsView")]
        [InlineData(FlyoutNames.FlyoutWebApiSettings, "WebApiSettingsView")]
        public void FlyoutName_MatchesViewName(string flyoutName, string expectedViewName)
        {
            Assert.Equal(expectedViewName, flyoutName);
        }

        [Fact]
        public void AllFlyoutNames_AreNonEmpty()
        {
            Assert.False(string.IsNullOrEmpty(FlyoutNames.FlyoutNotification));
            Assert.False(string.IsNullOrEmpty(FlyoutNames.FlyoutPortSettings));
            Assert.False(string.IsNullOrEmpty(FlyoutNames.FlyoutUserFoldersSettings));
            Assert.False(string.IsNullOrEmpty(FlyoutNames.FlyoutWebApiSettings));
        }

        [Fact]
        public void AllFlyoutNames_AreDistinct()
        {
            string[] allNames =
            [
                FlyoutNames.FlyoutNotification,
                FlyoutNames.FlyoutPortSettings,
                FlyoutNames.FlyoutUserFoldersSettings,
                FlyoutNames.FlyoutWebApiSettings
            ];

            Assert.Equal(allNames.Length, allNames.Distinct().Count());
        }
    }
}
