using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codify.VisualStudioOnline.BuildLight
{
    class BuildList
    {
        [JsonProperty(PropertyName = "value")]
        public IEnumerable<Build> Builds { get; set; }
    }

    class Build
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "result")]
        public string ResultStatus { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string ProgressStatus { get; set; }


    }

    class BuildDefinitionList
    {
        [JsonProperty(PropertyName = "value")]
        public IEnumerable<BuildDefinition> Definitions { get; set; }

    }

    class BuildDefinition
    {

        public int Id { get; set; }
        public string Name { get; set; }
    }


    class BuildResultStatus
    {
        public const string Canceled = "canceled";
        public const string PartiallySucceeded = "partiallySucceeded";
        public const string Succeeded = "succeeded";
        public const string Failed = "failed";
    }

    class BuildProgressStatus
    {
        public const string Completed = "completed";
        public const string InProgress = "inProgress";
    }
}
