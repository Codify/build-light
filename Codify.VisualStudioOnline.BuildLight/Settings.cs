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
        public bool CanConnectTo { get { return !string.IsNullOrWhiteSpace(Account) && !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password); } }

        [JsonIgnore]
        public bool IsLoaded { get { return CanConnectTo && !string.IsNullOrWhiteSpace(Project) && !string.IsNullOrWhiteSpace(BuildName); } }

        public async override Task OnPropertyChanged(string propertyName)
        {
            await base.OnPropertyChanged(propertyName);

            if (!propertyName.Equals("CanConnectTo") && !propertyName.Equals("IsLoaded"))
            {
                await OnPropertyChanged("CanConnectTo");
                await OnPropertyChanged("IsLoaded");
            }
        }
    }
}
