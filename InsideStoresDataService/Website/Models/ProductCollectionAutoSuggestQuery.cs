using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace Website
{
    public class ProductCollectionAutoSuggestQuery
    {
        public string Query { get; set; }

        public AutoSuggestMode Mode { get; set; }

        public int CollectionID { get; set; }

        public int Take { get; set; }

        public Action<string, List<string>> CompletedAction { get; set; }

        public ProductCollectionAutoSuggestQuery()
        {
            Take = 100;
        }
    }
}