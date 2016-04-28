using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Codify.VisualStudioOnline.BuildLight.UI.Converters
{
    public class BuildStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var status = (Status) value;

            Color colour;

            switch (status)
            {
                case Status.Cancelled:
                    colour = Windows.UI.Colors.Cyan;
                    break;
                case Status.Failed:
                    colour = Windows.UI.Colors.Red;
                    break;
                case Status.InProgress:
                    colour = Windows.UI.Colors.Blue;
                    break;
                case Status.PartiallySucceeded:
                    colour = Windows.UI.Colors.Yellow;
                    break;
                case Status.RetrievalError:
                    colour = Windows.UI.Colors.Purple;
                    break;
                case Status.Succeeded:
                    colour = Windows.UI.Colors.Green;
                    break;
                default:
                    colour = Windows.UI.Colors.Gray;
                    break;
            }

            return new SolidColorBrush(colour) { Opacity = 0.7 };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
