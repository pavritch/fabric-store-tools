using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace Website
{
    public class NewProductsByManufacturerAutoSuggestQuery
    {
        public string Query { get; set; }

        public AutoSuggestMode Mode { get; set; }

        public int? ManufacturerID { get; set; }
        public int Days { get; set; }

        public int Take { get; set; }

        public Action<string, List<string>> CompletedAction { get; set; }

        public NewProductsByManufacturerAutoSuggestQuery()
        {
            Take = 100;
        }
    }
}