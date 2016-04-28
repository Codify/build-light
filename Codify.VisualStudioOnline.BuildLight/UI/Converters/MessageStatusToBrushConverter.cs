using System;
using System.Collections.Generic;
using System.Linq;

using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Codify.VisualStudioOnline.BuildLight.UI.Converters
{
    public class MessageStatusToBrushConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var status = (MessageStatus)value;

            Color colour;

            switch (status)
            {
                case MessageStatus.Cancelled:
                    colour = Windows.UI.Colors.Cyan;
                    break;
                case MessageStatus.Failed:
                    colour = Windows.UI.Colors.Red;
                    break;
                case MessageStatus.InProgress:
                     colour = Windows.UI.Colors.Blue;
                    break;
                case MessageStatus.PartiallySucceeded:
                    colour = Windows.UI.Colors.Yellow;
                    break;
                case MessageStatus.RetrievalError:
                    colour = Windows.UI.Colors.Purple;
                    break;
                case MessageStatus.Succeeded:
                    colour = Windows.UI.Colors.Green;
                    break;
                case MessageStatus.Warning:
                case MessageStatus.Error:
                    break;
                default:
                    colour = Windows.UI.Colors.Silver;
                    break;
            }

            return new SolidColorBrush(colour);
        }


        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
