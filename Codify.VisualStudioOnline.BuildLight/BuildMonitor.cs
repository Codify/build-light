using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Codify.VisualStudioOnline.BuildLight
{
    internal class BuildMonitor
    {
        private const string _SettingsFilename = "config.json";
        private const string _BuildUrlFormat = "https://{0}.visualstudio.com/DefaultCollection/{1}/_apis/build/builds?definitions={2}&$top=1&api-version=2.0";
        private const string _DefinitionUrlFormat = "https://{0}.visualstudio.com/DefaultCollection/{1}/_apis/build/definitions?api-version=2.0";

        private Settings _Settings;

        private BuildDefinition _BuildDefinition;

        internal delegate void StatusChangedHandler(Status status, Guid? correlationId = null);
        internal event StatusChangedHandler StatusChanged;

        internal delegate void RetrievingStatusHandler(Guid? correlationId = null);
        internal event RetrievingStatusHandler RetrievingStatusStart;
        internal event RetrievingStatusHandler RetrievingStatusEnd;

        internal delegate void StoppedHandler();
        internal event StoppedHandler Stopped;

        internal delegate void LogHandler(string message, Guid? correlationId = null);
        internal event LogHandler Log;

        CancellationToken _Token;

        public AuthenticationHeaderValue AuthenticationHeader
        {
            get { return new AuthenticationHeaderValue("Basic", Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", _Settings.Username, _Settings.Password)))); }
        }

        internal void Start(Settings settings, CancellationToken token)
        {
            _Settings = settings;
            _Token = token;

            Task.Run(async () => await StartInternalAsync(), _Token);
        }

        internal async Task StartInternalAsync()
        {
            try
            {
                _BuildDefinition = await GetBuildDefinitionDetails();

                if (_BuildDefinition == null)
                {
                    StatusChanged?.Invoke(Status.RetrievalError);
                    Log("Could not retrieve build definition details");
                    return;
                }

                Status? lastStatus = null;

                while (!_Token.IsCancellationRequested)
                {
                    Guid? correlationId = Guid.NewGuid();

                    try
                    {
                        RetrievingStatusStart?.Invoke(correlationId);
                        lastStatus = await GetVsoStatus(correlationId, lastStatus);
                    }
                    catch (Exception ex)
                    {
                        Log("ERROR: " + ex.Message);
                    }
                    finally
                    {
                        RetrievingStatusEnd?.Invoke(correlationId);
                        await Task.Delay(60000, _Token);
                    }
                }
            }
            finally
            {
                Stopped?.Invoke();
            }

        }

        private async Task<string> GetAsync(HttpClient client, String apiUrl)
        {
            var responseBody = string.Empty;

            try
            {
                using (HttpResponseMessage response = client.GetAsync(apiUrl, _Token).Result)
                {
                    response.EnsureSuccessStatusCode();
                    responseBody = await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                Log("Error while fetching status: " + ex.Message);
                StatusChanged(Status.RetrievalError);
            }

            return responseBody;
        }

        private async Task<BuildDefinition> GetBuildDefinitionDetails()
        {

            var responseBody = string.Empty;

            string url = string.Format(_DefinitionUrlFormat, _Settings.Account, _Settings.Project);

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //Set alternate credentials
                client.DefaultRequestHeaders.Authorization = AuthenticationHeader;

                //Get me the last build
                responseBody = await GetAsync(client, url);

                if (!string.IsNullOrWhiteSpace(responseBody))
                {

                    BuildDefinitionList definitionList = Util.GetObjectFromJson<BuildDefinitionList>(responseBody);

                    return definitionList.Definitions.FirstOrDefault(x => x.Name.ToLowerInvariant() == _Settings.BuildName.ToLowerInvariant());

                }
                else
                {
                    return null;
                }
            }
        }

        private async Task<Status> GetVsoStatus(Guid? correlationId, Status? lastStatus)
        {
            var newStatus = Status.Unknown;

            var responseBody = string.Empty;

            Build build;

            string url = string.Format(_BuildUrlFormat, _Settings.Account, _Settings.Project, _BuildDefinition.Id);

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //Set alternate credentials
                client.DefaultRequestHeaders.Authorization = AuthenticationHeader;

                //Get me the last build
                responseBody = await GetAsync(client, url);

                var statusMessage = string.Empty;
                if (!string.IsNullOrWhiteSpace(responseBody))
                {
                    BuildList builds = Util.GetObjectFromJson<BuildList>(responseBody);

                    build = builds.Builds.FirstOrDefault();

                    if (build != null)
                    {
                        switch (build.ProgressStatus)
                        {
                            case BuildProgressStatus.Completed:

                                switch (build.ResultStatus)
                                {
                                    case BuildResultStatus.Succeeded:
                                        newStatus = Status.Succeeded;
                                        break;

                                    case BuildResultStatus.Failed:
                                        newStatus = Status.Failed;
                                        break;

                                    case BuildResultStatus.PartiallySucceeded:
                                        newStatus = Status.PartiallySucceeded;
                                        break;

                                    case BuildResultStatus.Canceled:
                                        newStatus = Status.Cancelled;
                                        break;

                                    default:
                                        newStatus = Status.Unknown;
                                        statusMessage = "setting status to 'default' because status is " + build.ResultStatus;
                                        break;
                                }

                                break;

                            case BuildProgressStatus.InProgress:
                                newStatus = Status.InProgress;
                                break;

                            default:
                                newStatus = Status.Unknown;
                                break;
                        }
                    }
                    else
                    {
                        StatusChanged?.Invoke(Status.Unknown, correlationId);
                        statusMessage = "Could not load build";
                    }
                }
                else
                {
                    StatusChanged?.Invoke(Status.Unknown, correlationId);
                    statusMessage = "Could not load build";
                }

                if (lastStatus.HasValue && (lastStatus.Value == newStatus))
                {
                    Log(string.IsNullOrWhiteSpace(statusMessage) ? "Build status is unchanged." : statusMessage, correlationId);
                }
                else
                {
                    StatusChanged?.Invoke(newStatus, correlationId);
                    if (!string.IsNullOrWhiteSpace(statusMessage))
                    {
                        Log(statusMessage, correlationId);
                    }
                }
            }

            return newStatus;
        }
    }
}
