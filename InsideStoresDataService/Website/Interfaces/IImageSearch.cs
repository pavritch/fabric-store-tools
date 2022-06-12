using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    public interface IImageSearch
    {
        bool IsSearchSupported { get;  }

        #region Descriptors using reference ProductID

        /// <summary>
        ///  Returns an ordered set of products matched to the input product. 
        /// </summary>
        /// <param name="productID"></param>
        /// <param name="tolerance"></param>
        /// <param name="maxResults"></param>
        /// <returns></returns>
        List<int> FindSimilarProducts(int productID, int tolerance = 50, int? maxResults = null);

        /// <summary>
        /// Returns an ordered set of products matched to the input product. 
        /// </summary>
        /// <param name="productID"></param>
        /// <param name="tolerance"></param>
        /// <param name="maxResults"></param>
        /// <returns></returns>
        List<int> FindSimilarProductsByTexture(int productID, int tolerance = 50, int? maxResults = null);


        /// <summary>
        /// Returns an ordered set of products matched to the input product. 
        /// </summary>
        /// <param name="productID"></param>
        /// <param name="tolerance"></param>
        /// <param name="maxResults"></param>
        /// <returns></returns>
        List<int> FindSimilarProductsByColor(int productID, int tolerance = 50, int? maxResults = null);

        /// <summary>
        /// Find products matching the manufactured CEDD based on the set of input products.
        /// </summary>
        /// <param name="products"></param>
        /// <param name="tolerance"></param>
        /// <param name="maxResults"></param>
        /// <returns></returns>
        List<int> FindSimilarProducts(List<int> products, int tolerance = 50, int? maxResults = null);

        #endregion

        #region Descriptors using CEDD

        /// <summary>
        /// Returns an ordered set of products matched to the input cedd (byte) descriptor. 
        /// </summary>
        /// <param name="excludedProductID">Don't include this product in the results.</param>
        /// <param name="cedd"></param>
        /// <param name="tolerance"></param>
        /// <param name="maxResults"></param>
        /// <returns></returns>
        List<int> FindSimilarProducts(int? excludedProductID, byte[] cedd, int tolerance = 50, int? maxResults = null);

        /// <summary>
        /// Returns an ordered set of products matched to the input cedd (float) descriptor. 
        /// </summary>
        /// <param name="excludedProductID">Don't include this product in the results.</param>
        /// <param name="cedd"></param>
        /// <param name="tolerance"></param>
        /// <param name="maxResults"></param>
        /// <returns></returns>
        List<int> FindSimilarProducts(int? excludedProductID, float[] cedd, int tolerance = 50, int? maxResults = null);

        /// <summary>
        /// Returns ordered set of similar products based on a ARF descriptor created from the input CEDD list.
        /// </summary>
        /// <remarks>
        /// Generally assumed that multiple descriptors are passed in. The feedback logic combines these 
        /// descriptors to manufacture a target custom descriptor to use for matching. If only one input
        /// descriptor is provided, then the results here would match the standard single descriptor method(s).
        /// </remarks>
        /// <param name="cedd"></param>
        /// <param name="tolerance"></param>
        /// <param name="maxResults"></param>
        /// <returns></returns>
        List<int> FindSimilarProducts(List<byte[]> descriptors, int tolerance = 50, int? maxResults = null);

        /// <summary>
        /// Returns ordered set of similar products based on texture histograms from the cedd.
        /// </summary>
        /// <param name="excludedProductID">Don't include this product in the results.</param>
        /// <param name="cedd"></param>
        /// <param name="tolerance"></param>
        /// <param name="maxResults"></param>
        /// <returns></returns>
        List<int> FindSimilarProductsByTexture(int? excludedProductID, byte[] cedd, int tolerance = 50, int? maxResults = null);

        /// <summary>
        /// Returns ordered set of similar products based on color histograms from the cedd.
        /// </summary>
        /// <param name="excludedProductID">Don't include this product in the results.</param>
        /// <param name="cedd"></param>
        /// <param name="tolerance"></param>
        /// <param name="maxResults"></param>
        /// <returns></returns>
        List<int> FindSimilarProductsByColor(int? excludedProductID, byte[] cedd, int tolerance = 50, int? maxResults = null);

        #endregion

        #region Find by Dominant Colors

        /// <summary>
        /// Returns an ordered list of products which have all specified colors within the tolerance.
        /// </summary>
        /// <remarks>
        /// Excludes products that are discontinued or don't have an image.
        /// </remarks>
        /// <param name="colors"></param>
        /// <param name="maxResults"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        List<int> FindProductsHavingDominantColors(List<System.Windows.Media.Color> colors, int tolerance = 5, int? maxResults = null);

        /// <summary>
        /// Return an ordered list of up to max results have a top dominant color within the tolerance.
        /// </summary>
        /// <remarks>
        /// Excludes products that are discontinued or don't have an image.
        /// </remarks>
        /// <param name="color"></param>
        /// <param name="maxResults"></param>
        /// <param name="tolerance">A percentage, based on max distance is 157, so every percent is 1.57 for distance</param>
        /// <returns></returns>
        List<int> FindProductsHavingTopDominantColor(System.Windows.Media.Color color, int tolerance = 5, int? maxResults = null);

        /// <summary>
        /// Return an ordered list of up to max results have a any dominant color within the tolerance.
        /// </summary>
        /// <remarks>
        /// Excludes discontinued products.
        /// </remarks>
        /// <param name="color"></param>
        /// <param name="maxResults"></param>
        /// <param name="tolerance">A percentage, based on max distance is 157, so every percent is 1.57 for distance</param>
        /// <returns></returns>
        List<int> FindProductsHavingAnyDominantColor(System.Windows.Media.Color color, int tolerance = 5, int? maxResults = null);
        
        #endregion
    }
}