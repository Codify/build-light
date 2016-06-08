using Codify.Vsts.BuildLight.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Threading;
using System.Net.Http;
using System.Net.Http.Headers;
using Codify.Vsts.BuildLight.Data;
using Newtonsoft.Json;
using Codify.Vsts.BuildLight.Extensions;
using Microsoft.ApplicationInsights;

namespace Codify.Vsts.BuildLight.Services
{
    public enum BuildEventCode
    {
        NoBuildInformationAvailable,
        NoBuildsFound,
        ServiceStart,
        StatusUpdate,
        BuildInformationRetrievalStart,
        BuildInstanceRetrievalStart,
        BuildInstanceRetrievalEnd,
        BuildInformationRetrievalEnd,
        Error,
        SkippedBuild,
        ServiceEnd
    }

    public class BuildEventArgs : EventArgs
    {
        public BuildEventArgs()
        {
            Timestamp = DateTime.Now;
        }

        public string Name { get; set; }

        public BuildEventCode Code { get; set; }

        public BuildDetails BuildDetails { get; set; }

        public Exception Exception { get; set; }

        public DateTime Timestamp { get; set; }

        public override string ToString()
        {
            var message = string.Empty;

            BuildInstance instance = null;
            if (BuildDetails != null)
            {
                if (BuildDetails.CurrentBuild != null)
                {
                    instance = BuildDetails.CurrentBuild;
                }
                else if (BuildDetails.PreviousBuild != null)
                {
                    instance = BuildDetails.PreviousBuild;
                }
            }

            if (instance != null)
            {
                if (instance.ProgressStatus == BuildProgressStatus.Completed)
                {
                    message = string.Format("Build '{0}' completed with a result status of '{1}'.", instance.Name, instance.ResultStatus.ToString().ToLower());
                }
                else if (instance.ProgressStatus == BuildProgressStatus.InProgress)
                {
                    message = string.Format("Build '{0}' is current in progress.", instance.Name);
                }
                else
                {
                    message = string.Format("The execution status of the build instance '{0}' is unknown.", instance.Name);
                }
            }

            return message;
        }
    }

    public class BuildService
    {

        public event EventHandler<BuildEventArgs> ServiceEvent;

        public event EventHandler<BuildEventArgs> BuildEvent;


        public BuildService(Settings settings, CancellationToken cancellationToken)
        {
            Settings = settings;
            Settings.PropertyChanged += OnSettingsChanged;

            Telemetry = new TelemetryClient();

            CancellationToken = cancellationToken;

            Task.Run(async () =>
            {
                await StartMonitingAsync();
            }, CancellationToken);
        }

        public BuildService() : base()
        {
        }

        #region Properties

        private TelemetryClient Telemetry { get; set; }

        private CancellationToken CancellationToken { get; set; }

        private Timer BuildCheckTimer { get; set; }

        private Settings Settings { get; set; }

        private string VstsUrl { get { return string.Format("https://{0}.visualstudio.com", Settings.Account); } }

        private string ProjectUrl { get { return string.Format("{0}/DefaultCollection/{1}", VstsUrl, Settings.Project); } }

        private string BuildDefinitionsUrl { get { return string.Format("{0}/_apis/build/definitions?api-version=2.0", ProjectUrl); } }

        private string BuildUrl { get { return string.Format("{0}/_apis/build/builds?definitions={1}&$top=2&api-version=2.0", ProjectUrl, "{0}"); } }

        private BuildDefinitionList BuildDefinitions { get; set; }

        private bool UpdateBuildPeriod { get; set; }

        public BuildResultStatus ServiceStatus { get; set; }

        private TimeSpan BuildCheckPeriod
        {
            get
            {
                TimeSpan period = new TimeSpan(0, 1, 0);

                if (Settings != null)
                {
                    period = new TimeSpan
                        (
                            Settings.BuildCheckScale == BuildCheckScale.Hours ? Settings.BuildCheckPeriod : 0,
                            Settings.BuildCheckScale == BuildCheckScale.Minutes ? Settings.BuildCheckPeriod : 0,
                            Settings.BuildCheckScale == BuildCheckScale.Seconds ? Settings.BuildCheckPeriod : 0
                        );
                }

                return period;
            }
        }

        public AuthenticationHeaderValue AuthenticationHeader
        {
            get { return new AuthenticationHeaderValue("Basic", Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", Settings.Username, Settings.Password)))); }
        }

        private bool CanConnectToVsts
        {
            get
            {
                return (Settings != null) && !string.IsNullOrWhiteSpace(Settings.Account) && !string.IsNullOrWhiteSpace(Settings.Username) && !string.IsNullOrWhiteSpace(Settings.Password) && !string.IsNullOrWhiteSpace(Settings.Project);
            }
        }

        #endregion

        private async Task StartMonitingAsync()
        {
            await RaiseEventAsync(ServiceEvent, new BuildEventArgs() { Code = BuildEventCode.ServiceStart });

            while (!CancellationToken.IsCancellationRequested)
            {
                try
                {
                    await RaiseEventAsync(ServiceEvent, new BuildEventArgs() { Code = BuildEventCode.BuildInformationRetrievalStart });

                    if ((BuildDefinitions == null) || !BuildDefinitions.Definitions.Any())
                    {
                        await GetBuildDefinitionsAsync();
                    }

                    await Task.Delay(2000, CancellationToken);

                    if (BuildDefinitions != null)
                    {
                        if (BuildDefinitions.Definitions.Any())
                        {
                            ServiceStatus = BuildResultStatus.Unknown;

                            foreach (var definition in BuildDefinitions.Definitions)
                            {
                                if ((!Settings.BuildNames.Any() || Settings.BuildNames.Contains(definition.Name)) && (!definition.IsDisabled || Settings.IncludeDisabledBuilds))
                                {
                                    await AnalyseDefinitionAsync(definition);
                                }
                                else
                                {
                                    await RaiseEventAsync(ServiceEvent, new BuildEventArgs() { Code = BuildEventCode.SkippedBuild, Name = definition.Name });
                                }
                            }
                        }
                        else
                        {
                            await RaiseEventAsync(ServiceEvent, new BuildEventArgs() { Code = BuildEventCode.NoBuildsFound });
                        }
                    }
                }
                catch (Exception ex)
                {
                    await RaiseEventAsync(ServiceEvent, new BuildEventArgs() { Code = BuildEventCode.Error, Exception = ex });
                }
                finally
                {
                    await RaiseEventAsync(ServiceEvent, new BuildEventArgs() { Code = BuildEventCode.BuildInformationRetrievalEnd });
                    await Task.Delay(BuildCheckPeriod, CancellationToken);
                }
            }

            await RaiseEventAsync(ServiceEvent, new BuildEventArgs() { Code = BuildEventCode.ServiceEnd });
        }

        private async Task AnalyseDefinitionAsync(BuildDefinition definition)
        {
            await RaiseEventAsync(BuildEvent, new BuildEventArgs() { Code = BuildEventCode.BuildInstanceRetrievalStart, Name = definition.Name });

            try
            {
                var last2Builds = await GetLastBuildInstanceAsync(definition);
                if (last2Builds.Any())
                {
                    UpdateLastBuildStatusAsync(last2Builds[0]);

                    await RaiseBuildEventAsync(definition, last2Builds.Count > 1 ? last2Builds[1] : null, last2Builds[0]);
                }
                else
                {
                    await RaiseEventAsync(BuildEvent, new BuildEventArgs() { Code = BuildEventCode.NoBuildsFound, Name = definition.Name });
                }
            }
            catch (Exception ex)
            {
                await RaiseBuildEventAsync(definition, null, null, ex);
            }
            finally
            {
                await RaiseEventAsync(BuildEvent, new BuildEventArgs() { Code = BuildEventCode.BuildInstanceRetrievalEnd, Name = definition.Name });
            }
        }

        private void UpdateLastBuildStatusAsync(BuildInstance build)
        {
            var buildStatus = build.ProgressStatus == BuildProgressStatus.InProgress ? BuildResultStatus.InProgress : build.ResultStatus;

            if (ServiceStatus < buildStatus)
            {
                ServiceStatus = buildStatus;
            }
        }

        private async Task RaiseBuildEventAsync(BuildDefinition definition, BuildInstance previousBuild = null, BuildInstance mostRecentBuild = null, Exception ex = null)
        {
            await RaiseEventAsync(BuildEvent, new BuildEventArgs()
            {
                Name = definition.Name,
                Code = ex == null ? BuildEventCode.StatusUpdate : BuildEventCode.Error,
                BuildDetails = new BuildDetails()
                {
                    Definition = definition,
                    CurrentBuild = mostRecentBuild,
                    PreviousBuild = previousBuild
                },
                Exception = ex
            });
        }

        private async Task RaiseEventAsync<T>(EventHandler<T> handler, T args)
        {
            await Task.Run(() =>
            {
                if (handler != null)
                {
                    handler.Invoke(this, args);
                }
            });
        }

        #region VSTS Build Queries

        private async Task GetBuildDefinitionsAsync()
        {
            BuildDefinitions = await GetAsync<BuildDefinitionList>(BuildDefinitionsUrl);
            if (BuildDefinitions == null)
            {
                await RaiseEventAsync(ServiceEvent, new BuildEventArgs() { Code = BuildEventCode.NoBuildInformationAvailable });
            }
        }

        private async Task<List<BuildInstance>> GetLastBuildInstanceAsync(BuildDefinition definition)
        {
            List<BuildInstance> instances = null;

            var instanceList = await GetAsync<BuildInstanceList>(string.Format(BuildUrl, definition.Id));
            if ((instanceList != null) && (instanceList.Builds != null))
            {
                instances = instanceList.Builds.Take(2).ToList();
            }

            return instances;
        }

        public async Task<T> GetAsync<T>(string url)
        {
            T result = default(T);

            if (CanConnectToVsts)
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = AuthenticationHeader;

                    var responseBody = await GetAsync(client, url);
                    if (!string.IsNullOrWhiteSpace(responseBody))
                    {
                        result = responseBody.ConvertJsonTo<T>();
                    }
                }
            }

            return result;
        }

        private async Task<string> GetAsync(HttpClient client, String apiUrl)
        {
            var responseBody = string.Empty;

            try
            {
                using (var response = client.GetAsync(apiUrl, CancellationToken).Result)
                {
                    response.EnsureSuccessStatusCode();
                    responseBody = await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                //Log("Error while fetching status: " + ex.Message);
                //StatusChanged(Status.RetrievalError);
            }

            return responseBody;
        }

        #endregion

        #region Settings Property Changed

        private async void OnSettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("Account") || e.PropertyName.Equals("Username") || e.PropertyName.Equals("Password") || e.PropertyName.Equals("Project") || e.PropertyName.Equals("BuildNames"))
            {
                await GetBuildDefinitionsAsync();
            }
        }

        #endregion

    }
}
