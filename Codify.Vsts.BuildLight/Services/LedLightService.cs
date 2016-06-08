using Codify.Vsts.BuildLight.Models;
using Codify.Vsts.BuildLight.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Codify.Vsts.BuildLight.Data;
using Windows.UI;
using Microsoft.ApplicationInsights;

namespace Codify.Vsts.BuildLight.Services
{
    internal class LedLightService : IDisposable
    {

        private GpioPin red;

        private GpioPin green;

        private GpioPin blue;

        internal LedLightService(Settings settings, BuildService buildService)
        {
            Settings = settings;
            Telemetry = new TelemetryClient();
            BuildService = buildService;

            Initialise();
        }

        #region Properties

        private TelemetryClient Telemetry { get; set; }

        private Dictionary<Color, LedColour> AvailableColours { get; set; }

        private BuildService BuildService { get; set; }

        public Settings Settings { get; set; }

        #endregion

        private void BuildService_BuildEvent(object sender, BuildEventArgs e)
        {
            switch (e.Code)
            {
                case BuildEventCode.Error:
                    SetLight(AvailableColours[Colors.Red]);
                    break;
            }
        }

        private void ShowCombinedBuildStatus()
        {
            switch (BuildService.ServiceStatus)
            {
                case BuildResultStatus.Cancelled:
                    SetLight(AvailableColours[Colors.Cyan]);
                    break;
                case BuildResultStatus.Failed:
                    SetLight(AvailableColours[Colors.Red]);
                    break;
                case BuildResultStatus.InProgress:
                    SetLight(AvailableColours[Colors.Blue]);
                    break;
                case BuildResultStatus.PartiallySucceeded:
                    SetLight(AvailableColours[Colors.Yellow]);
                    break;
                case BuildResultStatus.RetrievalError:
                    SetLight(AvailableColours[Colors.Purple]);
                    break;
                case BuildResultStatus.Succeeded:
                    SetLight(AvailableColours[Colors.Green]);
                    break;
                default:
                    SetLight(AvailableColours[Colors.Purple]);
                    break;
            }
        }

        private void BuildService_ServiceEvent(object sender, BuildEventArgs e)
        {
            switch (e.Code)
            {
                case BuildEventCode.ServiceStart:
                    SetLight(AvailableColours[Colors.Purple]);
                    break;
                case BuildEventCode.BuildInformationRetrievalStart:
                    SetLight(AvailableColours[Colors.White]);
                    break;
                case BuildEventCode.Error:
                    SetLight(AvailableColours[Colors.Red]);
                    break;
                case BuildEventCode.NoBuildInformationAvailable:
                case BuildEventCode.NoBuildsFound:
                    SetLight(AvailableColours[Colors.Purple]);
                    break;
                case BuildEventCode.BuildInformationRetrievalEnd:
                    ShowCombinedBuildStatus();
                    break;
                case BuildEventCode.ServiceEnd:
                    SetLight(AvailableColours[Colors.Black]);
                    break;
            }
        }

        private void SetLight(LedColour ledColour)
        {
            if (this.red != null)
            {
                this.red.Write(ledColour.Red);
            }
            if (this.green != null)
            {
                this.green.Write(ledColour.Green);
            }
            if (this.blue != null)
            {
                this.blue.Write(ledColour.Blue);
            }
        }

        private void Initialise()
        {
            var gpioController = GpioController.GetDefault();
            if (gpioController != null)
            {
                AvailableColours = new Dictionary<Color, LedColour>()
                {
                    { Colors.Black, new LedColour(GpioPinValue.Low, GpioPinValue.Low, GpioPinValue.Low) },
                    { Colors.Red, new LedColour(GpioPinValue.High, GpioPinValue.Low, GpioPinValue.Low) },
                    { Colors.Green, new LedColour(GpioPinValue.Low, GpioPinValue.High, GpioPinValue.Low) },
                    { Colors.Blue, new LedColour(GpioPinValue.Low, GpioPinValue.Low, GpioPinValue.High) },
                    { Colors.Cyan, new LedColour(GpioPinValue.Low, GpioPinValue.High, GpioPinValue.High) },
                    { Colors.Purple, new LedColour(GpioPinValue.High, GpioPinValue.Low, GpioPinValue.High) },
                    { Colors.Yellow, new LedColour(GpioPinValue.High, GpioPinValue.High, GpioPinValue.Low) },
                    { Colors.White, new LedColour(GpioPinValue.High, GpioPinValue.High, GpioPinValue.High) }
                };


                GpioOpenStatus gpioOpenStatus;

                if (gpioController.TryOpenPin(Settings.RedPin, GpioSharingMode.Exclusive, out red, out gpioOpenStatus))
                {
                    red.Write(GpioPinValue.Low);
                    red.SetDriveMode(GpioPinDriveMode.Output);
                }
                else
                {
                    throw new Exception("Failed to assign the 'Red' pin.");
                }

                if (gpioController.TryOpenPin(Settings.GreenPin, GpioSharingMode.Exclusive, out green, out gpioOpenStatus))
                {
                    green.Write(GpioPinValue.Low);
                    green.SetDriveMode(GpioPinDriveMode.Output);
                }
                else
                {
                    throw new Exception("Failed to assign the 'Green' pin.");
                }

                if (gpioController.TryOpenPin(Settings.BluePin, GpioSharingMode.Exclusive, out blue, out gpioOpenStatus))
                {
                    blue.Write(GpioPinValue.Low);
                    blue.SetDriveMode(GpioPinDriveMode.Output);
                }
                else
                {
                    throw new Exception("Failed to assign the 'Blue' pin.");
                }

                if (BuildService != null)
                {
                    BuildService.ServiceEvent += BuildService_ServiceEvent;
                    BuildService.BuildEvent += BuildService_BuildEvent;

                    ShowCombinedBuildStatus();
                }
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (red != null)
                    {
                        red.Dispose();
                        red = null;
                    }
                    if (green != null)
                    {
                        green.Dispose();
                        green = null;
                    }
                    if (blue != null)
                    {
                        blue.Dispose();
                        blue = null;
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~LEDLightListener() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

        #region LedColour class

        private class LedColour
        {
            public LedColour(GpioPinValue red, GpioPinValue green, GpioPinValue blue)
            {
                Red = red;
                Green = green;
                Blue = blue;
            }

            public GpioPinValue Red { get; set; }

            public GpioPinValue Green { get; set; }

            public GpioPinValue Blue { get; set; }
        }

        #endregion
    }
}
