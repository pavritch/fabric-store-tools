using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization.Formatters;

namespace ShopifyCommon
{
    public class ProductInformationManager
    {
        private bool isLoaded = false;
        private bool isSnapshotLoaded = false;

        private List<ProductInformation> productList = new List<ProductInformation>();
        private List<ProductInformation> productSnapshotList = new List<ProductInformation>();
        private string filePath; // abs path to where product file is persisted
        private bool isCanada;

        public ProductInformationManager(string filePath, bool isCanada=false)
        {
            this.filePath = filePath;
            this.isCanada = isCanada;
        }

        public void Load()
        {
            isLoaded = true;
            productList.Clear();
            if (!File.Exists(filePath))
                return;

            var json = File.ReadAllText(filePath);
            productList = Deserialize(json);
        }

        public bool LoadSnapshot()
        {
            isSnapshotLoaded = true;
            productSnapshotList.Clear();
            if (!File.Exists(SnapshotFilepath))
                return false;

            var json = File.ReadAllText(SnapshotFilepath);
            productSnapshotList = Deserialize(json);
            return true;
        }

        public void Save()
        {
            var json = Serialize();
            File.WriteAllText(filePath, json);
        }

        private string SnapshotFilepath
        {
            get
            {
                return Path.Combine(Path.GetDirectoryName(filePath), isCanada ? "ShopifyProductsSnapshot-CA.json" : "ShopifyProductsSnapshot.json");
            }
        }
        public void SaveSnapshot()
        {
            var json = Serialize();
            File.WriteAllText(SnapshotFilepath, json);
        }

        public List<ProductInformation> Products
        {
            get
            {
                if (!isLoaded)
                    Load();

                return productList;
            }
        }

        public List<ProductInformation> SnapshotProducts
        {
            get
            {
                if (!isSnapshotLoaded)
                    LoadSnapshot();

                return productSnapshotList;
            }
        }

        public void AddProduct(ProductInformation product)
        {
            if (!isLoaded)
                Load();

            productList.Add(product);
        }

        public void DeleteProductFile()
        {
            isLoaded = false;
            File.Delete(filePath);
            productList.Clear();
        }

        public void DeleteSnapshotProductFile()
        {
            isSnapshotLoaded = false;
            File.Delete(SnapshotFilepath);
            productSnapshotList.Clear();
        }


        #region Serializer Support

        private string Serialize()
        {
            string json = JsonConvert.SerializeObject(productList, Formatting.Indented, SerializerSettings);
            return json;
        }

        private List<ProductInformation> Deserialize(string json)
        {
            var data = JsonConvert.DeserializeObject<List<ProductInformation>>(json, SerializerSettings);
            return data;
        }

        /// <summary>
        /// Common settings used for serialization/deserialization.
        /// </summary>
        private JsonSerializerSettings SerializerSettings
        {
            get
            {
                var jsonSettings = new JsonSerializerSettings
                {
                    Converters = new List<JsonConverter>() { new IsoDateTimeConverter() },
                    TypeNameHandling = TypeNameHandling.None,
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
