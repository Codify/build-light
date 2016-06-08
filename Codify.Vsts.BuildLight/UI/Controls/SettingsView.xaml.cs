using Codify.Vsts.BuildLight.Models;
using Codify.Vsts.BuildLight.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Codify.Vsts.BuildLight.UI.Controls
{
    public sealed partial class SettingsView : UserControl
    {
        public static readonly DependencyProperty SettingsProperty = DependencyProperty.Register("Settings", typeof(Settings), typeof(SettingsView), new PropertyMetadata(null, OnSettingsChanged));

        public SettingsView()
        {
            ViewModel = new SettingsViewModel();
            this.InitializeComponent();
        }

        public SettingsViewModel ViewModel { get; set; }

        public Settings Settings { get { return (Settings)GetValue(SettingsProperty); } set { SetValue(SettingsProperty, value); } }

        private static void OnSettingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as SettingsView;
            if (control != null)
            {
                control.ViewModel.Settings = (Settings)e.NewValue;
            }
        }
    }
}
