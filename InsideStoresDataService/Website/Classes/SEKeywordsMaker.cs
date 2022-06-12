using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Website
{

    public class SEKeywordsMaker
    {
        public string SEKeywords { get; private set; }

        /// </remarks>
        private static readonly string[] KeywordsKeepList = new string[]
        {
            "Brand",
            "Item Number",
            "Pattern",
            "Pattern Name",
            "Pattern Number",
            "Product", // Greenhouse and Stout use Product rather than pattern. And combine like: 99045 Black Swan
            "Color",
            "Color Name",
            "Color Group",
            "Collection",
            "Designer",
            "Category",
            "Group",
            "Product Use",
            "Product Type",
            "Type",
            "Style",
            "Use",
        };

        public SEKeywordsMaker(InsideFabricProduct product)
        {
            var listKeywords = new List<string>();
            var hashKeywords = new List<string>();

            Action<string> add = (s) =>
                {
                    // must have a value, must not be same as any existing value
                    // but we keep the ordered list

                    if (!string.IsNullOrWhiteSpace(s) && !hashKeywords.Contains(s))
                    {
                        listKeywords.Add(s);
                        hashKeywords.Add(s);
                    }
                };

            var props = product.OriginalRawProperties;

            add(product.ProductGroup);

            var manufName = product.m.Name.Replace(" Fabrics", "").Replace(" Fabric", "");
            add(manufName);

            if (!string.IsNullOrEmpty(product.ManufacturerPartNumber))
                add(product.ManufacturerPartNumber.ToUpper());

            foreach (var key in KeywordsKeepList.Intersect(props.Keys))
                add(props[key]);

            foreach (var s in product.FabricColors)
                add(s.Replace(" Fabric", ""));

            foreach (var s in product.FabricTypes)
                add(s.Replace(" Fabric", ""));

            foreach (var s in product.FabricPatterns)
                add(s.Replace(" Fabric", ""));

            add(product.SKU);

            SEKeywords = listKeywords.ToCommaDelimitedList();

        }

        public SEKeywordsMaker(InsideWallpaperProduct product)
        {
            var listKeywords = new List<string>();
            var hashKeywords = new List<string>();

            Action<string> add = (s) =>
            {
                // must have a value, must not be same as any existing value
                // but we keep the ordered list

                if (!string.IsNullOrWhiteSpace(s) && !hashKeywords.Contains(s))
                {
                    listKeywords.Add(s);
                    hashKeywords.Add(s);
                }
            };

            var props = product.OriginalRawProperties;

            add(product.ProductGroup);

            var manufName = product.m.Name.Replace(" Fabrics", "").Replace(" Fabric", "");
            add(manufName);

            if (!string.IsNullOrEmpty(product.ManufacturerPartNumber))
                add(product.ManufacturerPartNumber.ToUpper());

            foreach (var key in KeywordsKeepList.Intersect(props.Keys))
                add(props[key]);

            foreach (var s in product.FabricColors)
                add(s.Replace(" Fabric", ""));

            foreach (var s in product.FabricTypes)
                add(s.Replace(" Fabric", ""));

            foreach (var s in product.FabricPatterns)
                add(s.Replace(" Fabric", ""));

            add(product.SKU);

            SEKeywords = listKeywords.ToCommaDelimitedList();

        }

    }
}
