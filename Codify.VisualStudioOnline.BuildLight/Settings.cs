using Codify.VisualStudioOnline.BuildLight.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codify.VisualStudioOnline.BuildLight
{

    public enum LogVerbosity
    {
        Minimal,
        Normal
    }

    public class Settings : NotifyPropertyChanged
    {
        public static int DefaultRedPinSetting = 18;
        public static int DefaultGreenPinSetting = 22;
        public static int DefaultBluePinSettings = 24;

        public Settings()
        {
            RedPin = DefaultRedPinSetting;
            GreenPin = DefaultGreenPinSetting;
            BluePin = DefaultBluePinSettings;

            LogVerbosity = LogVerbosity.Normal;
        }

        [JsonProperty("account")]
        public string Account { get { return GetValue<string>(); } set { SetValue(value); } }

        [JsonIgnore]
        public string Username { get { return GetValue<string>(); } set { SetValue(value); } }

        [JsonIgnore]
        public string Password { get { return GetValue<string>(); } set { SetValue(value); } }

        [JsonProperty("project")]
        public string Project { get { return GetValue<string>(); } set { SetValue(value); } }

        [JsonProperty("buildName")]
        public string BuildName { get { return GetValue<string>(); } set { SetValue(value); } }

        [JsonProperty("showLogs")]
        public bool ShowLogs { get { return GetValue<bool>(); } set { SetValue(value); } }

        [JsonProperty("logVerbosity")]
        public LogVerbosity LogVerbosity { get { return GetValue<LogVerbosity>(); } set { SetValue(value); } }

        [JsonProperty("redPin")]
        public int RedPin { get { return GetValue<int>(); } set { SetValue(value); } }

        [JsonProperty("greenPin")]
        public int GreenPin { get { return GetValue<int>(); } set { SetValue(value); } }

        [JsonProperty("bluePin")]
        public int BluePin { get { return GetValue<int>(); } set { SetValue(value); } }

        [JsonIgnore]
        public string RedPinValue { get { return GetValue<string>(); } set { SetValue(value); } }

        [JsonIgnore]
        public string GreenPinValue { get { return GetValue<string>(); } set { SetValue(value); } }

        [JsonIgnore]
        public string BluePinValue { get { return GetValue<string>(); } set { SetValue(value); } }

        public string RedPinError { get { return GetValue<string>(); } set { SetValue(value); } }

        public string GreenPinError { get { return GetValue<string>(); } set { SetValue(value); } }

        public string BluePinError { get { return GetValue<string>(); } set { SetValue(value); } }

        [JsonIgnore]
        public bool CanConnectTo { get { return !string.IsNullOrWhiteSpace(Account) && !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password); } }

        [JsonIgnore]
        public bool IsLoaded { get { return CanConnectTo && !string.IsNullOrWhiteSpace(Project) && !string.IsNullOrWhiteSpace(BuildName); } }

        public void ConvertPinValueToPin(string pinValue, int pin, Action<int> updatePin)
        {
            int pinIntValue = 0;
            int.TryParse(pinValue, out pinIntValue);
            if (pinIntValue != pin)
            {
                updatePin.Invoke(pinIntValue);
            }
        }

        public void ConvertPinToPinValue(int pin, string pinValue, Action<string> updatePinValue)
        {
            if (pinValue != pin.ToString())
            {
                updatePinValue.Invoke(pin.ToString());
            }
        }

        private void ValidatePinUniqueness()
        {
            RedPinError = ((RedPin == GreenPin) || (RedPin == BluePin)) ? "The 'Red' pin is not unique." : string.Empty;
            GreenPinError = ((GreenPin == RedPin) || (GreenPin == BluePin)) ? "The 'Green' pin is not unique." : string.Empty;
            BluePinError = ((BluePin == GreenPin) || (BluePin == RedPin)) ? "The 'Blue' pin is not unique." : string.Empty;
        }

        public async override Task OnPropertyChanged(string propertyName)
        {
            await base.OnPropertyChanged(propertyName);

            if (propertyName.Equals("RedPinValue"))
            {
                ConvertPinValueToPin(RedPinValue, RedPin, p => RedPin = p);
            }
            else if (propertyName.Equals("GreenPinValue"))
            {
                ConvertPinValueToPin(GreenPinValue, GreenPin, p => GreenPin = p);
            }
            else if (propertyName.Equals("BluePinValue"))
            {
                ConvertPinValueToPin(BluePinValue, BluePin, p => BluePin = p);
            }
            else if (propertyName.Equals("RedPin"))
            {
                ValidatePinUniqueness();
                ConvertPinToPinValue(RedPin, RedPinValue, p => RedPinValue = p);
            }
            else if (propertyName.Equals("GreenPin"))
            {
                ValidatePinUniqueness();
                ConvertPinToPinValue(GreenPin, GreenPinValue, p => GreenPinValue = p);
            }
            else if (propertyName.Equals("BluePin"))
            {
                ValidatePinUniqueness();
                ConvertPinToPinValue(BluePin, BluePinValue, p => BluePinValue = p);
            }

            if (!propertyName.Equals("CanConnectTo") && !propertyName.Equals("IsLoaded"))
            {
                await OnPropertyChanged("CanConnectTo");
                await OnPropertyChanged("IsLoaded");
            }

        }
    }
}
