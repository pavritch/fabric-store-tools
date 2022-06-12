using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    /// <summary>
    /// These are the kinds of queries for products which can be 
    /// submitted through the API to the web stores.
    /// </summary>
    public enum QueryRequestMethods
    {
        ListByCategory,
        ListByManufacturer,
        Search,
        AdvancedSearch,
        ListRelatedProducts,
        ProductSet,
        CrossMarketingProducts,
        ListByLabelValueWithinManufacturer,
        ListByPatternCorrelator,
        ListByProductCollection,
        ListBooksByManufacturer,
        ListPatternsByManufacturer,
        ListCollectionsByManufacturer,
        ListNewProductsByManufacturer,
        ListDiscontinuedProducts,
        ListProductsMissingImages,

        FindSimilarProducts,
        FindSimilarProductsByColor,
        FindSimilarProductsByTexture,

        FindByTopDominantColor,
        FindByAnyDominantColor,
        FacetSearch
    }
}