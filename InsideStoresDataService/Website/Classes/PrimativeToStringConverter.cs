using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace Website
{
    // not used

    /// <summary>
    /// A formatter that will take all the values of the object and output them as quoted strings rather than plain numbers, etc.
    /// </summary>
    class PrimitiveToStringConverter : JsonConverter
    {
        //string json = JsonConvert.SerializeObject(myObject, new PrimitiveToStringConverter());

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsPrimitive;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString().ToLower());
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}