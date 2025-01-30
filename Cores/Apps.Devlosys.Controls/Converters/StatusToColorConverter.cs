using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Apps.Devlosys.Controls.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = (int)value;
            return status switch
            {
                0  => Brushes.Green,     // OK
                1  => Brushes.OrangeRed, // NOK
                2  => Brushes.DarkRed,   // Scrap
                3  => Brushes.Gray,      // Initial state, pending test
                10 => Brushes.Azure,     // Booking failed
                _  => Brushes.Yellow     // Unknown
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
