﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace Apps.Devlosys.Infrastructure.Converters
{
    public class CheckEnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value == parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
