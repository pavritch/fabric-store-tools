using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Website
{
    /// <summary>
    /// Describes a category in SQL with associated keywords.
    /// </summary>
    public class CategoryInformation
    {
        public int CategoryID { get; set; }
        public string Name { get; set; }
        public string[] Keywords { get; set;}

        /// <summary>
        /// When true, do not do anything with this category.
        /// </summary>
        /// <remarks>
        /// It's likely some container category.
        /// </remarks>
        public bool Ignore { get; set; }

        public CategoryInformation()
        {

        }

        public CategoryInformation(int CategoryID, string Name, bool Ignore=false)
        {
            Keywords = new string[0];
            this.CategoryID = CategoryID;
            this.Name = Name;
            this.Ignore = Ignore;
        }


        /// <summary>
        /// This is the list of keywords - lower case. Name is not included. Put meaningful terms into keywords.
        /// </summary>
        public IEnumerable<string> DistinctKeywords
        {
            get
            {
                var set = new HashSet<string>();

                Action<string> add = (s) =>
                    {
                        var s2 = s.ToLower().Trim();
                        if (!string.IsNullOrWhiteSpace(s2))
                            set.Add(s2);
                    };

                foreach (var s in Keywords)
                    add(s);

                return set;
            }
        }

    }
}