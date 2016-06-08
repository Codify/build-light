using Newtonsoft.Json;
using System.Collections.Generic;
using Codify.Vsts.BuildLight.Extensions;
using System;

namespace Codify.Vsts.BuildLight.Data
{
    public enum BuildProgressStatus
    {
        Unknown = 0,
        Completed = 1,
        InProgress = 2
    }

    public enum BuildResultStatus
    {
        Unknown = 0,
        RetrievalError = 1,
        Succeeded = 2,
        PartiallySucceeded = 3,
        InProgress = 4,
        Cancelled = 5,
        Failed = 6
    }

    public class BuildInstanceList
    {
        [JsonProperty("value")]
        public IEnumerable<BuildInstance> Builds { get; set; }
    }

    public class BuildUser
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("uniqueName")]
        public string UniqueName { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("imageUrl")]
        public string ImageUrl { get; set; }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var otherUser = obj as BuildUser;
            return otherUser != null && otherUser.Id == Id;
        }
    }

    public class BuildInstance
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("result")]
        public string Result { get; set; }

        [JsonProperty("buildNumber")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("status")]
        public string Progress { get; set; }

        [JsonProperty("definition")]
        public BuildDefinition Definition { get; set; }

        [JsonProperty("queueTime")]
        public DateTime QueueTime { get; set; }

        [JsonProperty("startTime")]
        public DateTime? StartTime { get; set; }

        [JsonIgnore]
        public string FormattedStartTime { get { return StartTime.HasValue ? StartTime.Value.ToString("dd MMM yyyy HH:mm:ss") : "Not started"; } }

        [JsonProperty("finishTime")]
        public DateTime? FinishTime { get; set; }

        [JsonIgnore]
        public string FormattedFinishTime { get { return FinishTime.HasValue ? FinishTime.Value.ToString("dd MMM yyyy HH:mm:ss") : "Not finished"; } }

        [JsonProperty("sourceBranch")]
        public string SourceBranch { get; set; }

        [JsonProperty("sourceVersion")]
        public string SourceVersion { get; set; }

        [JsonProperty("requestedFor")]
        public BuildUser RequestedFor { get; set; }

        [JsonProperty("requestedBy")]
        public BuildUser RequestedBy { get; set; }

        [JsonIgnore]
        public BuildProgressStatus ProgressStatus { get { return Progress.ConvertToBuildProgressStatus(); } }

        [JsonIgnore]
        public BuildResultStatus ResultStatus { get { return Result.ConvertToBuildResultStatus(); } }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var otherInstance = obj as BuildInstance;
            return otherInstance != null && otherInstance.Id == Id;
        }

        public bool HasStatusChanged(BuildInstance otherInstance)
        {
            return otherInstance == null || otherInstance.Progress != Progress || otherInstance.Result != Result;
        }
    }
}
