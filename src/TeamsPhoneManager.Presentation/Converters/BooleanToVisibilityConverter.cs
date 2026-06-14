using System.Globalization;
using Avalonia.Data.Converters;

namespace teams_phonemanager.Converters
{
    /// <summary>
    /// Converts a boolean value to visibility (bool for IsVisible property in Avalonia).
    /// In Avalonia, IsVisible is a bool property, not a Visibility enum like WPF.
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue;
            }
            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue;
            }
            return false;
        }
    }
}
