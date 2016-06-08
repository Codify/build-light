using Codify.Vsts.BuildLight.Models;
using Codify.Vsts.BuildLight.Services;
using Codify.Vsts.BuildLight.UI;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Codify.Vsts.BuildLight.ViewModels
{
    public class LoggingViewModel : BaseViewModel
    {
        public LoggingViewModel()
        {
            LogEntries = new ObservableCollection<LogEntry>();
            LogEntries.CollectionChanged += LogEntries_CollectionChanged;
        }

        public LoggingViewModel(BuildService buildService)
            : this()
        {
            BuildService = buildService;
        }

        public ObservableCollection<LogEntry> LogEntries { get; set; }

        private void LogEntries_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            for (var index = 0; index < LogEntries.Count; index++)
            {
                LogEntries[index].Index = index;
            }
        }

        protected async override void OnBuildEvent(object sender, BuildEventArgs e)
        {
            base.OnBuildEvent(sender, e);
            await AddBuildEventToLogsAsync(e);
        }

        protected async override void OnServiceEvent(object sender, BuildEventArgs e)
        {
            base.OnServiceEvent(sender, e);
            await AddBuildEventToLogsAsync(e);
        }

        private async Task AddBuildEventToLogsAsync(BuildEventArgs e)
        {
            await PerformUICode(() =>
            {
                LogEntries.Add(new LogEntry()
                {
                    Timestamp = e.Timestamp,
                    BuildName = e.BuildDetails?.Definition.Name,
                    Code = e.Code.ToString(),
                    Text = e.ToString(),
                    Description = e.Exception?.StackTrace
                });

                while (LogEntries.Count > 1000)
                {
                    LogEntries.RemoveAt(0);
                }
            });
        }
    }
}
