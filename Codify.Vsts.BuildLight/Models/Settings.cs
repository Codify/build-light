using Codify.Vsts.BuildLight.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codify.Vsts.BuildLight.Models
{
    public enum BuildCheckScale
    {
        Seconds,
        Minutes,
        Hours
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
            BuildNames = new ObservableCollection<string>();
        }

        [JsonProperty("account")]
        public string Account { get { return GetValue<string>(); } set { SetValue(value); } }

        [JsonIgnore]
        public string Username { get { return GetValue<string>(); } set { SetValue(value); } }

        [JsonIgnore]
        public string Password { get { return GetValue<string>(); } set { SetValue(value); } }

        [JsonProperty("project")]
        public string Project { get { return GetValue<string>(); } set { SetValue(value); } }

        [JsonProperty("redPin")]
        public int RedPin { get { return GetValue<int>(); } set { SetValue(value); } }

        [JsonProperty("greenPin")]
        public int GreenPin { get { return GetValue<int>(); } set { SetValue(value); } }

        [JsonProperty("bluePin")]
        public int BluePin { get { return GetValue<int>(); } set { SetValue(value); } }

        [JsonProperty("buildCheckPeriod")]
        public int BuildCheckPeriod { get; set; }

        [JsonProperty("buildCheckScale")]
        public BuildCheckScale BuildCheckScale { get; set; }

        [JsonProperty("buildNames")]
        public ObservableCollection<string> BuildNames { get; set; }

        [JsonProperty("includeDisabledBuilds")]
        public bool IncludeDisabledBuilds { get; set; }

    }
}
