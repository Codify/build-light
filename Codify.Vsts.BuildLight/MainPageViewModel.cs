using Codify.Vsts.BuildLight.Data;
using Codify.Vsts.BuildLight.Models;
using Codify.Vsts.BuildLight.Services;
using Codify.Vsts.BuildLight.UI;
using Codify.Vsts.BuildLight.ViewModels;
using Microsoft.ApplicationInsights;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.System;

namespace Codify.Vsts.BuildLight
{
    public enum AvailableViews
    {
        BuildState = 0,
        Logging = 1,
        Settings = 2
    }

    public class MainPageViewModel : SettingsViewModel
    {
        public MainPageViewModel()
        {
            RegisterCommands();
        }

        public bool ShowNavigationBarExpanded { get { return GetValue<bool>(); } set { SetValue(value); } }

        public DelegateCommand ExpandNavigationBarCommand { get; set; }

        public DelegateCommand DisplayBuildStateCommmand { get; set; }

        public DelegateCommand DisplayLogsCommmand { get; set; }

        public DelegateCommand DisplayCodifyWebsiteCommand { get; set; }

        public DelegateCommand DisplaySettingsCommand { get; set; }

        public CancellationTokenSource CancellationToken { get; set; }

        public int BuildCheckIdentifier { get { return GetValue<int>(); } set { SetValue(value); } }

        public AvailableViews CurrentView { get { return GetValue<AvailableViews>(); } set { SetValue(value); } }

        private LedLightService LightService { get; set; }

        private void RegisterCommands()
        {
            ExpandNavigationBarCommand = new DelegateCommand(parameter => ShowNavigationBarExpanded = !ShowNavigationBarExpanded);
            DisplayBuildStateCommmand = new DelegateCommand(parameter => CurrentView = AvailableViews.BuildState);
            DisplayLogsCommmand = new DelegateCommand(parameter => CurrentView = AvailableViews.Logging);
            DisplayCodifyWebsiteCommand = new DelegateCommand(parameter => OnDisplayCodifyWebsite(parameter));
            DisplaySettingsCommand = new DelegateCommand(parameter => CurrentView = AvailableViews.Settings);
        }

        internal async Task InitialiseAsync()
        {
            await LoadSettingsAsync();

            await Task.Run(() =>
            {
                CancellationToken = new CancellationTokenSource();
                BuildService = new BuildService(Settings, CancellationToken.Token);

                LightService = new LedLightService(Settings, BuildService);
            });
        }

        public BuildResultStatus CombinedBuildStatus {  get { return GetValue<BuildResultStatus>(); } set { SetValue(value); } }

        protected override async void OnServiceEvent(object sender, BuildEventArgs e)
        {
            base.OnServiceEvent(sender, e);

            if (e.Code == BuildEventCode.BuildInformationRetrievalEnd)
            {
                await PerformUICode(() => CombinedBuildStatus = BuildService.ServiceStatus);
            }
        }

        private async void OnDisplayCodifyWebsite(object parameter)
        {
            await Launcher.LaunchUriAsync(new Uri("http://www.codify.com"));
        }

    }
}
