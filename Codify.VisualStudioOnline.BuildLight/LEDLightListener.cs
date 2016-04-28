using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace Codify.VisualStudioOnline.BuildLight
{
    internal class LEDLightListener
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

        internal void Start()
        {
            InitGPIO();
        }

        private void Monitor_StatusChanged(Status status, Guid? correlationId)
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


        private void Monitor_RetrievingStatusStart(Guid? correlationId)
        {
            red.Write(GpioPinValue.High);
            green.Write(GpioPinValue.High);
            blue.Write(GpioPinValue.High);
        }

        private void InitGPIO()
        {
            red = GpioController.GetDefault().OpenPin(_Settings.RedPin);
            red.Write(GpioPinValue.Low);
            red.SetDriveMode(GpioPinDriveMode.Output);

            green = GpioController.GetDefault().OpenPin(_Settings.GreenPin);
            green.Write(GpioPinValue.Low);
            green.SetDriveMode(GpioPinDriveMode.Output);

            blue = GpioController.GetDefault().OpenPin(_Settings.BluePin);
            blue.Write(GpioPinValue.Low);
            blue.SetDriveMode(GpioPinDriveMode.Output);
        }
    }
}
