using System.Globalization;
using System.Windows.Data;

namespace Project_K.ViewModel.Helpers
{
    public class StringToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? ((double)value).ToString() : string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (double.TryParse(value.ToString(), out double result))
            {
                return result;
            }
            return null; // Default value or handle as needed
        }
    }
}
