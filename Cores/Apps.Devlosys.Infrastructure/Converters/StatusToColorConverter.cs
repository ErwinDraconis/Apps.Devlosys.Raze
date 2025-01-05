using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Apps.Devlosys.Infrastructure.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int status)
            {
                switch (status)
                {
                    case 0:
                        return Brushes.Green;
                    case 1: // Part is fail
                        return Brushes.OrangeRed;
                    case 2: // Part is scrap
                        return Brushes.DarkRed;
                    case 10: // Part failed during iTAC or MES booking
                        return Brushes.Azure;
                    default:
                        return Brushes.Yellow;
                }
            }

            // Return a default color if the value isn't valid
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
