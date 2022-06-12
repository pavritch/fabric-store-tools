using Newtonsoft.Json;

namespace ReportingEngine.Classes
{
    [JsonObject(MemberSerialization.OptIn)]
    public class PropertyRecord
    {
        /// <summary>
        /// Manufacturer Part Number
        /// </summary>
        /// <remarks>
        /// Unique identifier.
        /// </remarks>
        public string MPN { get; set; }

        /// <summary>
        /// Property name.
        /// </summary>
        [JsonProperty]
        public string Name { get; set; }

        /// <summary>
        /// Property value.
        /// </summary>
        [JsonProperty]
        public string Value { get; set; }

        public PropertyRecord()
        {

        }

        public PropertyRecord(string MPN, string Name, string Value)
        {
            this.MPN = MPN;
            this.Name = Name;
            this.Value = Value;
        }
    }
}
