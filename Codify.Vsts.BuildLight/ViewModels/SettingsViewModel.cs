using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Codify.Vsts.BuildLight.Extensions;
using System.ComponentModel;
using Codify.Vsts.BuildLight.Models;
using Codify.Vsts.BuildLight.UI;

namespace Codify.Vsts.BuildLight.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {

        public SettingsViewModel()
        {
            AvailableBuildCheckScales = new List<string>()
            {
                BuildCheckScale.Seconds.ToString(),
                BuildCheckScale.Minutes.ToString(),
                BuildCheckScale.Hours.ToString()
            };

            AddBuildCommand = new DelegateCommand(name => Settings.BuildNames.Add((string)name), name => !string.IsNullOrWhiteSpace((string)name));
            RemoveBuildCommand = new DelegateCommand(name => Settings.BuildNames.Remove((string)name), name => !string.IsNullOrWhiteSpace((string)name));
            SaveSettingsCommand = new DelegateCommand(async o => await SaveSettingsAsync());
        }

        public DelegateCommand AddBuildCommand { get; set; }

        public DelegateCommand RemoveBuildCommand { get; set; }

        public DelegateCommand SaveSettingsCommand { get; set; }

        public List<string> AvailableBuildCheckScales { get; set; }

        public Settings Settings {  get { return GetValue<Models.Settings>(); } set { SetValue(value); } }

        protected async Task LoadSettingsAsync()
        {
            List<Exception> exceptions = new List<Exception>();

            try
            {
                Settings = (await FileIO.ReadTextAsync(await GetSettingsFile())).ConvertJsonTo<Settings>();
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }

            if (Settings == null)
            {
                try
                {
                    Settings = new Settings();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (Settings != null)
            {
                GetCredentials();
                Settings.PropertyChanged += Settings_PropertyChanged;
            }
        }

        protected async Task SaveSettingsAsync()
        {
            var correlationId = Guid.NewGuid();

            try
            {
                await SaveCredentialsAsync();
                await FileIO.WriteTextAsync(await GetSettingsFile(true), Settings.GetJsonFromObject());
            }
            catch (Exception ex)
            {
            }
        }

        protected virtual async void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

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

        #region Manage Credentials

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

        #endregion

    }
}
