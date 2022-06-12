using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace StoreCurator.Controllers
{
    public class RestApiController : ApiController
    {

        private IDatabase _database = null;

        private IDatabase DB
        {
            get
            {
                if (_database == null)
                    _database = new Database();

                return _database;
            }
        }

        [HttpGet]
        [Route("api/SetPretty/{productId:int}/{isPretty:bool}")]
        public bool SetPretty(int productID, bool isPretty)
        {
            try
            {
                Debug.WriteLine(string.Format("SetPretty {0}: {1}", productID, isPretty));
                return DB.SetPretty(productID, isPretty);
            }
            catch(Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
                return false;
            }
        }

        [HttpGet]
        [Route("api/SetPublished/{productId:int}/{isPublished:bool}")]
        public bool SetPublished(int productID, bool isPublished)
        {
            try
            {
                Debug.WriteLine(string.Format("SetPublished {0}: {1}", productID, isPublished));
                return DB.SetPublished(productID, isPublished);
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
                return false;
            }
        }

        [HttpGet]
        [Route("api/RemoveCategory/{productId:int}/{categoryID:int}")]
        public bool RemoveCategory(int productID, int categoryID)
        {
            try
            {
                Debug.WriteLine(string.Format("RemoveCategory {0}: {1}", productID, categoryID));
                return DB.RemoveCategory(productID, categoryID);
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
                return false;
            }
        }

        [HttpGet]
        [Route("api/AddCategory/{productId:int}/{categoryID:int}")]
        public bool AddCategory(int productID, int categoryID)
        {
            try
            {
                Debug.WriteLine(string.Format("AddCategory {0}: {1}", productID, categoryID));
                return DB.AddCategory(productID, categoryID);
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
                return false;
            }
        }

        private List<ProductSummary> GetTestProducts()
        {
            var domainName = ConfigurationManager.AppSettings["WebsiteDomainName"];

            using(var dc = new AspStoreDataContextReadOnly())
            {

                var list = (from p in dc.Products
                            select new ProductSummary()
                            {
                                ProductID = p.ProductID,
                                Name = p.Name,
                                IsPretty = true,

                                ProductUrl = string.Format("http://{2}/p-{0}-{1}.aspx", p.ProductID, p.SEName, domainName),

                                ImageIconUrl = string.Format("http://{1}/images/product/Icon/{0}", p.ImageFilenameOverride, domainName),
                                ImageSmallUrl = string.Format("http://{1}/images/product/Small/{0}", p.ImageFilenameOverride, domainName),
                                ImageMediumUrl = string.Format("http://{1}/images/product/Medium/{0}", p.ImageFilenameOverride, domainName),
                                ImageLargeUrl = string.Format("http://{1}/images/product/Large/{0}", p.ImageFilenameOverride, domainName)

                            }).Take(50).ToList();


                return list;

            }
        }
    }
}
