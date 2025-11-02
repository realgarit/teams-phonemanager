using System;
using System.Globalization;
using System.Windows.Data;

namespace teams_phonemanager.Converters
{
    public class TimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan timeSpan)
            {
                return timeSpan.ToString(@"hh\:mm");
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
            {
                if (TimeSpan.TryParseExact(stringValue, @"hh\:mm", culture, out TimeSpan result))
                {
                    return result;
                }
                // Try parsing with different formats
                if (TimeSpan.TryParse(stringValue, out result))
                {
                    return result;
                }
            }
            return TimeSpan.Zero;
        }
    }
}
