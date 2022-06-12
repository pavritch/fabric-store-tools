using System;
using System.Collections.Generic;

namespace InsideFabric.Data
{
    public class HomewareCategoryNode
    {
        public int Id { get; set; }
        public string MenuName { get; set; }
        public int GoogleTaxonomyId { get; set; }
        public bool Included { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public string SeKeywords { get; set; }
        public string SeDescription { get; set; }
        public string SearchTerms { get; set; }

        public int ParentId { get; set; }
        public HomewareCategoryNode Parent { get; set; }
        public List<HomewareCategoryNode> Children { get; set; }
    }
}