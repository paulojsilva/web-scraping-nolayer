using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json
{
    public static class JsonConvertExtension
    {
        public static string SerializeObject(object value)
        {
            if (value == null) return null;

            return JsonConvert.SerializeObject(value, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        public static string SerializeCamelCaseObject(object value)
        {
            if (value == null) return null;

            return JsonConvert.SerializeObject(value, new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }

        public static T DeserializeObject<T>(string value)
        {
            if (value == null) return default(T);
            return JsonConvert.DeserializeObject<T>(value);
        }
    }
}
