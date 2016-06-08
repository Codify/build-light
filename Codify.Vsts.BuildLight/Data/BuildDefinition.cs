using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Codify.Vsts.BuildLight.Data
{
    public class BuildDefinition
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("queueStatus")]
        public string QueueStatus { get; set; }

        [JsonIgnore]
        public bool IsDisabled {  get { return !string.IsNullOrWhiteSpace(QueueStatus) && QueueStatus.Equals("disabled", StringComparison.OrdinalIgnoreCase); } }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var otherDefinition = obj as BuildDefinition;

            return (otherDefinition != null) && (otherDefinition.Id == Id);
        }
    }

    public class BuildDefinitionList
    {
        public BuildDefinitionList()
        {
            Definitions = new List<BuildDefinition>();
        }

        [JsonProperty("value")]
        public List<BuildDefinition> Definitions { get; set; }
    }
}
