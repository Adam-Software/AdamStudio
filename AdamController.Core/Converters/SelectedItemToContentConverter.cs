﻿using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace AdamController.Core.Converters
{
    /*public class SelectedItemToContentConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // first value is selected menu item, second value is selected option item
            return values != null && values.Length > 1 ? values[0] ?? values[1] : null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return targetTypes.Select(t => Binding.DoNothing).ToArray();
        }
    }*/
}
