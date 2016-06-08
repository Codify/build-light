using Codify.Vsts.BuildLight.Data;
using System;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Codify.Vsts.BuildLight.UI.Converters
{
    public class BuildStatusToBackgroundBrushConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return new SolidColorBrush(CalculateColour((BuildResultStatus)value));
        }

        private Color CalculateColour(BuildResultStatus status)
        {
            Color colour = UnknownColour;

            switch (status)
            {
                case BuildResultStatus.Cancelled:
                    colour = CancelledColour;
                    break;
                case BuildResultStatus.Failed:
                    colour = FailedColour;
                    break;
                case BuildResultStatus.InProgress:
                    colour = InProgressColour;
                    break;
                case BuildResultStatus.PartiallySucceeded:
                    colour = PartiallySucceededColour;
                    break;
                case BuildResultStatus.RetrievalError:
                    colour = RetrievalErrorColour;
                    break;
                case BuildResultStatus.Succeeded:
                    colour = SucceededColour;
                    break;
                default:
                    colour = UnknownColour;
                    break;

            }

            return colour;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        public Color UnknownColour { get; set; }

        public Color InProgressColour { get; set; }

        public Color PartiallySucceededColour { get; set; }

        public Color SucceededColour { get; set; }

        public Color FailedColour { get; set; }

        public Color CancelledColour { get; set; }

        public Color RetrievalErrorColour { get; set; }
    }
}
