using AdamStudio.Core;
using Xunit;

namespace AdamStudio.Tests.Core
{
    /// <summary>
    /// Tests for the View name constants.
    ///
    /// These are used with Prism's RegisterForNavigation&lt;TView&gt;() which
    /// registers the view under its type name. The string constants must match
    /// the view type names exactly, or navigation will silently fail.
    /// </summary>
    public class ViewNamesTests
    {
        [Fact]
        public void ScratchView_IsScratchControlView()
        {
            Assert.Equal("ScratchControlView", ViewNames.ScratchView);
        }

        [Fact]
        public void SettingsView_IsSettingsControlView()
        {
            Assert.Equal("SettingsControlView", ViewNames.SettingsView);
        }

        [Fact]
        public void AllViews_AreDistinct()
        {
            Assert.NotEqual(ViewNames.ScratchView, ViewNames.SettingsView);
        }

        [Fact]
        public void AllViews_AreNonEmpty()
        {
            Assert.False(string.IsNullOrEmpty(ViewNames.ScratchView));
            Assert.False(string.IsNullOrEmpty(ViewNames.SettingsView));
        }
    }
}
