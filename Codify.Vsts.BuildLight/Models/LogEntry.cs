using Codify.Vsts.BuildLight.UI;
using System;

namespace Codify.Vsts.BuildLight.Models
{
    public class LogEntry : NotifyPropertyChanged
    {
        public int Index {  get { return GetValue<int>(); } set { SetValue(value); } }
        public DateTime Timestamp { get { return GetValue<DateTime>(); } set { SetValue(value); } }

        public string FormattedTimestamp {  get { return Timestamp.ToString("dd-MMM-yyyy HH:mm:ss"); } }

        public string Code { get { return GetValue<string>(); } set { SetValue(value); } }

        public string BuildName { get { return GetValue<string>(); } set { SetValue(value); } }

        public string Text { get { return GetValue<string>(); } set { SetValue(value); } }

        public string Description { get { return GetValue<string>(); } set { SetValue(value); } }

        public bool HasDescription {  get { return !string.IsNullOrWhiteSpace(Description); } }
    }
}
