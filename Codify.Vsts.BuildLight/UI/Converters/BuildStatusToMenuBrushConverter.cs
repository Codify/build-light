using Codify.Vsts.BuildLight.Data;
using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.Foundation;
using System.Collections.Generic;

namespace Codify.Vsts.BuildLight.UI.Converters
{
    public class BuildStatusToMenuBrushConverter : IValueConverter
    {
        private Dictionary<BuildResultStatus, Color> Colours1;
        private Dictionary<BuildResultStatus, Color> Colours2;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Color colour1 = UnknownColour1;
            Color colour2 = UnknownColour2;

            if (Colours1 == null)
            {
                InitialiseColours();
            }

            var status = (BuildResultStatus)value;

            colour1 = CalculateColour(status, Colours1);
            colour2 = CalculateColour(status, Colours2);

            return new LinearGradientBrush()
            {
                StartPoint = new Point(0.0, 0.0),
                EndPoint = new Point(1.0, 1.0),
                GradientStops = new GradientStopCollection()
                {
                    new GradientStop() { Color = colour1, Offset = 0.0 },
                    new GradientStop() { Color = colour2, Offset = 0.8 }
                }
            };
        }

        private Color CalculateColour(BuildResultStatus status, Dictionary<BuildResultStatus, Color> colours)
        {
            Color colour = colours[BuildResultStatus.Unknown];

            switch (status)
            {
                case BuildResultStatus.Cancelled:
                    colour = colours[BuildResultStatus.Cancelled];
                    break;
                case BuildResultStatus.Failed:
                    colour = colours[BuildResultStatus.Failed];
                    break;
                case BuildResultStatus.InProgress:
                    colour = colours[BuildResultStatus.InProgress];
                    break;
                case BuildResultStatus.PartiallySucceeded:
                    colour = colours[BuildResultStatus.PartiallySucceeded];
                    break;
                case BuildResultStatus.RetrievalError:
                    colour = colours[BuildResultStatus.RetrievalError];
                    break;
                case BuildResultStatus.Succeeded:
                    colour = colours[BuildResultStatus.Succeeded];
                    break;
                default:
                    colour = colours[BuildResultStatus.Unknown];
                    break;
            }

            return colour;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        public void InitialiseColours()
        {
            Colours1 = new Dictionary<BuildResultStatus, Color>()
            {
                { BuildResultStatus.Unknown, UnknownColour1 },
                { BuildResultStatus.InProgress, InProgressColour1 },
                { BuildResultStatus.Cancelled, CancelledColour1 },
                { BuildResultStatus.Failed, FailedColour1 },
                { BuildResultStatus.PartiallySucceeded, PartiallySucceededColour1 },
                { BuildResultStatus.RetrievalError, RetrievalErrorColour1 },
                { BuildResultStatus.Succeeded, SucceededColour1 }
            };

            Colours2 = new Dictionary<BuildResultStatus, Color>()
            {
                { BuildResultStatus.Unknown, UnknownColour2 },
                { BuildResultStatus.InProgress, InProgressColour2},
                { BuildResultStatus.Cancelled, CancelledColour2 },
                { BuildResultStatus.Failed, FailedColour2 },
                { BuildResultStatus.PartiallySucceeded, PartiallySucceededColour2 },
                { BuildResultStatus.RetrievalError, RetrievalErrorColour2 },
                { BuildResultStatus.Succeeded, SucceededColour2 }
            };
        }

        public Color UnknownColour1 { get; set; }
        public Color UnknownColour2 { get; set; }

        public Color InProgressColour1 { get; set; }
        public Color InProgressColour2 { get; set; }

        public Color PartiallySucceededColour1 { get; set; }
        public Color PartiallySucceededColour2 { get; set; }

        public Color SucceededColour1 { get; set; }
        public Color SucceededColour2 { get; set; }

        public Color FailedColour1 { get; set; }
        public Color FailedColour2 { get; set; }

        public Color CancelledColour1 { get; set; }
        public Color CancelledColour2 { get; set; }

        public Color RetrievalErrorColour1 { get; set; }
        public Color RetrievalErrorColour2 { get; set; }

    }
}
