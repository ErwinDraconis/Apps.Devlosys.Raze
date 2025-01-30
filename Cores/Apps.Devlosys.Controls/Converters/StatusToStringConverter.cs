using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Apps.Devlosys.Controls.Converters
{
    public class StatusToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is int status)
            {
                return status switch
                {
                    0 => "Ok",
                    1 => "Fail",
                    2 => "Scrap",
                    3 => "Pending judgment",
                    10 => "iTAC or MES Failed",
                    _ => "Unknown"
                };
            }
            return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
