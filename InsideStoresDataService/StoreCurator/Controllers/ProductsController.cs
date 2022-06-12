using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StoreCurator.Entities;

namespace StoreCurator.Controllers
{
    public class ProductsController : Controller
    {
        public ActionResult GetProductsByManufacturer(int manufacturerID, DisplayFormats format, int pageSize, int pageNumber)
        {
            try
            {
                var productList = GetTestProductList();
                return ShowView(format, productList);
            }
            catch(Exception Ex)
            {
                return View("Error", Ex);
            }
        }

        public ActionResult GetProductsByCategory(int categoryID, DisplayFormats format, int pageSize, int pageNumber)
        {
            try
            {
                var productList = GetTestProductList();
                return ShowView(format, productList);
            }
            catch (Exception Ex)
            {
                return View("Error", Ex);
            }
        }

        public ActionResult GetProductsByQuery(string query, DisplayFormats format, int pageSize, int pageNumber)
        {
            try
            {
                var productList = GetTestProductList();
                return ShowView(format, productList);
            }
            catch (Exception Ex)
            {
                return View("Error", Ex);
            }
        }

        private ActionResult ShowView(DisplayFormats format, ProductList productList)
        {
            var viewName = Enum.GetName(typeof(DisplayFormats), format);
            return View(viewName, productList);
        }

        private ProductList GetTestProductList()
        {
            using (var dc = new AspStoreDataContextReadOnly())
            {
                var domainName = ConfigurationManager.AppSettings["WebsiteDomainName"];

                var products = (from p in dc.Products
                                join pv in dc.ProductVariants on p.ProductID equals pv.ProductID where pv.IsDefault == 1
                                join pm in dc.ProductManufacturers on p.ProductID equals pm.ProductID

                            select new ProductSummary()
                            {
                                ProductID = p.ProductID,
                                Name = p.Name,
                                SKU = p.SKU, // when not variants
                                ManufacturerID = pm.ManufacturerID,
                                IsPretty = pm.DisplayOrder > 0,

                                ProductUrl = string.Format("http://{2}/p-{0}-{1}.aspx", p.ProductID, p.SEName, domainName),

                                ImageIconUrl = string.Format("http://{1}/images/product/Icon/{0}", p.ImageFilenameOverride, domainName),
                                ImageSmallUrl = string.Format("http://{1}/images/product/Small/{0}", p.ImageFilenameOverride, domainName),
                                ImageMediumUrl = string.Format("http://{1}/images/product/Medium/{0}", p.ImageFilenameOverride, domainName),
                                ImageLargeUrl = string.Format("http://{1}/images/product/Large/{0}", p.ImageFilenameOverride, domainName),

                                // for now, default variant only.

                                Cost = pv.Cost.GetValueOrDefault(),
                                OurPrice = pv.Price,
                                SalePrice = pv.SalePrice,
                                MSRP = pv.MSRP.GetValueOrDefault(),

                            }).Take(50).ToList();


                var pl = new ProductList()
                {
                    PageNumber = 1,
                    PageSize = 30,
                    TotalResults = 300,
                    Products = products,
                };

                return pl;
            }

        }
    }
}