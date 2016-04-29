using Codify.VisualStudioOnline.BuildLight.UI;
using System;
using System.Threading.Tasks;

namespace Codify.VisualStudioOnline.BuildLight
{
    public enum MessageStatus
    {
        Unknown,
        InProgress,
        PartiallySucceeded,
        Succeeded,
        Failed,
        Cancelled,
        RetrievalError,

        Information,
        Warning,
        Error
    }

    public class LogMessage : NotifyPropertyChanged
    {
        public LogMessage(string text)
        {
            Id = Guid.NewGuid();
            Time = DateTime.Now;
            Text = text;
        }

        public LogMessage(string text, MessageStatus status)
            : this(text)
        {
            Status = status;
        }

        public Guid Id { get; set; }

        public DateTime Time { get; set; }


        public MessageStatus Status { get { return GetValue<MessageStatus>(); } set { SetValue(value); } }

        public string Text { get { return GetValue<string>(); } set { SetValue(value); } }

        public string FormattedText
        {
            get { return string.Format("{0} - {1}", Time.ToString("dd-MMM-yyyy HH:mm:ss"), Text); }
        }

        public async override Task OnPropertyChanged(string propertyName)
        {
            await base.OnPropertyChanged(propertyName);

            if (propertyName.Equals("Text") || propertyName.Equals("Status"))
            {
                await OnPropertyChanged("FormattedText");
            }
        }
    }
}
