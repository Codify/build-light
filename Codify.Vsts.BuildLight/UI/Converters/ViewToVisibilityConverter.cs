using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Codify.Vsts.BuildLight.UI.Converters
{
    public class ViewToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var visibility = Visibility.Collapsed;

            if ((value != null) && (parameter != null))
            {
                var view = (AvailableViews)value;

                var expectedView = (AvailableViews)Enum.Parse(typeof(AvailableViews), parameter.ToString());

                visibility = view == expectedView ? Visibility.Visible : Visibility.Collapsed;
            }

            return visibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
