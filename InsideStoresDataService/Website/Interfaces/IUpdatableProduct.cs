using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Website.Entities;

namespace Website
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Intended for classes like InsideFabricProduct
    /// </remarks>
    public interface IUpdatableProduct
    {
        void Initialize(IWebStore store, Product p, List<ProductVariant> variants, Manufacturer m, List<Category> categories, AspStoreDataContext dc);
    }
}