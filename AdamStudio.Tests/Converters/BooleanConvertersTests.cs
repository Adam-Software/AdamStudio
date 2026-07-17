using System;
using System.Globalization;
using System.Windows;
using AdamStudio.Core.Converters;
using Xunit;

namespace AdamStudio.Tests.Converters
{
    /// <summary>
    /// Tests for BoolToVisibilityConverter.
    ///
    /// Maps bool? to Visibility:
    ///   true  -> Visibility.Visible
    ///   false -> Visibility.Collapsed
    ///   null  -> Visibility.Collapsed
    ///
    /// ConvertBack is not implemented (one-way converter).
    /// </summary>
    public class BoolToVisibilityConverterTests
    {
        private readonly BoolToVisibilityConverter mConverter = new();

        [Fact]
        public void Convert_True_ReturnsVisible()
        {
            object result = mConverter.Convert(true, typeof(Visibility), null, CultureInfo.InvariantCulture);
            Assert.Equal(Visibility.Visible, result);
        }

        [Fact]
        public void Convert_False_ReturnsCollapsed()
        {
            object result = mConverter.Convert(false, typeof(Visibility), null, CultureInfo.InvariantCulture);
            Assert.Equal(Visibility.Collapsed, result);
        }

        [Fact]
        public void Convert_Null_ReturnsCollapsed()
        {
            object result = mConverter.Convert(null, typeof(Visibility), null, CultureInfo.InvariantCulture);
            Assert.Equal(Visibility.Collapsed, result);
        }

        [Fact]
        public void Convert_Back_ThrowsNotImplemented()
        {
            Assert.Throws<NotImplementedException>(() =>
                mConverter.ConvertBack(Visibility.Visible, typeof(bool?), null, CultureInfo.InvariantCulture));
        }
    }

    /// <summary>
    /// Tests for BoolToVisibilityReConverter — the inverted variant.
    ///
    ///   true  -> Visibility.Collapsed
    ///   false -> Visibility.Visible
    ///   null  -> Visibility.Visible
    /// </summary>
    public class BoolToVisibilityReConverterTests
    {
        private readonly BoolToVisibilityReConverter mConverter = new();

        [Fact]
        public void Convert_True_ReturnsCollapsed()
        {
            object result = mConverter.Convert(true, typeof(Visibility), null, CultureInfo.InvariantCulture);
            Assert.Equal(Visibility.Collapsed, result);
        }

        [Fact]
        public void Convert_False_ReturnsVisible()
        {
            object result = mConverter.Convert(false, typeof(Visibility), null, CultureInfo.InvariantCulture);
            Assert.Equal(Visibility.Visible, result);
        }

        [Fact]
        public void Convert_Null_ReturnsVisible()
        {
            object result = mConverter.Convert(null, typeof(Visibility), null, CultureInfo.InvariantCulture);
            Assert.Equal(Visibility.Visible, result);
        }

        [Fact]
        public void Convert_Back_ThrowsNotImplemented()
        {
            Assert.Throws<NotImplementedException>(() =>
                mConverter.ConvertBack(Visibility.Visible, typeof(bool?), null, CultureInfo.InvariantCulture));
        }
    }

    /// <summary>
    /// Tests for InverseBooleanConverter — bidirectional bool inverter.
    /// </summary>
    public class InverseBooleanConverterTests
    {
        private readonly InverseBooleanConverter mConverter = new();

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void Convert_InvertsBool(bool input, bool expected)
        {
            object result = mConverter.Convert(input, typeof(bool), null, CultureInfo.InvariantCulture);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void ConvertBack_AlsoInvertsBool(bool input, bool expected)
        {
            object result = mConverter.ConvertBack(input, typeof(bool), null, CultureInfo.InvariantCulture);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Convert_NullValue_ThrowsNullReferenceException()
        {
            // The converter casts value to bool without null check. This test
            // documents the current behavior — if the converter is made null-safe
            // in the future, this test should be updated.
            Assert.Throws<NullReferenceException>(() =>
                mConverter.Convert(null, typeof(bool), null, CultureInfo.InvariantCulture));
        }
    }
}
