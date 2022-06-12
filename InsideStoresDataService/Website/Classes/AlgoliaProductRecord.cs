using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    /// <summary>
    /// The actions to be performed against the algolia database to sync up.
    /// </summary>
    public enum AlgoliaAction
    {
        None = 0,
        Upsert = 1,
        Delete = 2
    }

    public class AlgoliaProductRecord
    {
        // this is the string of our ProductID
        public string objectID { get; set; }
        public string sku { get; set; }
        public string name { get; set; }
        public string brand { get; set; }
        public string mpn { get; set; }
        public string upc { get; set; }
        public bool isLive { get; set; }
        public int rank { get; set; }

        public List<string> categories { get; set; }
        public List<string> properties { get; set; }

        public AlgoliaProductRecord(int productID)
        {
            objectID = productID.ToString();
            categories = new List<string>();
            properties = new List<string>();
            rank = 0;
        }

        public void AddProperty(string value)
        {
            properties.Add(value);
        }

        public void AddCategory(string value)
        {
            categories.Add(value);
        }

    }
}