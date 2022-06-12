using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace Website
{
    public class CollectionsAutoSuggestQuery
    {
        /// <summary>
        /// Which manufacturer?
        /// </summary>
        public int manufacturerID { get; set; }

        /// <summary>
        /// What kind of things for this manufacturer.
        /// </summary>
        /// <remarks>
        /// Books, Patterns or Collections.
        /// </remarks>
        public CollectionsKind Kind { get; set; }

        /// <summary>
        /// Filter phrase.
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// StartsWith, Contains.
        /// </summary>
        public AutoSuggestMode Mode { get; set; }

        /// <summary>
        /// How many results to return.
        /// </summary>
        public int Take { get; set; }


        public Action<CollectionsKind, int, string, List<string>> CompletedAction { get; set; }

        public CollectionsAutoSuggestQuery()
        {
            Take = 100;
            Mode = AutoSuggestMode.Contains;
        }
    }
}