using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Website
{
    /// <summary>
    /// For control panel dashboard.
    /// </summary>
    [DataContract]
    public class WebStoreInformation
    {
        [DataMember]
        public StoreKeys StoreKey { get; set; }

        [DataMember]
        public string FriendlyName { get; set; }

        [DataMember]
        public string Domain { get; set; }

        [DataMember]
        public int ProductCount { get; set; }

        [DataMember]
        public int FeaturedProductCount { get; set; }

        [DataMember]
        public int CategoryCount { get; set; }

        [DataMember]
        public int ManufacturerCount { get; set; }

        [DataMember]
        public DateTime? TimeWhenPopulationCompleted { get; set; }

        [DataMember]
        public TimeSpan? TimeToPopulate { get; set; }


        public WebStoreInformation()
        {

        }

        public WebStoreInformation(IWebStore store)
        {
            var productData = store.ProductData;

            StoreKey = store.StoreKey;
            FriendlyName = store.FriendlyName;
            Domain = store.Domain;
            ProductCount = productData.ProductsForSaleCount;
            FeaturedProductCount = productData.FeaturedProducts.Count();
            CategoryCount = productData.Categories.Count();
            ManufacturerCount = productData.Manufacturers.Count();
            TimeWhenPopulationCompleted = productData.TimeWhenPopulationCompleted;
            TimeToPopulate = productData.TimeToPopulate;
        }

    }
}