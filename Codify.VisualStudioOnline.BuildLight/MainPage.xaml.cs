using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Codify.VisualStudioOnline.BuildLight
{
    public sealed partial class MainPage : Page
    {
        BuildMonitor _BuildMonitor = new BuildMonitor();
        StorageFile _LogFile;
        LEDLightListener _LEDListener;
        CancellationTokenSource _CancellationToken;
        Settings _Settings = new Settings();
        Telemetry _Telemetry;
        private bool _Running;
        private string _LogFileName;
        private bool _Loaded = false;

        public MainPage()
        {
            _LogFileName = "Log." + Guid.NewGuid().ToString() + ".txt";
            _Telemetry = new Telemetry();
            this.InitializeComponent();

        }

        private async void _BuildMonitor_Log(string message)
        {
            await Log(message);
        }

        private void LogBackground(string message)
        {
            Task.Run(async () => await Log(message));

        }

        private async Task Log(string message)
        {
            try
            {

                await FileIO.AppendTextAsync(_LogFile, message + "\r\n");

            }
            catch (FileNotFoundException)
            {
                Debug.WriteLine("Could not load log file");
            }

            await PerformUICode(
            () =>
            {
                if (settingsContainer.Visibility == Visibility.Visible)
                {
                    txtLog.Text += message + "\r\n";
                }

            }
            );


        }

        private async void _BuildMonitor_Stopped()
        {
            _Running = false;

            await PerformUICode(
            () =>
            {
                tbRunStatus.Text = "State: Stopped";
            }
            );
        }

        private void UpdateSettings()
        {

            //store username and password securely
            var vault = new Windows.Security.Credentials.PasswordVault();
            vault.Add(new Windows.Security.Credentials.PasswordCredential(
                "BuildLight", txtUsername.Text, txtPassword.Password)
            );

            _Settings = new Settings()
            {
                Account = txtVSAccount.Text,
                Project = txtVSProject.Text,
                BuildName = txtBuildName.Text,
                RedPin = int.Parse(txtR.Text),
                GreenPin = int.Parse(txtG.Text),
                BluePin = int.Parse(txtB.Text),
            };

        }

        private async Task<bool> LoadSettings()
        {
            try
            {
                await Log("Loading settings ...");

                await Log("Trying to retrieve credentials...");
                var vault = new Windows.Security.Credentials.PasswordVault();
                bool credentialsLoaded = true;


                try
                {
                    var allCreds = vault.FindAllByResource("BuildLight");
                    var credential = allCreds.FirstOrDefault();
                    if (credential != null)
                    {
                        credential.RetrievePassword();
                        txtUsername.Text = credential.UserName;
                        txtPassword.Password = credential.Password;
                    }
                    else
                    {
                        credentialsLoaded = false;
                    }
                }
                catch (Exception ex)
                {
                    credentialsLoaded = false;
                    await Log("Could not load credentials from password vault: " + ex.Message);
                }

                await Log("Trying to load settings file...");
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;

                bool fileExists = true;

                try
                {

                    StorageFile file = await localFolder.GetFileAsync("config.json");
                    _Settings = Util.GetObjectFromJson<Settings>(await FileIO.ReadTextAsync(file));

                }
                catch (FileNotFoundException)
                {
                    fileExists = false;
                }

                txtBuildName.Text = _Settings.BuildName;
                txtVSAccount.Text = _Settings.Account;
                txtVSProject.Text = _Settings.Project;
                txtR.Text = _Settings.RedPin.ToString();
                txtG.Text = _Settings.GreenPin.ToString();
                txtB.Text = _Settings.BluePin.ToString();



                return fileExists && credentialsLoaded;
            }
            catch (Exception ex)
            {
                _Telemetry.TrackError(ex);
                LogBackground("Error while loading settings: " + ex.Message);
                return false;
            }
        }

        private async Task InitialiaseLogfile()
        {
            try
            {

                StorageFolder localFolder = ApplicationData.Current.LocalFolder;

                _LogFile = await localFolder.CreateFileAsync(_LogFileName, CreationCollisionOption.ReplaceExisting);
            }
            catch (Exception ex)
            {
                _Telemetry.TrackError(ex);
                Debug.WriteLine("Failed to initialiase log file: " + ex.Message);
                throw;
            }
        }

        private async Task ReadLogfile()
        {
            try
            {
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;

                StorageFile file = await localFolder.GetFileAsync(_LogFileName);
                string contents = await FileIO.ReadTextAsync(file);
                await SetLogViewContents(contents);

            }
            catch (Exception ex)
            {
                _Telemetry.TrackError(ex);
                Debug.WriteLine(ex.Message);
            }
        }

        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            _Telemetry.TrackEvent("App.StartClick");

            UpdateSettings();
            await Start();


        }

        private async Task Start()
        {
            if (!_Running)
            {
                _CancellationToken = new CancellationTokenSource();

                if (_LEDListener == null)
                {
                    try
                    {
                        var gpio = Windows.Devices.Gpio.GpioController.GetDefault();
                        if (gpio != null)
                        {
                            _Telemetry.TrackEvent("LED.Found");
                            await Log("Initialising LED listener...");
                            _LEDListener = new LEDLightListener();
                            _LEDListener.Subscribe(_BuildMonitor, _Settings);
                            _LEDListener.Start();
                        }
                    }
                    catch (Exception ex)
                    {
                        _Telemetry.TrackError(ex);
                        Debug.WriteLine("Could not initialise LED listener: " + ex.Message);
                    }
                }

                _BuildMonitor.Start(
                    txtUsername.Text,
                    txtPassword.Password,
                    _Settings,
                    _CancellationToken.Token
                );

                _Running = true;
                tbRunStatus.Text = "State: Running";
                UpdateButtonStates();
            }

        }

        private void UpdateButtonStates()
        {
            if (_Running)
            {
                btnStart.IsEnabled = false;
                btnStop.IsEnabled = true;
            }
            else
            {
                btnStart.IsEnabled = true;
                btnStop.IsEnabled = false;
            }
        }


        private async void _BuildMonitor_RetrievingStatusEnd()
        {
            await SetLoadingVisibilityAsync(Visibility.Collapsed);
        }

        private async void _BuildMonitor_RetrievingStatusStart()
        {
            await SetLoadingVisibilityAsync(Visibility.Visible);
            await SetStatusColorAsync(Windows.UI.Colors.White);
        }

        private async void _BuildMonitor_StatusChanged(Status status)
        {
            switch (status)
            {
                case Status.Unknown:
                    _Telemetry.TrackEvent("Build.StatusError");
                    await SetStatusColorAsync(Windows.UI.Colors.Purple);
                    break;
                case Status.InProgress:
                    _Telemetry.TrackEvent("Build.InProgress");
                    await SetStatusColorAsync(Windows.UI.Colors.Blue);
                    break;
                case Status.PartiallySucceeded:
                    _Telemetry.TrackEvent("Build.PartiallySucceeded");
                    await SetStatusColorAsync(Windows.UI.Colors.Yellow);
                    break;
                case Status.Succeeded:
                    _Telemetry.TrackEvent("Build.Success");
                    await SetStatusColorAsync(Windows.UI.Colors.Green);
                    break;
                case Status.Failed:
                    _Telemetry.TrackEvent("Build.Fail");
                    await SetStatusColorAsync(Windows.UI.Colors.Red);
                    break;
                case Status.Cancelled:
                    _Telemetry.TrackEvent("Build.Cancelled");
                    await SetStatusColorAsync(Windows.UI.Colors.Cyan);
                    break;
                case Status.RetrievalError:
                    _Telemetry.TrackEvent("Build.StatusError");
                    await SetStatusColorAsync(Windows.UI.Colors.Purple);
                    break;
                default:
                    _Telemetry.TrackEvent("Build.StatusError");
                    await SetStatusColorAsync(Windows.UI.Colors.Purple);
                    break;

            }
        }

        private async Task SetStatusColorAsync(Windows.UI.Color color)
        {
            await PerformUICode(
            () =>
            {
                rectangleBackground.Fill = new SolidColorBrush(color);
            }
            );
        }


        private async Task SetLogViewContents(string contents)
        {
            await PerformUICode(
            () =>
            {
                txtLog.Text = contents;
            }
            );
        }

        private async Task SetLoadingVisibilityAsync(Visibility visibility)
        {
            await PerformUICode(() =>
            {
                tbLoading.Visibility = visibility;
            }
            );
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            settingsContainer.Visibility =
                settingsContainer.Visibility == Visibility.Visible
                ? Visibility.Collapsed : Visibility.Visible;

            if (settingsContainer.Visibility == Visibility.Visible)
            {
                await ReadLogfile();
            }
        }

        private async void btnStop_Click(object sender, RoutedEventArgs e)
        {

            _Telemetry.TrackEvent("App.StopClick");

            if (_Running)
            {
                _CancellationToken.Cancel();
            }

            while (_Running)
            {
                await Task.Delay(100);
            }

            UpdateButtonStates();
        }

        /// <summary>
        /// All UI work should be performed on the UI thread. Call this method to perform UI work. If the app is not yet loaded, no work will be performed.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private async Task PerformUICode(Action action)
        {
            //if the app is not loaded, just return
            if (!_Loaded)
            {
                return;
            }

            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                action();

            }
            );
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await InitialiaseLogfile();

            await Log("App loading...");

            _Telemetry.TrackEvent("App.Start");

            _BuildMonitor.StatusChanged += _BuildMonitor_StatusChanged;
            _BuildMonitor.RetrievingStatusStart += _BuildMonitor_RetrievingStatusStart;
            _BuildMonitor.RetrievingStatusEnd += _BuildMonitor_RetrievingStatusEnd;
            _BuildMonitor.Stopped += _BuildMonitor_Stopped;
            _BuildMonitor.Log += _BuildMonitor_Log;

            //default pin settings in case not configured
            _Settings.RedPin = 18;
            _Settings.GreenPin = 22;
            _Settings.BluePin = 24;



            bool settingsLoaded = await LoadSettings();
            if (settingsLoaded)
            {
                await Log("Starting...");
                await Start();
                settingsContainer.Visibility = Visibility.Collapsed;
            }

            _Loaded = true;

            await Log("App initialised...");
        }
    }
}
