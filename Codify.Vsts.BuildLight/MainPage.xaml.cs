
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Codify.Vsts.BuildLight
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            ViewModel = new MainPageViewModel();
            this.Loaded += async (sender, e) =>
            {
                await Task.Delay(1000);
                await Task.Run(() => ViewModel.InitialiseAsync());
            };
            this.InitializeComponent();
        }

        public MainPageViewModel ViewModel { get; set; }

    }
}
