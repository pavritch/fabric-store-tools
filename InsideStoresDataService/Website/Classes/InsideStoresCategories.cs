using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Website.Entities;

namespace Website
{
    public class InsideStoresCategories
    {

        public Dictionary<int, CategoryInformation> Categories { get; private set; }

        /// <summary>
        /// Init and populate using actual data from SQL.
        /// </summary>
        /// <remarks>
        /// After any initial SQL population, this is the one to use so it can take advantage of
        /// the further manual tuning of the keywords.
        /// </remarks>
        /// <param name="listCategories"></param>
        public InsideStoresCategories(IEnumerable<Category> listCategories)
        {
            var list = new List<CategoryInformation>();

            foreach (var cat in listCategories)
            {
                var rec = new CategoryInformation(cat.CategoryID, cat.Name);
                if (!string.IsNullOrWhiteSpace(cat.ExtensionData))
                    rec.Keywords = cat.ExtensionData.ConvertToListOfLines().ToArray();

                if (rec.Keywords.Count() > 0)
                    list.Add(rec);
            }

            Categories = list.Where(e => !e.Ignore).ToDictionary(k => k.CategoryID, v => v);
        }

        public List<string> GetKeywordsForCategoryID(int CategoryID)
        {
            lock (this)
            {
                var list = new List<string>();
                CategoryInformation catInfo;
                if (Categories.TryGetValue(CategoryID, out catInfo) && !catInfo.Ignore)
                    list = catInfo.DistinctKeywords.ToList();

                return list;
            }
        }
    }
}

