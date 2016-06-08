using Codify.Vsts.BuildLight.UI;
using System.Linq;
using Codify.Vsts.BuildLight.Services;
using System.Collections.ObjectModel;
using Codify.Vsts.BuildLight.Models;

namespace Codify.Vsts.BuildLight.ViewModels
{
    public class BuildStateViewModel : BaseViewModel
    {
        public BuildStateViewModel()
        {
            Builds = new ObservableCollection<BuildDetails>();
        }

        public ObservableCollection<BuildDetails> Builds { get; set; }

        protected async override void OnBuildEvent(object sender, BuildEventArgs e)
        {
            base.OnBuildEvent(sender, e);

            var definition = e.BuildDetails?.Definition;

            if ((definition != null) || !string.IsNullOrWhiteSpace(e.Name))
            {
                var build = Builds.FirstOrDefault(b => b.Definition == definition || b.Definition.Name.Equals(e.Name));

                if (e.Code == BuildEventCode.BuildInstanceRetrievalStart)
                {
                    if (build != null)
                    {
                        await PerformUICode(() => build.IsBusy = true);
                    }
                }
                else if (e.Code == BuildEventCode.BuildInstanceRetrievalEnd)
                {
                    if (build != null)
                    {
                        await PerformUICode(() => build.IsBusy = false);
                    }
                }
                else if (e.Code == BuildEventCode.StatusUpdate)
                {
                    if (build == null)
                    {
                        await PerformUICode(() => Builds.Add(e.BuildDetails));
                    }
                    else if (((build.CurrentBuild == null) && (e.BuildDetails.CurrentBuild != null)) || build.CurrentBuild.HasStatusChanged(e.BuildDetails.CurrentBuild))
                    {
                        await PerformUICode(() =>
                        {
                            Builds.Remove(build);
                            Builds.Insert(0, e.BuildDetails);
                        });
                    }
                }
            }
        }

        protected async override void OnServiceEvent(object sender, BuildEventArgs e)
        {
            base.OnServiceEvent(sender, e);

            if (e.Code == BuildEventCode.SkippedBuild)
            {
                await PerformUICode(() =>
                {
                    var build = Builds.FirstOrDefault(b => b.Definition.Name.Equals(e.Name));
                    if (build != null)
                    {
                        Builds.Remove(build);
                    }
                });
            }

        }
    }
}
