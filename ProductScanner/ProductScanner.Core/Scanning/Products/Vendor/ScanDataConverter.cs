using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ProductScanner.Core.Scanning.Products.Vendor
{
    // used to customize serialization for ScanData - otherwise it doesn't contain typed properties
    // I think the cleanest solution is to move ScanData away from being a Dictionary
    // this is kind of ugly because any new properties in ScanData requires a change here
    public class ScanDataConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof (ScanData);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var temp = serializer.Deserialize<Dictionary<ScanField, string>>(reader);
            var eobj = new ScanData();
            foreach (var key in temp.Keys)
            {
                if (key == ScanField.Prop_Cost) eobj.Cost = Convert.ToDecimal(temp[key]);
                else
                    eobj.Add(key, temp[key]);
            }
            return eobj;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var eobj = (ScanData)value;
            var temp = new Dictionary<ScanField, string>(eobj);
            temp.Add(ScanField.Prop_Cost, eobj.Cost.ToString());
            serializer.Serialize(writer, temp);
        }
    }
}