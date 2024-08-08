using MahApps.Metro.IconPacks;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AdamStudio.Core.Converters
{
    [ValueConversion(typeof(bool?), typeof(Visibility))]
    public class StringToViewRegionIconsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string stringValue = string.Empty;

            if (value != null)
                stringValue = (string)value;

            if (stringValue == RegionNames.SettingsRegion)
                return PackIconSimpleIconsKind.Scratch;

            if (stringValue == RegionNames.ScratchRegion)
                return PackIconFeatherIconsKind.Settings;

            return PackIconSimpleIconsKind.AbbRobotStudio;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Convert only one way");
        }
     
    }
}
