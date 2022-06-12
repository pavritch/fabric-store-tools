using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Website
{
    [JsonObject(MemberSerialization.OptOut)]
    public class SearchCriteria
    {
        // list of manufacturer IDs
        public List<int>ManufacturerList {get;set;}
        
        // lists of category IDs
        public List<int>TypeList {get;set;}
        public List<int>PatternList {get;set;}
        public List<int>ColorList {get;set;}
        public List<int>PriceRangeList {get;set;}
        
        // text phrases
        public string Keywords {get;set;}
        public string PartNumber {get;set;}
        public string Collection {get;set;}
        public string ColorName {get;set;}

        public SearchCriteria()
        {
            Keywords = string.Empty;
            PartNumber = string.Empty;
            Collection = string.Empty;
            ColorName = string.Empty;
            ManufacturerList = new List<int>();
            TypeList = new List<int>();
            PatternList = new List<int>();
            ColorList = new List<int>();
            PriceRangeList = new List<int>();
        }

        public SearchCriteria(HttpContextBase HttpContext)
        {
            var request = HttpContext.Request;

            if (request.RequestType == "GET")
            {
                // strings will be null if not populated

                Keywords = ParseString(request.QueryString["Keywords"]);
                PartNumber = ParseString(request.QueryString["PartNumber"]);
                Collection = ParseString(request.QueryString["Collection"]);
                ColorName = ParseString(request.QueryString["ColorName"]);

                // collections will be empty collection if not populated

                ManufacturerList = ParseIntList(request.QueryString["BrandList"]);
                TypeList = ParseIntList(request.QueryString["TypeList"]);
                PatternList = ParseIntList(request.QueryString["PatternList"]);
                ColorList = ParseIntList(request.QueryString["ColorList"]);
                PriceRangeList = ParseIntList(request.QueryString["PriceRangeList"]);
            }
            else // post
            {
                // strings will be null if not populated

                Keywords = ParseString(request.Form["Keywords"]);
                PartNumber = ParseString(request.Form["PartNumber"]);
                Collection = ParseString(request.Form["Collection"]);
                ColorName = ParseString(request.Form["ColorName"]);

                // collections will be empty collection if not populated

                ManufacturerList = ParseIntList(request.Form["BrandList"]);
                TypeList = ParseIntList(request.Form["TypeList"]);
                PatternList = ParseIntList(request.Form["PatternList"]);
                ColorList = ParseIntList(request.Form["ColorList"]);
                PriceRangeList = ParseIntList(request.Form["PriceRangeList"]);
            }
        }

  
        private string ParseString(string Parameter)
        {
            if (string.IsNullOrWhiteSpace(Parameter))
                return null;

            return Parameter.Trim();
        }

        private List<int> ParseIntList(string Parameter)
        {
            List<int> list = new List<int>();

            if (string.IsNullOrWhiteSpace(Parameter))
                return list;

            var s = Parameter.Trim();

            var ary = s.Replace(" ", "").Split(',');
            foreach (var x in ary)
            {
                int value;
                if (int.TryParse(x, out value))
                {
                    list.Add(value);
                }
            }

            return list;
        }

    }
}
