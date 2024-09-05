using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ViewModels.Converter
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool booleanValue = (bool)value;
            if (booleanValue)
            {
                return Visibility.Visible;
            }
            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visibleValue = (Visibility)value;
            if (visibleValue == Visibility.Visible)
            {
                return true;
            }
            return false;
        }
    }
}
