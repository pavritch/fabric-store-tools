using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Runtime.Serialization.Formatters;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace InsideFabric.Data
{
    /// <summary>
    /// Class used to hold json serializable data for InsideFabric.Products.ExtensionData4 SQL column.
    /// </summary>
    /// <remarks>
    /// This class is essentially a dictionary of objects - so it can hold pretty much anything.
    /// </remarks>
    [JsonObject(MemberSerialization.OptIn)]
    public class ExtensionData4
    {
        // these are the original properties parsed out from the HTML p.Description column
        // prior to doing any meaningful reorganization to some new unified taxonomy.
        // Keeping around for a little bit until the dust settles.
        public const string OriginalRawProperties = "OriginalRawProperties";

        public const string ManufacturerImageUrl = "ManufacturerImageUrl";
        public const string ManufacturerAlternateImageUrl = "ManufacturerAlternateImageUrl";

        /// <summary>
        /// List[ProductImage] which is a set of image definitions acquired by scanner.
        /// </summary>
        public const string ProductImages = "ProductImages";

        // List[string]
        public const string AvailableImageFilenames = "AvailableImageFilenames";

        public const string PrivateProperties = "PrivateProperties";

        // typesafe classes used to keep information about rug products
        public const string RugProductFeatures = "RugProductFeatures";
        public const string RugProductVariantFeatures = "RugProductVariantFeatures";

        // typesafe class used to keep information about each InsideAvenue product. Type is HomewareProductFeatures
        public const string HomewareProductFeatures = "HomewareProductFeatures";

        // manual edits for homeware products to survive scanner updates
        public const string HumanHomewareProductFeatures = "HumanHomewareProductFeatures";

        // key for property class which has the digital image feature analysis for the primary
        // image associated with this product. If missing, then could be haven't downloaded
        // image yet, product has no image, etc. The intent is that every product with some 
        // known image will have one of these feature sets.
        public const string ProductImageFeatures = "ProductImageFeatures";

        /// <summary>
        /// Detailed data on each processed image which is live and available. List[LiveProductImage]
        /// </summary>
        /// <remarks>
        /// In theory, this is a richer superset of AvailableImageFilenames.
        /// </remarks>
        public const string LiveProductImages = "LiveProductImages";

        /// <summary>
        /// A Dic[string, int] of the shapes available for this rug and the variant count. Missing means 0. Include sample.
        /// </summary>
        public const string RugShapes = "RugShapes";


        // Private Properties is a dic<string, string> of various things to keep - filled in by scanner (mostly)

        public class PrivatePropertiesKeys
        {
            // populated by scanner, when the record was inserted/full_update, alerting us to when we might need to trigger other actions
            // should always be there since this is technically a required field.
            public const string LastFullUpdate = "Last Full Update"; // DateTime.Now.ToShortDateString();

            // when we last freshened the ProductCategory associations for this product. In theory, if this is ever missing or less than
            // LastFullUpdate, then we'll need to make another run of it for this product to bring the associations up to date in case
            // something in the product record changed.
            // will only be there once we've actually done some categories for this product.
            public const string LastClassifyUpdate = "LastClassifyUpdate"; // DateTime.Now.ToShortDateString();

            /// <summary>
            /// Flag set to 'true' on Insert/FullUpdate product scan to use as a trigger for reclassification.
            /// </summary>
            /// <remarks>
            /// If missing or false is the same as false.
            /// </remarks>
            public const string RequiresClassifyUpdate = "RequiresClassifyUpdate"; // bool string


            // web page for the vendor details on this product
            public const string ProductDetailUrl = "Product Detail URL";

            // vendor has indicated there is limited stock on this item
            public const string IsLimitedAvailability = "IsLimitedAvailability";

            // When this key is present and the value is "true", then indicates the
            // product record has been manually edited, and these changes should be preserved.
            public const string HasBeenEdited = "HasBeenEdited";
        }
        


#if false // these keys are now obsolete, and we don't need any data associated with them in SQL
        // since we are about to spin up new values for Description, SEKeywords and SEDescription,
        // wanted to keep a copy of the original around for a little bit.
        public const string OriginalSEDescription = "OriginalSEDescription";
        public const string OriginalSEKeywords = "OriginalSEKeywords";
        public const string OriginalDescription = "OriginalDescription";

        // indicates if spun up versions of these fields have been generated
        // so can determine when should be if not already
        public const string IsSpunDescription = "IsSpunDescription";
        public const string IsSpunSEDescription = "IsSpunSEDescription";
        public const string IsSpunSEKeywords = "IsSpunSEKeywords";
#endif

        [JsonProperty]
        public Dictionary<string, object> Data { get; set; }

        public ExtensionData4()
        {
            Data = new Dictionary<string, object>();
        }

        #region Serializer Support
		
        public string Serialize()
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented, SerializerSettings);
            //Debug.WriteLine(json);
            return json;
        }

        public static ExtensionData4 Deserialize(string json)
        {
            var data = JsonConvert.DeserializeObject<ExtensionData4>(json, SerializerSettings);
            return data;
        }

        /// <summary>
        /// Common settings used for serialization/deserialization.
        /// </summary>
        public static JsonSerializerSettings SerializerSettings
        {
            get
            {
                var jsonSettings = new JsonSerializerSettings
                {
                    Converters = new List<JsonConverter>() { new IsoDateTimeConverter()},
                    TypeNameHandling = TypeNameHandling.All,
                    TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    NullValueHandling = NullValueHandling.Include,
                };

                return jsonSettings;
            }
        }

	    #endregion 
    }
}
