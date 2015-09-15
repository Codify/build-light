using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codify.VisualStudioOnline.BuildLight
{
    public static class Util
    {
        public static string GetJsonFromObject(object item)
        {
            string json = JsonConvert.SerializeObject(item, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, TypeNameHandling = TypeNameHandling.Auto });
            return json;
        }


        public static T GetObjectFromJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
        }
    }
}
