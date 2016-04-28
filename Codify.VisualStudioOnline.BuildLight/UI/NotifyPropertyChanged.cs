
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace Codify.VisualStudioOnline.BuildLight.UI
{
    public class NotifyPropertyChanged : INotifyPropertyChanged
    {
        private Dictionary<string, object> backingFields;

        public bool IsBusy { get { return GetValue<bool>(); } set { SetValue(value); } }

        public NotifyPropertyChanged()
        {
            backingFields = new Dictionary<string, object>();
        }

        protected T GetValue<T>([CallerMemberName]string propertyName = null)
        {
            var value = default(T);

            if (backingFields.ContainsKey(propertyName))
            {
                value = (T)backingFields[propertyName];
            }

            return value;
        }

        protected void SetValue<T>(T value, [CallerMemberName]string propertyName = null)
        {
            if (backingFields.ContainsKey(propertyName))
            {
                backingFields.Remove(propertyName);
            }

            backingFields.Add(propertyName, value);
            Task.Run(() => OnPropertyChanged(propertyName));
        }

        public async virtual Task OnPropertyChanged(string propertyName)
        {
            await PerformUICode(() =>
            {
                var handler = PropertyChanged;
                if (handler != null)
                {
                    handler.Invoke(this, new PropertyChangedEventArgs(propertyName));
                }
            });
        }

        /// <summary>
        /// All UI work should be performed on the UI thread. Call this method to perform UI work. If the app is not yet loaded, no work will be performed.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        protected async Task PerformUICode(Action action)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action());
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
