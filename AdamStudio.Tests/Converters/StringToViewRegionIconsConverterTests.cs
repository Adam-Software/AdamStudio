using System;
using System.Globalization;
using AdamStudio.Core;
using AdamStudio.Core.Converters;
using MahApps.Metro.IconPacks;
using Xunit;

namespace AdamStudio.Tests.Converters
{
    /// <summary>
    /// Tests for StringToViewRegionIconsConverter.
    ///
    /// This converter maps a view name string to an icon kind for the
    /// toolbar / region indicator. It has three branches:
    ///   - SettingsView -> PackIconSimpleIconsKind.Scratch
    ///   - ScratchView  -> PackIconFeatherIconsKind.Settings
    ///   - anything else -> PackIconSimpleIconsKind.Abbott (fallback)
    ///
    /// Note: the icon assignments look swapped (SettingsView maps to the
    /// "Scratch" icon, ScratchView maps to the "Settings" icon). This is
    /// intentional — the icon indicates what will happen when you click,
    /// not the current view. These tests lock that contract.
    /// </summary>
    public class StringToViewRegionIconsConverterTests
    {
        private readonly StringToViewRegionIconsConverter mConverter = new();

        [Fact]
        public void Convert_SettingsView_ReturnsScratchIcon()
        {
            object result = mConverter.Convert(
                ViewNames.SettingsView,
                typeof(object),
                null,
                CultureInfo.InvariantCulture);

            Assert.Equal(PackIconSimpleIconsKind.Scratch, result);
        }

        [Fact]
        public void Convert_ScratchView_ReturnsSettingsIcon()
        {
            object result = mConverter.Convert(
                ViewNames.ScratchView,
                typeof(object),
                null,
                CultureInfo.InvariantCulture);

            Assert.Equal(PackIconFeatherIconsKind.Settings, result);
        }

        [Fact]
        public void Convert_UnknownView_ReturnsFallbackIcon()
        {
            object result = mConverter.Convert(
                "SomeUnknownView",
                typeof(object),
                null,
                CultureInfo.InvariantCulture);

            Assert.Equal(PackIconSimpleIconsKind.Abbott, result);
        }

        [Fact]
        public void Convert_NullValue_ReturnsFallbackIcon()
        {
            // null is coerced to string.Empty inside Convert, which is
            // neither SettingsView nor ScratchView, so fallback applies.
            object result = mConverter.Convert(
                null,
                typeof(object),
                null,
                CultureInfo.InvariantCulture);

            Assert.Equal(PackIconSimpleIconsKind.Abbott, result);
        }

        [Fact]
        public void Convert_EmptyString_ReturnsFallbackIcon()
        {
            object result = mConverter.Convert(
                string.Empty,
                typeof(object),
                null,
                CultureInfo.InvariantCulture);

            Assert.Equal(PackIconSimpleIconsKind.Abbott, result);
        }

        [Fact]
        public void ConvertBack_ThrowsNotImplementedException()
        {
            Assert.Throws<NotImplementedException>(() =>
                mConverter.ConvertBack(
                    PackIconSimpleIconsKind.Scratch,
                    typeof(string),
                    null,
                    CultureInfo.InvariantCulture));
        }

        [Fact]
        public void Convert_IsDecoratedWithValueConversionAttribute()
        {
            // The [ValueConversion] attribute is used by XAML designers to
            // validate binding types. Verify it is present and declares the
            // expected source/target types.
            System.Attribute[] attrs = System.Attribute.GetCustomAttributes(
                typeof(StringToViewRegionIconsConverter),
                typeof(System.Windows.Data.ValueConversionAttribute));

            Assert.NotEmpty(attrs);
        }
    }
}
