using Codify.Vsts.BuildLight.Data;
using Newtonsoft.Json;
using System;

namespace Codify.Vsts.BuildLight.Extensions
{
    public static class StringExtensions
    {
        public static BuildResultStatus ConvertToBuildResultStatus(this string statusName)
        {
            var status = BuildResultStatus.Unknown;

            if ("canceled".Equals(statusName, StringComparison.OrdinalIgnoreCase))
            {
                status = BuildResultStatus.Cancelled;
            }
            else if ("partiallySucceeded".Equals(statusName, StringComparison.OrdinalIgnoreCase))
            {
                status = BuildResultStatus.PartiallySucceeded;
            }
            else if ("succeeded".Equals(statusName, StringComparison.OrdinalIgnoreCase))
            {
                status = BuildResultStatus.Succeeded;
            }
            else if ("failed".Equals(statusName, StringComparison.OrdinalIgnoreCase))
            {
                status = BuildResultStatus.Failed;
            }

            return status;
        }

        public static BuildProgressStatus ConvertToBuildProgressStatus(this string statusName)
        {
            var status = BuildProgressStatus.Unknown;

            if ("inProgress".Equals(statusName, StringComparison.OrdinalIgnoreCase))
            {
                status = BuildProgressStatus.InProgress;
            }
            else if ("completed".Equals(statusName, StringComparison.OrdinalIgnoreCase))
            {
                status = BuildProgressStatus.Completed;
            }

            return status;
        }

        public static T ConvertJsonTo<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
        }
    }
}
