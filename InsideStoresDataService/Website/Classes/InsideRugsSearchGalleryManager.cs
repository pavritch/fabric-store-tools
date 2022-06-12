using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    public class InsideRugsSearchGalleryManager : SearchGalleryManager
    {
        public InsideRugsSearchGalleryManager(IWebStore store)
            : base(store)
        {

            // facet keys: Manufacturer, Style, Color, Size, Shape, Weave, Material, Pile Height, Price
        }

        protected override List<int> GetManufacturerList()
        {
            var list = base.GetManufacturerList();
            var excluded = new List<int>() { 200 }; // inside rugs itself
            list.RemoveAll(e => excluded.Contains(e));
            return list;
        }
    }
}