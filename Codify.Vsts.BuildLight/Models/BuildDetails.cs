using Codify.Vsts.BuildLight.Data;
using Codify.Vsts.BuildLight.UI;
using System;

namespace Codify.Vsts.BuildLight.Models
{
    public class BuildDetails : NotifyPropertyChanged
    {
        public BuildDefinition Definition { get { return GetValue<BuildDefinition>(); } set { SetValue(value); } }

        public BuildInstance CurrentBuild { get { return GetValue<BuildInstance>(); } set { SetValue(value); } }

        public BuildInstance PreviousBuild { get { return GetValue<BuildInstance>(); } set { SetValue(value); } }

        public override int GetHashCode()
        {
            return Definition.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var otherDetails = obj as BuildDetails;
            return otherDetails != null && otherDetails.Definition == Definition;
        }
    }
}
