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
                case MessageStatus.Failed:
                case MessageStatus.Error:
                    colour = Colors.DarkRed;
                    break;
                case MessageStatus.InProgress:
                case MessageStatus.PartiallySucceeded:
                case MessageStatus.RetrievalError:
                case MessageStatus.Succeeded:
                    colour = Colors.Silver;
                    break;
                case MessageStatus.Warning:
                    colour = Colors.Orange;
                    break;
                default:
                    colour = Colors.Gray;
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
