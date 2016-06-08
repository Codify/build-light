
using Codify.Vsts.BuildLight.Services;
using Codify.Vsts.BuildLight.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Codify.Vsts.BuildLight.UI.Controls
{
    public sealed partial class BuildStateView : UserControl
    {
        public static readonly DependencyProperty BuildServiceProperty = DependencyProperty.Register("BuildService", typeof(BuildService), typeof(BuildStateView), new PropertyMetadata(null, OnBuildServiceChanged));

        public BuildStateView()
        {
            ViewModel = new BuildStateViewModel();
            this.InitializeComponent();
        }

        public BuildStateViewModel ViewModel { get; set; }

        public BuildService BuildService { get { return (BuildService)GetValue(BuildServiceProperty); } set { SetValue(BuildServiceProperty, value); } }

        private static void OnBuildServiceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as BuildStateView;
            if (control != null)
            {
                control.ViewModel.BuildService = (BuildService)e.NewValue;
            }
        }
    }
}
