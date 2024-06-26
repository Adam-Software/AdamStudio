﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AdamController.Core.Converters
{
    [ValueConversion(typeof(bool?), typeof(Visibility))]
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? booleanNulableValue = null;

            if (value != null)
                booleanNulableValue = (bool)value;
            
            if (booleanNulableValue == true)
                return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Convert only one way");
        }
     
    }
}
