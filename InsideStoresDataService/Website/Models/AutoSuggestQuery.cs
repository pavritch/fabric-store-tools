using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace Website
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AutoSuggestQuery
    {
        public string RequestUrl { get; set; }

        [JsonProperty]
        public string Query { get; set; }

        [JsonProperty]
        public AutoSuggestMode Mode { get; set; }

        [JsonProperty]
        public int ListID { get; set; }

        [JsonProperty]
        public int Take { get; set; }

        public Action<string, List<string>> CompletedAction { get; set; }

        public AutoSuggestQuery()
        {
            if (HttpContext.Current != null && HttpContext.Current.Request != null)
            {
                RequestUrl = HttpContext.Current.Request.Url.OriginalString;
            }

            Take = 100;
        }
    }
}