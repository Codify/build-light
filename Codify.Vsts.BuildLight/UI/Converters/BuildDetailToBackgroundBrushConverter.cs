using Codify.Vsts.BuildLight.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Codify.Vsts.BuildLight.Data;

namespace Codify.Vsts.BuildLight.UI.Converters
{
    public class BuildDetailToBackgroundBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var detail = value as BuildDetails;

            Color colour1 = UnknownColour;
            Color colour2 = UnknownColour;

            if (detail != null)
            {
                //colour1 = CalculateColour(detail.PreviousBuild);
                colour1 = colour2 = CalculateColour(detail.CurrentBuild);
            }

            return new LinearGradientBrush()
            {
                StartPoint = new Windows.Foundation.Point(0, 0),
                EndPoint = new Windows.Foundation.Point(1, 1),
                GradientStops = new GradientStopCollection()
                {
                    new GradientStop() { Offset = 0.0, Color = colour1 },
                    new GradientStop() { Offset = 0.4, Color = colour2 },
                }
            };
        }

        private Color CalculateColour(BuildInstance instance)
        {
            Color colour = UnknownColour;

            if (instance == null)
            {
                colour = UnknownColour;
            }
            else if (instance.ProgressStatus != BuildProgressStatus.Completed)
            {
                colour = InProgressColour;
            }
            else
            {
                colour = CalculateColour(instance.ResultStatus);
            }

            return colour;
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
