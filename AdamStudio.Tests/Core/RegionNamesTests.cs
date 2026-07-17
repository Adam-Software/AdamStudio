using AdamStudio.Core;
using System.Linq;
using Xunit;

namespace AdamStudio.Tests.Core
{
    /// <summary>
    /// Tests for the Prism region name constants.
    ///
    /// These constants are referenced via {x:Static core:RegionNames.MenuRegion}
    /// in XAML and via RegionNames.ContentRegion in code. A typo or rename here
    /// would break XAML bindings at runtime (not compile-time), so these tests
    /// lock the contract.
    /// </summary>
    public class RegionNamesTests
    {
        [Fact]
        public void ContentRegion_IsNonEmptyString()
        {
            Assert.False(string.IsNullOrEmpty(RegionNames.ContentRegion));
        }

        [Fact]
        public void MenuRegion_IsNonEmptyString()
        {
            Assert.False(string.IsNullOrEmpty(RegionNames.MenuRegion));
        }

        [Fact]
        public void StatusBarRegion_IsNonEmptyString()
        {
            Assert.False(string.IsNullOrEmpty(RegionNames.StatusBarRegion));
        }

        [Fact]
        public void ToolBarRegion_IsNonEmptyString()
        {
            Assert.False(string.IsNullOrEmpty(RegionNames.ToolBarRegion));
        }

        [Fact]
        public void FlyoutsRegion_IsNonEmptyString()
        {
            Assert.False(string.IsNullOrEmpty(RegionNames.FlyoutsRegion));
        }

        [Fact]
        public void AllRegions_AreDistinct()
        {
            string[] allRegions =
            [
                RegionNames.ContentRegion,
                RegionNames.MenuRegion,
                RegionNames.StatusBarRegion,
                RegionNames.ToolBarRegion,
                RegionNames.FlyoutsRegion
            ];

            Assert.Equal(allRegions.Length, allRegions.Distinct().Count());
        }

        [Theory]
        [InlineData(RegionNames.ContentRegion, nameof(RegionNames.ContentRegion))]
        [InlineData(RegionNames.MenuRegion, nameof(RegionNames.MenuRegion))]
        [InlineData(RegionNames.StatusBarRegion, nameof(RegionNames.StatusBarRegion))]
        [InlineData(RegionNames.ToolBarRegion, nameof(RegionNames.ToolBarRegion))]
        [InlineData(RegionNames.FlyoutsRegion, nameof(RegionNames.FlyoutsRegion))]
        public void RegionName_MatchesConstantIdentifier(string value, string identifier)
        {
            // The pattern $"${nameof(X)}" means the string value equals the
            // identifier name. This is a convention — verify it holds so XAML
            // {x:Static} bindings and code references stay in sync.
            Assert.Equal(identifier, value);
        }
    }
}
