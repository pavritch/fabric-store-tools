using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Utilities.Extensions;

namespace Utilities
{
    public static class MemoryStreamExtensions
    {
        public static void Append(this MemoryStream stream, byte value)
        {
            stream.Append(new[] { value });
        }

        public static void Append(this MemoryStream stream, byte[] values)
        {
            stream.Write(values, 0, values.Length);
        }
    }

    public static class JSONHelper
    {

        public static string ToJSON(this object obj, JsonSerializerSettings settings)
        {
            return JsonConvert.SerializeObject(obj, settings);
        }

        public static string ToJSON(this object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }

        public static byte[] ToCompressedJSON<T>(this List<T> list)
        {
            var stream = new MemoryStream();
            stream.Append("[\n".ToByteArray());
            stream.Append(JsonConvert.SerializeObject(list.First(), Formatting.Indented).ToByteArray());
            foreach (var item in list.Skip(1))
            {
                stream.Append((",\n" + JsonConvert.SerializeObject(item, Formatting.Indented)).ToByteArray());
            }
            stream.Append("\n]".ToByteArray());
            return stream.ToArray().GZipMemory();
        }

        public static T JSONtoList<T>(this string jsonObj)
        {
            var jsserializer = new JavaScriptSerializer();
            jsserializer.MaxJsonLength = 999999999;
            return jsserializer.Deserialize<T>(jsonObj);
        }

        public static T FromJSON<T>(this string jsonObj)
        {
            return JsonConvert.DeserializeObject<T>(jsonObj);
        }

        public static object FromJSON(this string jsonObj)
        {
            return JsonConvert.DeserializeObject(jsonObj, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All});
        }
    }
}