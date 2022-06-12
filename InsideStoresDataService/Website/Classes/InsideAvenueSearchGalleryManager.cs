using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    public class InsideAvenueSearchGalleryManager : SearchGalleryManager
    {
        public InsideAvenueSearchGalleryManager(IWebStore store)
            : base(store)
        {

            // facet keys:  Manufacturer, Decor, Furniture, Lighting, Home Improvement, Outdoor, Price
        }

        protected override List<int> GetManufacturerList()
        {
            var list = base.GetManufacturerList();
            var excluded = new List<int>() { 120 }; // your other warehouse
            list.RemoveAll(e => excluded.Contains(e));
            return list;
        }

    }
}