using Codify.VisualStudioOnline.BuildLight.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Codify.VisualStudioOnline.BuildLight.Extensions;

namespace Codify.VisualStudioOnline.BuildLight
{
    public class MainPageViewModel : NotifyPropertyChanged
    {
        public MainPageViewModel()
        {
            RunButtonText = "Start";

            Telemetry = new Telemetry();
            LogEntries = new ObservableCollection<LogMessage>();

            BuildStatus = Status.Unknown;

            RegisterCommands();
        }

        public bool ShowSettings { get { return GetValue<bool>(); } set { SetValue(value); } }

        public string RunButtonText { get { return GetValue<string>(); } set { SetValue(value); } }

        public Status BuildStatus { get { return GetValue<Status>(); } set { SetValue(value); } }

        public ObservableCollection<LogMessage> LogEntries { get; set; }

        public string LogFileName { get; set; }

        public StorageFile LogFile { get; set; }

        private Telemetry Telemetry { get; set; }

        private BuildMonitor BuildMonitor { get; set; }

        public Settings Settings { get { return GetValue<Settings>(); } set { SetValue(value); } }

        private CancellationTokenSource CancellationToken { get; set; }

        private LEDLightListener LedListener { get; set; }

        public DelegateCommand ToggleRunningCommand { get; set; }

        public DelegateCommand SaveSettingsCommand { get; set; }

        #region Command Management

        private void RegisterCommands()
        {
            ToggleRunningCommand = new DelegateCommand(ExecuteRunningToggle);
            SaveSettingsCommand = new DelegateCommand(ExecuteSaveSettings);
        }

        private async void ExecuteRunningToggle(object argument)
        {
            if (IsBusy)
            {
                RunButtonText = "Stop";

                Telemetry.TrackEvent("App.Start");
                CancellationToken = new CancellationTokenSource();

                if (LedListener == null)
                {
                    try
                    {
                        var gpio = Windows.Devices.Gpio.GpioController.GetDefault();
                        if (gpio != null)
                        {
                            Telemetry.TrackEvent("LED.Found");
                            await Log("Initialising LED listener...");
                            LedListener = new LEDLightListener();
                            LedListener.Subscribe(BuildMonitor, Settings);
                            LedListener.Start();
                        }
                        else
                        {
                            // We are running on a machine that does not support the ledlistener, so flag it as successfully started
                        }
                    }
                    catch (Exception ex)
                    {
                        Telemetry.TrackError(ex);
                        Debug.WriteLine("Could not initialise LED listener: " + ex.Message);
                    }
                }

                await Log("Starting build monitor...");
                BuildMonitor.Start(Settings, CancellationToken.Token);
            }
            else
            {
                RunButtonText = "Start";

                Telemetry.TrackEvent("App.StopClick");
                if (LedListener != null)
                {
                    LedListener.Dispose();
                    LedListener.Unsubscribe(BuildMonitor);
                    LedListener = null;
                }
                CancellationToken.Cancel();
            }
        }

        private async void ExecuteSaveSettings(object argument)
        {
            if ((!ShowSettings) || (argument != null))
            {
                await SaveSettingsAsync();
            }
        }

        #endregion

        #region Initialise Application

        internal async void Initialise()
        {
            await InitialiseLoggingAsync();

            InitialiseBuildMonitor();

            await LoadSettingsAsync();

            if (!Settings.IsLoaded())
            {
                ShowSettings = true;
            }
            else
            {
                IsBusy = true;
                ExecuteRunningToggle(null);
            }
        }

        #endregion

        #region Build Monitor Activity

        private void InitialiseBuildMonitor()
        {
            BuildMonitor = new BuildMonitor();

            BuildMonitor.StatusChanged += BuildMonitor_StatusChanged;
            BuildMonitor.RetrievingStatusStart += BuildMonitor_RetrievingStatusStart;
            BuildMonitor.Stopped += BuildMonitor_Stopped;
            BuildMonitor.Log += BuildMonitor_Log;
        }

        private async void BuildMonitor_Log(string text, Status status, Guid? correlationId)
        {
            await Log(text, status, correlationId);
        }

        private async void BuildMonitor_Stopped()
        {
            IsBusy = false;
            RunButtonText = "Start";

            BuildStatus = Status.Unknown;

            await Log("Build Monitor stopped.");
        }

        private async void BuildMonitor_RetrievingStatusStart(Guid? correlationId)
        {
            await Log("Retrieving Build status ...", Status.Unknown, correlationId);
            BuildStatus = Status.Unknown;
        }

        private async void BuildMonitor_StatusChanged(Status status, Guid? correlationId)
        {
            Telemetry.TrackEvent("Build." + status.ToString());
            await Log("Build " + status.ToString(), status, correlationId);
            BuildStatus = status == Status.Unknown ? Status.RetrievalError : status;
        }


        #endregion

        #region Settings Management

        private async Task<StorageFile> GetSettingsFile(bool overwrite = false)
        {
            StorageFile file = null;

            if (overwrite)
            {
                file = await ApplicationData.Current.LocalFolder.CreateFileAsync("config.json", CreationCollisionOption.ReplaceExisting);
            }
            else
            {
                file = await ApplicationData.Current.LocalFolder.GetFileAsync("config.json");
            }

            return file;
        }

        private async Task LoadSettingsAsync()
        {
            List<Exception> exceptions = new List<Exception>();

            var correlationId = Guid.NewGuid();

            try
            {
                await Log("Loading settings ...", correlationId);

                Settings = Util.GetObjectFromJson<Settings>(await FileIO.ReadTextAsync(await GetSettingsFile()));
            }
            catch (Exception ex)
            {
                Telemetry.TrackError(ex);
                exceptions.Add(ex);
            }

            if (Settings == null)
            {
                await Log("Settings do not exists, Initialising new settings ...", correlationId);
                try
                {
                    Settings = new Settings();
                }
                catch (Exception ex)
                {
                    Telemetry.TrackError(ex);
                    exceptions.Add(ex);
                }
            }

            if (Settings != null)
            {
                GetCredentials();
                await Log("Settings loaded.", correlationId);
                Settings.PropertyChanged += Settings_PropertyChanged;
            }
            else
            {
                LogBackground("Failed to load settings", MessageStatus.Error, correlationId);
                exceptions.ForEach(ex => LogBackground(ex.Message, MessageStatus.Error));
            }
        }

        private async Task SaveSettingsAsync()
        {
            var correlationId = Guid.NewGuid();

            try
            {
                await Log("Saving settings ...", correlationId);
                await SaveCredentialsAsync();
                await FileIO.WriteTextAsync(await GetSettingsFile(true), Util.GetJsonFromObject(Settings));
                await Log("Settings saved.", correlationId);
            }
            catch (Exception ex)
            {
                Telemetry.TrackError(ex);
                LogBackground("Failed to save settings: " + ex.Message, MessageStatus.Error, correlationId);
            }
        }

        private bool GetCredentials()
        {
            var vault = new Windows.Security.Credentials.PasswordVault();
            var credentialsLoaded = true;

            try
            {
                var allCreds = vault.FindAllByResource("BuildLight");
                var credential = allCreds.FirstOrDefault();
                if (credential != null)
                {
                    credential.RetrievePassword();
                    Settings.Username = credential.UserName;
                    Settings.Password = credential.Password;
                }
                else
                {
                    credentialsLoaded = false;
                }
            }
            catch (Exception ex)
            {
                credentialsLoaded = false;
                Telemetry.TrackError(ex);
            }

            return credentialsLoaded;
        }

        private async Task SaveCredentialsAsync()
        {
            if (!string.IsNullOrWhiteSpace(Settings.Username) && !string.IsNullOrWhiteSpace(Settings.Password))
            {
                await Task.Run(() =>
                    {
                        var vault = new Windows.Security.Credentials.PasswordVault();
                        vault.Add(new Windows.Security.Credentials.PasswordCredential("BuildLight", Settings.Username, Settings.Password));
                    });
            }
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

        }

        #endregion

        #region Logging Management

        private async Task InitialiseLoggingAsync()
        {
            try
            {
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;

                LogFileName = "Log." + Guid.NewGuid().ToString() + ".txt";
                LogFile = await localFolder.CreateFileAsync(LogFileName, CreationCollisionOption.ReplaceExisting);
            }
            catch (Exception ex)
            {
                Telemetry.TrackError(ex);
                Debug.WriteLine("Failed to initialiase log file: " + ex.Message);
                throw;
            }
        }

        private void LogBackground(string text, MessageStatus status, Guid? correlationId = null)
        {
            Task.Run(async () => await Log(text, status, correlationId));
        }


        private async Task Log(string text, Status status, Guid? correlationId = null)
        {
            MessageStatus messageStatus;

            if (!Enum.TryParse<MessageStatus>(status.ToString(), out messageStatus))
            {
                messageStatus = MessageStatus.Information;
            }

            await Log(text, messageStatus, correlationId);
        }

        private async Task Log(string text, Guid? correlationId = null)
        {
            await Log(message => message.Text = text, () => new LogMessage(text), text, MessageStatus.Information, correlationId);
        }

        private async Task Log(string text, MessageStatus status, Guid? correlationId = null)
        {
            await Log(message => { message.Text = text; message.Status = status; }, () => new LogMessage(text, status), text, status, correlationId);
        }

        private async Task Log(Action<LogMessage> updateMessage, Func<LogMessage> createMessage, string text, MessageStatus status, Guid? correlationId)
        {
            try
            {
                await FileIO.AppendTextAsync(LogFile, text + "\r\n");
            }
            catch (FileNotFoundException)
            {
                Debug.WriteLine("Could not load log file");
            }

            var message = correlationId.HasValue ? LogEntries.FirstOrDefault(m => m.Id == correlationId) : null;
            if (message != null)
            {
                await PerformUICode(() =>
                {
                    if (status == message.Status)
                    {
                        LogEntries.Remove(message);
                    }
                    else
                    {
                        updateMessage.Invoke(message);
                    }
                });
            }
            else
            {
                message = createMessage.Invoke();
                if (correlationId.HasValue)
                {
                    message.Id = correlationId.Value;
                }
                await PerformUICode(() => 
                {
                    LogEntries.Insert(0, message);
                    while (LogEntries.Count > 1000)
                    {
                        LogEntries.RemoveAt(LogEntries.Count - 1);
                    }
                });
            }
        }

        #endregion

    }
}
