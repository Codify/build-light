using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace Codify.VisualStudioOnline.BuildLight
{
    internal class LEDLightListener : IDisposable
    {
        private GpioPin red;
        private GpioPin green;
        private GpioPin blue;
        private Settings _Settings;

        internal void Subscribe(BuildMonitor monitor, Settings settings)
        {
            monitor.RetrievingStatusStart += Monitor_RetrievingStatusStart;
            monitor.StatusChanged += Monitor_StatusChanged;
            _Settings = settings;
        }

        internal void Unsubscribe(BuildMonitor monitor)
        {
            monitor.RetrievingStatusStart -= Monitor_RetrievingStatusStart;
            monitor.StatusChanged -= Monitor_StatusChanged;
        }

        internal void Start()
        {
            InitGPIO();
        }

        private void Monitor_StatusChanged(Status status, Guid? correlationId)
        {

            try
            {
                switch (status)
                {
                    case Status.Unknown:
                        //purple
                        red.Write(GpioPinValue.High);
                        blue.Write(GpioPinValue.High);
                        green.Write(GpioPinValue.Low);
                        break;
                    case Status.PartiallySucceeded:
                        //yellow
                        red.Write(GpioPinValue.High);
                        blue.Write(GpioPinValue.Low);
                        green.Write(GpioPinValue.High);
                        break;
                    case Status.Succeeded:
                        //green
                        red.Write(GpioPinValue.Low);
                        blue.Write(GpioPinValue.Low);
                        green.Write(GpioPinValue.High);
                        break;
                    case Status.Failed:
                        //red
                        red.Write(GpioPinValue.High);
                        blue.Write(GpioPinValue.Low);
                        green.Write(GpioPinValue.Low);
                        break;
                    case Status.Cancelled:
                        //cyan
                        red.Write(GpioPinValue.Low);
                        blue.Write(GpioPinValue.High);
                        green.Write(GpioPinValue.High);
                        break;
                    case Status.InProgress:
                        //blue
                        red.Write(GpioPinValue.Low);
                        blue.Write(GpioPinValue.High);
                        green.Write(GpioPinValue.Low);
                        break;
                    case Status.RetrievalError:
                        //purple
                        red.Write(GpioPinValue.High);
                        blue.Write(GpioPinValue.High);
                        green.Write(GpioPinValue.Low);
                        break;
                    default:
                        //same as unknown, purple
                        red.Write(GpioPinValue.High);
                        blue.Write(GpioPinValue.High);
                        green.Write(GpioPinValue.Low);
                        break;
                }
            }
            catch(NullReferenceException ex)
            {
                throw new Exception("Invalid pin reference.", ex);
            }
        }


        private void Monitor_RetrievingStatusStart(Guid? correlationId)
        {
            red.Write(GpioPinValue.High);
            green.Write(GpioPinValue.High);
            blue.Write(GpioPinValue.High);
        }

        private void InitGPIO()
        {
            var gpioController = GpioController.GetDefault();

            GpioOpenStatus gpioOpenStatus;

            if (gpioController.TryOpenPin(_Settings.RedPin, GpioSharingMode.Exclusive, out red, out gpioOpenStatus))
            {
                red.Write(GpioPinValue.Low);
                red.SetDriveMode(GpioPinDriveMode.Output);
            }
            else
            {
                throw new Exception("Failed to assign the 'Red' pin.");
            }

            if (gpioController.TryOpenPin(_Settings.GreenPin, GpioSharingMode.Exclusive, out green, out gpioOpenStatus))
            {
                green.Write(GpioPinValue.Low);
                green.SetDriveMode(GpioPinDriveMode.Output);
            }
            else
            {
                throw new Exception("Failed to assign the 'Green' pin.");
            }

            if (gpioController.TryOpenPin(_Settings.BluePin, GpioSharingMode.Exclusive, out blue, out gpioOpenStatus))
            {
                blue.Write(GpioPinValue.Low);
                blue.SetDriveMode(GpioPinDriveMode.Output);
            }
            else
            {
                throw new Exception("Failed to assign the 'Blue' pin.");
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


    }
}
