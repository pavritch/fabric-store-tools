using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StoreCurator
{
    public class CategoryMenuItem
    {
        public int CategoryID { get; set; }
        public int ParentCategoryID { get; set; }
        public string Name { get; set; }

        public List<CategoryMenuItem> Children { get; set; }

        public CategoryMenuItem()
        {
            Children = new List<CategoryMenuItem>(); 
        }
    }
}