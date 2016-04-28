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
        public MainPage()
        {
            ViewModel = new MainPageViewModel();

            this.Loaded += (sender, e) => ViewModel.Initialise();
            this.InitializeComponent();
        }

        public MainPageViewModel ViewModel { get; set; }

    }
}
