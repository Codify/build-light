using Codify.Vsts.BuildLight.Models;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Codify.Vsts.BuildLight.UI.Converters
{
    public class BuildCheckScaleToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value == null ? BuildCheckScale.Seconds.ToString() : ((BuildCheckScale)value).ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return string.IsNullOrWhiteSpace((string)value) ? BuildCheckScale.Seconds : Enum.Parse(typeof(BuildCheckScale), (string)value);
        }
    }
}
