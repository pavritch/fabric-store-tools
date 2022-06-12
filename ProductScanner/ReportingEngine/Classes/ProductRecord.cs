using Newtonsoft.Json;

namespace ReportingEngine.Classes
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ProductRecord
    {
        /// <summary>
        /// Inside Fabric SKU
        /// </summary>
        /// <remarks>
        /// Unique identifier.
        /// </remarks>
        public string SKU { get; set; }

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

        public ProductRecord()
        {

        }

        public ProductRecord(string SKU, string Name, string Value)
        {
            this.SKU = SKU;
            this.Name = Name;
            this.Value = Value;
        }
    }
}
