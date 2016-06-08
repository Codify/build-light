using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codify.Vsts.BuildLight.Extensions
{
    public static class ObjectExtensions
    {
        public static string GetJsonFromObject(this object item)
        {
            return JsonConvert.SerializeObject(item, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, TypeNameHandling = TypeNameHandling.Auto });
        }
    }
}
