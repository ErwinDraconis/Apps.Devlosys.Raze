using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Apps.Devlosys.Controls.Converters
{
    public class BoolToVisConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSVGFound)
            {
                // Check the ConverterParameter (this will be either "SVGFound" or "SVGNotFound")
                string isSVGFoundParameter = parameter as string;

                if (isSVGFoundParameter == "SVGFound" && isSVGFound)
                    return Visibility.Visible;

                if (isSVGFoundParameter == "SVGNotFound" && !isSVGFound)
                    return Visibility.Visible;

                return Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


}
