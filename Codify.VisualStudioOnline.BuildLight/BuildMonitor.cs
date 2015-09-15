using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace Codify.VisualStudioOnline.BuildLight
{
    internal class BuildMonitor
    {
        private const string _SettingsFilename = "config.json";
        private const string _BuildUrlFormat = "https://{0}.visualstudio.com/DefaultCollection/{1}/_apis/build/builds?definitions={2}&$top=1&api-version=2.0";
        private const string _DefinitionUrlFormat = "https://{0}.visualstudio.com/DefaultCollection/{1}/_apis/build/definitions?api-version=2.0";

        // Get the alternate credentials that you'll use to access the Visual Studio Online account.
        private string _AltUsername;
        private string _AltPassword;

        private Settings _Settings;

        private BuildDefinition _BuildDefinition;

        internal delegate void StatusChangedHandler(Status status);
        internal event StatusChangedHandler StatusChanged;

        internal delegate void RetrievingStatusHandler();
        internal event RetrievingStatusHandler RetrievingStatusStart;
        internal event RetrievingStatusHandler RetrievingStatusEnd;

        internal delegate void StoppedHandler();
        internal event StoppedHandler Stopped;

        internal delegate void LogHandler(string message);
        internal event LogHandler Log;

        CancellationToken _Token;

        internal void Start(
            string altUsername,
            string altPassword,
            Settings settings,
            CancellationToken token
            )
        {
            _AltUsername = altUsername;
            _AltPassword = altPassword;
            _Settings = settings;
            _Token = token;

            Task.Run(async () => await StartInternalAsync(), _Token);

        }

        internal async Task StartInternalAsync()
        {
            try
            {
                await StoreSettings();

                _BuildDefinition = await GetBuildDefinitionDetails();

                if (_BuildDefinition == null)
                {
                    StatusChanged(Status.RetrievalError);
                    Log("Could not retrieve build definition details");
                    return;
                }

                while (!_Token.IsCancellationRequested)
                {
                    try
                    {
                        RetrievingStatusStart();
                        await GetVsoStatus();
                    }
                    catch (Exception ex)
                    {
                        Log("ERROR: " + ex.Message);
                    }
                    finally
                    {
                        RetrievingStatusEnd();
                        await Task.Delay(60000, _Token);
                    }
                }
            }
            finally
            {
                Stopped();
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
                client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                //Set alternate credentials
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(
                        System.Text.ASCIIEncoding.ASCII.GetBytes(
                            string.Format("{0}:{1}", _AltUsername, _AltPassword))));


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

        private async Task GetVsoStatus()
        {

            var responseBody = string.Empty;

            Build build;

            string url = string.Format(_BuildUrlFormat, _Settings.Account, _Settings.Project, _BuildDefinition.Id);

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                //Set alternate credentials
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(
                        System.Text.ASCIIEncoding.ASCII.GetBytes(
                            string.Format("{0}:{1}", _AltUsername, _AltPassword))));


                //Get me the last build
                responseBody = await GetAsync(client, url);

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

                                        StatusChanged(Status.Succeeded);
                                        break;

                                    case BuildResultStatus.Failed:
                                        StatusChanged(Status.Failed);
                                        break;

                                    case BuildResultStatus.PartiallySucceeded:
                                        StatusChanged(Status.PartiallySucceeded);
                                        break;


                                    case BuildResultStatus.Canceled:
                                        StatusChanged(Status.Cancelled);
                                        break;

                                    default:

                                        StatusChanged(Status.Unknown);
                                        Log("setting status to 'default' because status is " + build.ResultStatus);
                                        break;
                                }

                                break;

                            case BuildProgressStatus.InProgress:
                                StatusChanged(Status.InProgress);
                                break;

                            default:
                                StatusChanged(Status.Unknown);
                                break;
                        }



                    }
                    else
                    {
                        StatusChanged(Status.Unknown);
                        Log("could not load build");
                    }
                }
                else
                {
                    StatusChanged(Status.Unknown);
                    Log("could not load build");
                }

            }


        }

        private async Task StoreSettings()
        {
            try
            {
                //store username and password securely
                var vault = new Windows.Security.Credentials.PasswordVault();
                vault.Add(new Windows.Security.Credentials.PasswordCredential(
                    "BuildLight", _AltUsername, _AltPassword)
                );


                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                StorageFile file = await localFolder.CreateFileAsync(_SettingsFilename, CreationCollisionOption.ReplaceExisting);

                string contents = Util.GetJsonFromObject(_Settings);
                await FileIO.WriteTextAsync(file, contents);
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                throw;
            }

        }


    }
}
