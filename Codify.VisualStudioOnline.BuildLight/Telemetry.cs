using Microsoft.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codify.VisualStudioOnline.BuildLight
{
    public class Telemetry
    {
        private readonly TelemetryClient _Client;

        public Telemetry()
        {
            _Client = new TelemetryClient();

        }

        public void TrackEvent(string eventName)
        {
            _Client.TrackEvent(eventName);
        }

        public void TrackError(Exception ex)
        {
            _Client.TrackException(ex);
        }
    }
}
