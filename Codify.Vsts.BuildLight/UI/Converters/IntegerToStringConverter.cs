using System;
using Windows.UI.Xaml.Data;

namespace Codify.Vsts.BuildLight.UI.Converters
{
    public class IntegerToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var integerValue = value == null ? 0 : (int)value;
            return integerValue.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return string.IsNullOrWhiteSpace((string)value) ? 0 : int.Parse((string)value);
        }
    }
}
