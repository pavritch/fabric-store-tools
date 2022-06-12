using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StoreCurator.Entities;

namespace StoreCurator.Controllers
{
    public class HomeController : Controller
    {
        public class IndexPageViewData
        {
            public List<ManufacturerMenuItem> Manufacturers { get; set; }
            public List<CategoryMenuItem> Categories { get; set; }
        }

        private List<ManufacturerMenuItem> GetManufacturers()
        {
            using (var dc = new AspStoreDataContextReadOnly())
            {
                var list = (from m in dc.Manufacturers where m.Deleted == 0 && m.Published == 1
                            orderby m.Name
                            select new ManufacturerMenuItem()
                            {
                                ManufacturerID = m.ManufacturerID,
                                Name = m.Name,
                            }).ToList();

                return list;
            }
        }

        private List<CategoryMenuItem> GetCategories()
        {
            using (var dc = new AspStoreDataContextReadOnly())
            {
                var tree = new List<CategoryMenuItem>();

                var list = (from c in dc.Categories where c.Deleted == 0 && c.Published == 1
                           orderby c.CategoryID
                           select new CategoryMenuItem()
                           {
                               CategoryID = c.CategoryID,
                               ParentCategoryID = c.ParentCategoryID,
                               Name = c.Name,
                           }).ToList();

                // need to transform the list into a tree

                Func<CategoryMenuItem, List<CategoryMenuItem>> getChildren = null;

                getChildren = (categoryNode) =>
                    {
                        var branch = new List<CategoryMenuItem>();
                        foreach (var item in list.Where(e => e.ParentCategoryID == categoryNode.CategoryID).OrderBy(e => e.Name))
                        {
                            item.Children = getChildren(item);
                            branch.Add(item);
                        }
                        return branch;
                    };

                foreach(var item in list.Where(e => e.ParentCategoryID == 0).OrderBy(e => e.CategoryID))
                {
                    item.Children = getChildren(item);
                    tree.Add(item);
                }

                return tree;
            }
        }

        public ActionResult Index()
        {
            var viewData = new IndexPageViewData()
            {
                Manufacturers = GetManufacturers(),
                Categories = GetCategories(),
            };

            return View(viewData);
        }

    }
}