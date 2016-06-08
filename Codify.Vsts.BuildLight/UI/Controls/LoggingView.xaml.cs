using Codify.Vsts.BuildLight.Models;
using Codify.Vsts.BuildLight.Services;
using Codify.Vsts.BuildLight.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Codify.Vsts.BuildLight.UI.Controls
{

    public sealed partial class LoggingView : UserControl
    {
        public static readonly DependencyProperty BuildServiceProperty = DependencyProperty.Register("BuildService", typeof(BuildService), typeof(LoggingView), new PropertyMetadata(null, OnBuildServiceChanged));

        public LoggingView()
        {
            ViewModel = new LoggingViewModel();
            this.InitializeComponent();
        }

        public LoggingViewModel ViewModel { get; set; }

        public BuildService BuildService { get { return (BuildService)GetValue(BuildServiceProperty); } set { SetValue(BuildServiceProperty, value); } }

        private static void OnBuildServiceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as LoggingView;
            if (control != null)
            {
                control.ViewModel.BuildService = (BuildService)e.NewValue;
            }
        }
    }
}
