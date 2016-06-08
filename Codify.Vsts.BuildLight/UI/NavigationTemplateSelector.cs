using Codify.Vsts.BuildLight;
using Codify.Vsts.BuildLight.Models;
using Codify.Vsts.BuildLight.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Codify.Vsts.BuildLight.UI
{
    public class NavigationTemplateSelector : DataTemplateSelector
    {

        public DataTemplate BuildStateTemplate { get; set; }

        public DataTemplate LogsTemplate { get; set; }

        public DataTemplate SettingsTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            DataTemplate template = null;

            if ((item == null) || ((AvailableViews)item == AvailableViews.BuildState))
            {
                template = BuildStateTemplate;
            }
            else if ((AvailableViews)item == AvailableViews.Logging)
            {
                template = LogsTemplate;
            }
            else
            {
                template = SettingsTemplate;
            }

            return template;
        }
    }
}
