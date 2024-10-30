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
            if ((int)value == 0)
                return Brushes.Green;
            else if ((int)value == 1) // Part is fail
                return Brushes.OrangeRed;
            else if((int)value == 2) // Part is scrap
                return Brushes.DarkRed;
            else if ((int)value == 10) // Part failed during iTAC or MES booking
                return Brushes.Azure;
            else
                return Brushes.Yellow;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
