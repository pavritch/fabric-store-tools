using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using InsideStores.Imaging;
using System.Threading;
using System.Threading.Tasks;
using ColorMine.ColorSpaces.Comparisons;
using System.Diagnostics;
using InsideStores.Imaging.Descriptors;
using System.Windows.Media.Imaging;
using System.Text;

namespace Website
{
    /// <summary>
    /// Manager class attached to IWebStore to provide low-level search features. Singleton.
    /// </summary>
    /// <remarks>
    /// Includes searching by CEDD (colors, textures, both), plus dominant colors.
    /// Do not cache results at this level. Caching can be handled by callers.
    /// </remarks>
    public class ImageSearch : IImageSearch
    {
        #region Locals

        private bool enableTestOutput = false;

        private string TestName;
        private CacheProduct TargetProduct;

        private IWebStore Store { get; set; }

	    #endregion

        #region Initialization

        public ImageSearch(IWebStore store)
        {
            this.Store = store;

#if false
            if (Directory.Exists(@"D:\InsideRugs-Dev\images\product\small") && Directory.Exists(@"c:\temp")
                && File.Exists(@"C:\Dev\InsideStores\Projects\InsideStoresDataService\Website\App_Data\Templates\ImageDescriptorTest.html"))
                enableTestOutput = true;
#endif

        }

        public bool IsSearchSupported
        {
            get
            {
                return Store.IsImageSearchEnabled;
            }
        } 
        #endregion

        #region Image Descriptors via reference ProductID

        /// <summary>
        ///  Returns an ordered set of products matched to the input product. 
        /// </summary>
        /// <param name="productID"></param>
        /// <param name="tolerance"></param>
        /// <param name="maxResults"></param>
        /// <returns></returns>
        public List<int> FindSimilarProducts(int productID, int tolerance = 50, int? maxResults = null)
        {
            var product = Store.ProductData.LookupProduct(productID);

            if (product == null || product.ImageDescriptor == null)
                return new List<int>();

            SetTest("FindSimilarProducts", productID);
            return FindSimilarProducts(productID, product.ImageDescriptor, tolerance, maxResults);
        }

        /// <summary>
        /// Returns an ordered set of products matched to the input product. 
        /// </summary>
        /// <param name="productID"></param>
        /// <param name="tolerance"></param>
        /// <param name="maxResults"></param>
        /// <returns></returns>
        public List<int> FindSimilarProductsByTexture(int productID, int tolerance = 50, int? maxResults = null)
        {
            var product = Store.ProductData.LookupProduct(productID);

            if (product == null || product.ImageDescriptor == null)
                return new List<int>();

            SetTest("FindSimilarProductsByTexture", productID);

            return FindSimilarProductsByTexture(productID, product.ImageDescriptor, tolerance, maxResults);
        }


        /// <summary>
        /// Returns an ordered set of products matched to the input product. 
        /// </summary>
        /// <param name="productID"></param>
        /// <param name="tolerance"></param>
        /// <param name="maxResults"></param>
        /// <returns></returns>
        public List<int> FindSimilarProductsByColor(int productID, int tolerance = 50, int? maxResults = null)
        {
            var product = Store.ProductData.LookupProduct(productID);

            if (product == null || product.ImageDescriptor == null)
                return new List<int>();

            SetTest("FindSimilarProductsByColor", productID);

            return FindSimilarProductsByColor(productID, product.ImageDescriptor, tolerance, maxResults);
        }

        /// <summary>
        /// Find products matching the manufactured CEDD based on the set of input products.
        /// </summary>
        /// <param name="products"></param>
        /// <param name="tolerance"></param>
        /// <param name="maxResults"></param>
        /// <returns></returns>
        public List<int> FindSimilarProducts(List<int> products, int tolerance = 50, int? maxResults = null)
        {
            Debug.Assert(products != null);

            var listCEDD = new List<byte[]>();

            foreach(var id in products)
            {
                var product = Store.ProductData.LookupProduct(id);
                if (product == null || product.ImageDescriptor == null)
                    continue;

                listCEDD.Add(product.ImageDescriptor);
            }

            return FindSimilarProducts(listCEDD, tolerance, maxResults);
        }

        #endregion

        #region Image Descriptors via CEDD

        /// <summary>
        /// Returns an ordered set of products matched to the input cedd descriptor. 
        /// </summary>
        /// <param name="excludedProductID">Don't include this product in the results.</param>
        /// <param name="cedd"></param>
        /// <param name="tolerance"></param>
        /// <param name="maxResults"></param>
        /// <returns></returns>
        public List<int> FindSimilarProducts(int? excludedProductID, byte[] cedd, int tolerance = 50, int? maxResults = null)
        {
            if (!IsSearchSupported)
                return new List<int>();

            Debug.Assert(cedd.Length == 144);

            var td = new TanimotoDistance(cedd);

            //return FindSimilar(excludedProductID, (product) =>
            //    {
            //        var dist = td.GetDistance(product.ImageDescriptor);

            //        if (dist > 1.0)
            //            Debug.WriteLine("distance > 1");

            //        return dist;
            //    }
            //    , tolerance, maxResults);

            return FindSimilar(excludedProductID, (product) => td.GetDistance(product.ImageDescriptor), tolerance, maxResults);
        }



        /// <summary>
        /// Returns an ordered set of products matched to the input cedd descriptor. 
        /// </summary>
        /// <param name="excludedProductID">Don't include this product in the results.</param>
        /// <param name="cedd"></param>
        /// <param name="tolerance"></param>
        /// <param name="maxResults"></param>
        /// <returns></returns>
        public List<int> FindSimilarProducts(int? excludedProductID, float[] cedd, int tolerance = 50, int? maxResults = null)
        {
            if (!IsSearchSupported)
                return new List<int>();

            Debug.Assert(cedd.Length == 144);

            var td = new TanimotoDistance(cedd);

            return FindSimilar(excludedProductID, (product) => td.GetDistance(product.ImageDescriptor), tolerance, maxResults);
        }


        /// <summary>
        /// Returns ordered set of similar products based on texture histograms from the cedd.
        /// </summary>
        /// <param name="excludedProductID">Don't include this product in the results.</param>
        /// <param name="cedd"></param>
        /// <param name="tolerance"></param>
        /// <param name="maxResults"></param>
        /// <returns></returns>
        public List<int> FindSimilarProductsByTexture(int? excludedProductID, byte[] cedd, int tolerance = 50, int? maxResults = null)
        {
            if (!IsSearchSupported)
                return new List<int>();

            Debug.Assert(cedd.Length == 144);

            var td = new TanimotoDistance(TextureHistogramTransform(cedd));

            return FindSimilar(excludedProductID, (product) => td.GetDistance(product.TextureHistogram ?? TextureHistogramTransform(product.ImageDescriptor)), tolerance, maxResults);
        }

        /// <summary>
        /// Returns ordered set of similar products based on color histograms from the cedd.
        /// </summary>
        /// <param name="excludedProductID">Don't include this product in the results.</param>
        /// <param name="cedd"></param>
        /// <param name="tolerance"></param>
        /// <param name="maxResults"></param>
        /// <returns></returns>
        public List<int> FindSimilarProductsByColor(int? excludedProductID, byte[] cedd, int tolerance = 50, int? maxResults = null)
        {
            if (!IsSearchSupported)
                return new List<int>();

            Debug.Assert(cedd.Length == 144);

            var td = new TanimotoDistance(ColorHistogramTransform(cedd));

            return FindSimilar(excludedProductID, (product) => td.GetDistance(product.ColorHistogram ?? ColorHistogramTransform(product.ImageDescriptor)), tolerance, maxResults);
        }

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
        public List<int> FindSimilarProducts(List<byte[]> descriptors, int tolerance = 50, int? maxResults = null)
        {
            Debug.Assert(descriptors != null);

            if (!IsSearchSupported || descriptors.Count() == 0)
                return new List<int>();

            AutomaticRelevanceFeedback arf = new AutomaticRelevanceFeedback(descriptors[0]);
            for (int i = 1; i < descriptors.Count; i++)
                arf.ApplyNewValues(descriptors[i]);

            var td = new TanimotoDistance(arf.GetNewDescriptor());

            return FindSimilar(null, (product) => td.GetDistance(product.ImageDescriptor), tolerance, maxResults);
        }


        /// <summary>
        /// Return ordered set of products using a descriptor manufactured on the fly based on the supplied textures and colors.
        /// </summary>
        /// <param name="theTextures"></param>
        /// <param name="theColors"></param>
        /// <param name="normalize"></param>
        /// <param name="tolerance"></param>
        /// <param name="maxResults"></param>
        /// <returns></returns>
        public List<int> FindSimilarProducts(List<CEDD.TextureTypes> theTextures, List<System.Windows.Media.Color> theColors, bool normalize = true, int tolerance = 50, int? maxResults = null)
        {
            if (!IsSearchSupported)
                return new List<int>();

            Debug.Assert(theTextures != null);
            Debug.Assert(theColors != null);

            var cedd = CEDD.CreateCustomDescriptor(theTextures, theColors, normalize);
            return FindSimilarProducts(null, cedd, tolerance, maxResults);
        }

        #endregion

        #region Dominant Colors
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
        public List<int> FindProductsHavingDominantColors(List<System.Windows.Media.Color> colors, int tolerance = 5, int? maxResults = null)
        {
            if (!IsSearchSupported)
                return new List<int>();

            var dtStart = DateTime.Now;

            // all math done in lab color mode for speed
            var targetLabColors = colors.ToLabColors();

            // dic[productID, distance]
            var dic = new ConcurrentDictionary<int, double>();

            // the max distance seems to be about 157, so we use this as the basis to compute distance bounds
            var labTolerance = tolerance * 1.57;

            Parallel.ForEach(Store.ProductData.Products.Values, (product) =>
            {
                if (product.IsDiscontinued || product.IsMissingImage || product.LabColors == null || product.LabColors.Count() == 0)
                    return;

                // product must match all input colors, distances are summed for comparison to other results

                double sumDistance = 0.0;

                foreach (var labColor in targetLabColors)
                {
                    bool found = false;
                    foreach (var productLabColor in product.LabColors)
                    {
                        var distance = labColor.Compare(productLabColor, new Cie1976Comparison());
                        if (distance <= labTolerance)
                        {
                            sumDistance += sumDistance;
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                        return; // a color was missing, skip this product
                }

                // at this point, the distance we keep is the sum of all distances for all colors,
                // which is simply used to help in sorting better results to the top

                dic[product.ProductID] = sumDistance;
            });


            var results = FinalizeResults(dic, maxResults);

            //Debug.WriteLine(string.Format("GetProductsHavingDominantColors: {0}", DateTime.Now - dtStart));

            return results;
        }

        /// <summary>
        /// Return an ordered list of up to max results have a top dominant color within the tolerance.
        /// </summary>
        /// <remarks>
        /// Excludes discontinued products.
        /// </remarks>
        /// <param name="color"></param>
        /// <param name="maxResults"></param>
        /// <param name="tolerance">A percentage, based on max distance is 157, so every percent is 1.57 for distance</param>
        /// <returns></returns>
        public List<int> FindProductsHavingTopDominantColor(System.Windows.Media.Color color, int tolerance = 5, int? maxResults = null)
        {
            if (!IsSearchSupported)
                return new List<int>();

            //Debug.WriteLine(string.Format("FindProductsHavingTopDominantColor({0}, {1}, {2})", color.ToRGBColorsString(), tolerance, maxResults.HasValue ? maxResults.ToString() : "null"));

            var dtStart = DateTime.Now;

            // all math done in lab color mode for speed
            var targetLabColor = color.ToLabColor();
            //Debug.WriteLine(string.Format("    LabColor = L:{0:N3}  A:{1:N3}  B:{2:N3}", targetLabColor.L, targetLabColor.A, targetLabColor.B));

            // dic[productID, distance]
            var dic = new ConcurrentDictionary<int, double>();

            // the max distance seems to be about 157, so we use this as the basis to compute distance bounds
            var labTolerance = tolerance * 1.57;

            //// distance as int, count
            //var histogram = new Dictionary<int, int>();
            //for (var i = 0; i < 200; i++)
            //    histogram[i] = 0;
            //Action<double> addHist = (d) =>
            //    {
            //        var k = (int)Math.Round(d);
            //        lock(histogram)
            //        {
            //            histogram[k] += 1;
            //        }
            //    };

            ProductGroup? group = Store.SupportedProductGroups.First(); 

            Parallel.ForEach(Store.ProductData.Products.Values, (product) =>
                {
                    // must be from the primary group for this store
                    if (product.ProductGroup != group)
                        return;

                    if (product.IsDiscontinued || product.IsMissingImage || product.LabColors == null || product.LabColors.Count() == 0)
                        return;

                    var distance = targetLabColor.Compare(product.LabColors.First(), new Cie1976Comparison());

                    //addHist(distance);

                    if (distance <= labTolerance)
                        dic[product.ProductID] = distance;
                });

            //Debug.WriteLine(string.Format("    Dic.Count() = {0:N0}", dic.Count()));

            var results = FinalizeResults(dic, maxResults);

            //Debug.WriteLine(string.Format("FindProductsHavingTopDominantColor: {0}", DateTime.Now - dtStart));

            // output histogram
            //for (int i = 0; i < 200; i++)
            //    Debug.WriteLine(string.Format("{0}, {1}", i, histogram[i]));
            return results;
        }

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
        public List<int> FindProductsHavingAnyDominantColor(System.Windows.Media.Color color, int tolerance = 5, int? maxResults = null)
        {
            if (!IsSearchSupported)
                return new List<int>();

            //Debug.WriteLine(string.Format("FindProductsHavingAnyDominantColor({0}, {1}, {2})", color.ToRGBColorsString(), tolerance, maxResults.HasValue ? maxResults.ToString() : "null"));

            var dtStart = DateTime.Now;

            // all math done in lab color mode for speed
            var targetLabColor = color.ToLabColor();
            //Debug.WriteLine(string.Format("    LabColor = L:{0:N3}  A:{1:N3}  B:{2:N3}", targetLabColor.L, targetLabColor.A, targetLabColor.B));

            // dic[productID, distance]
            var dic = new ConcurrentDictionary<int, double>();

            // the max distance seems to be about 157, so we use this as the basis to compute distance bounds
            var labTolerance = tolerance * 1.57;

            Parallel.ForEach(Store.ProductData.Products.Values, (product) =>
            {
                if (product.IsDiscontinued || product.IsMissingImage || product.LabColors == null || product.LabColors.Count() == 0)
                    return;

                foreach (var productLabColor in product.LabColors)
                {
                    var distance = targetLabColor.Compare(productLabColor, new Cie1976Comparison());
                    if (distance <= labTolerance)
                    {
                        // it's a keeper - save and continue
                        dic[product.ProductID] = distance;
                        return;
                    }
                }

                // if drops through to here, did not have a match, ignore
            });

            //Debug.WriteLine(string.Format("    Dic.Count() = {0:N0}", dic.Count()));

            var results = FinalizeResults(dic, maxResults);

            //Debug.WriteLine(string.Format("FindProductsHavingAnyDominantColor: {0}", DateTime.Now - dtStart));

            return results;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Download image from URL and extract descriptor. Null if error.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static byte[] GetDescriptorFromImageUrl(string url)
        {
            try
            {
                //var myImage = new BitmapImage(new Uri(url));
                //var cedd = myImage.CalculateDescriptor();

                var imageBytes = url.GetImageFromWeb();

                if (imageBytes == null || !imageBytes.HasImagePreamble())
                    return null;

                var bmsrc = imageBytes.FromImageByteArrayToBitmapSource();
                var cedd = bmsrc.CalculateDescriptor();

                Debug.Assert(cedd.Length == 144);
                Debug.Assert(cedd.Sum(e => e) != 0);

                return cedd;
            }
            catch(Exception Ex)
            {
                Debug.WriteLine(Ex.Message);

                return null;
            }
        }

        public static byte[] GetDescriptorFromImageFile(string filepath)
        {
            try
            {
                var imageBytes = filepath.ReadBinaryFile();

                if (imageBytes == null || !imageBytes.HasImagePreamble())
                    return null;

                var bmsrc = imageBytes.FromImageByteArrayToBitmapSource();
                var cedd = bmsrc.CalculateDescriptor();

                Debug.Assert(cedd.Length == 144);
                Debug.Assert(cedd.Sum(e => e) != 0);

                return cedd;
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);

                return null;
            }
        }


        /// <summary>
        /// Byte texture histogram compatible with Tanimoto formula. 
        /// </summary>
        /// <remarks>
        /// Also used by ProductDataCache to prepare cached histogram in CacheProduct.
        /// </remarks>
        /// <param name="cedd"></param>
        /// <returns></returns>
        public static byte[] TextureHistogramTransform(byte[] cedd)
        {

            byte[] histogram = new byte[6];
            for (int texture = 0; texture < 6; texture++)
                for (int color = 0; color < 24; color++)
                    histogram[texture] += cedd[texture * 24 + color];

            return histogram;
        }

        /// <summary>
        /// Byte color histogram compatible with Tanimoto formula. 
        /// </summary>
        /// <remarks>
        /// Also used by ProductDataCache to prepare cached histogram in CacheProduct.
        /// </remarks>
        /// <param name="cedd"></param>
        /// <returns></returns>
        public static byte[] ColorHistogramTransform(byte[] cedd)
        {
            byte[] histogram = new byte[24];
            for (int texture = 0; texture < 6; texture++)
                for (int color = 0; color < 24; color++)
                    histogram[color] += cedd[texture * 24 + color];

            return histogram;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Returns ordered list of matched products.
        /// </summary>
        /// <remarks>
        /// Exclude discontinued.
        /// </remarks>
        /// <param name="td">Precomputed Tanimoto from either byte[] or float[]</param>
        /// <param name="tolerance"></param>
        /// <param name="maxResults"></param>
        /// <returns></returns>
        private List<int> FindSimilar(int? ExcludedProductID, Func<CacheProduct, float> callback, int tolerance = 50, int? maxResults = null)
        {
            try
            {
                var dtStart = DateTime.Now;
                var toleranceDistance = tolerance / 100.0;

                // dic[productID, distance]
                var dic = new ConcurrentDictionary<int, float>();

                // if a product is provided, then results must be from that same product group

                ProductGroup? group = null;
                CacheProduct cacheProduct = null;
                if (ExcludedProductID.HasValue)
                {
                    if (Store.ProductData.Products.TryGetValue(ExcludedProductID.Value, out cacheProduct))
                        group = cacheProduct.ProductGroup;
                }
                else
                {
                    // otherwise, must be the primary (first) group associated with the store.

                    // note that the only tiny flaw here is that suggested/colors for Trim will show as fabric,
                    // but it othwerwise solves other issues

                    group = Store.SupportedProductGroups.First();
                }

                Parallel.ForEach(Store.ProductData.Products.Values, (product) =>
                {
                    if (product.IsDiscontinued || product.IsMissingImage || product.ImageDescriptor == null)
                        return;

                    if (group.HasValue)
                    {
                        // enforce any product group restriction
                        if (product.ProductGroup.HasValue && product.ProductGroup != group)
                            return;
                    }
                    else if (Store.StoreKey == StoreKeys.InsideFabric)
                    {
                        if (product.ProductGroup.HasValue && product.ProductGroup.Value != ProductGroup.Fabric)
                            return;
                    }

                    // only show products which are currently in stock
                    if (Store.HasAutomatedInventoryTracking && product.StockStatus == InventoryStatus.OutOfStock)
                        return;

                    // don't return self on searches to some similar product
                    if (ExcludedProductID.HasValue && product.ProductID == ExcludedProductID)
                        return;

                    var distance = callback(product);
                    //Debug.Assert(distance <= 1.0);

                    if (distance <= toleranceDistance)
                        dic[product.ProductID] = distance;
                });


                var results = FinalizeResults(dic, maxResults);

                //Debug.WriteLine(string.Format("FindSimilarProducts: {0}", DateTime.Now - dtStart));

                return results;
            }
            catch(Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
                return new List<int>();
            }
        }


        /// <summary>
        /// Prepare the final list of products based on passed in collection and maximum allowed results.
        /// </summary>
        /// <remarks>
        /// Can be used by any caller using a dictionary to track productID and distance.
        /// </remarks>
        /// <param name="dic"></param>
        /// <param name="maxResults"></param>
        /// <returns></returns>
        private List<int> FinalizeResults<T>(ConcurrentDictionary<int, T> dic, int? maxResults)
        {
            List<int> results;

            if (maxResults.HasValue)
                results = dic.AsParallel().OrderBy(e => e.Value).Select(e => e.Key).Take(maxResults.Value).ToList();
            else
                results = dic.AsParallel().OrderBy(e => e.Value).Select(e => e.Key).ToList();

            if (enableTestOutput && TargetProduct != null && !string.IsNullOrWhiteSpace(TestName))
                SaveResultsAsHtml(TestName, TargetProduct, dic, 1000);

            return results;
        }


        #endregion


        #region Testing

        //TestTransformBounds("ColorHistogramTransform", ColorHistogramTransform);
        //TestTransformBounds("TextureHistogramTransform", TextureHistogramTransform);

        public void TestTransformBounds(string name, Func<byte[], byte[]> transform)
        {
            Debug.WriteLine(string.Format("Begin test: {0}", name));

            var products = Store.ProductData.Products.Values.ToList();

            double minValue = double.MaxValue;
            double maxValue = double.MinValue;

            var rnd = new Random();
            for (int i = 0; i < 1000; i++)
            {
                var product = products[i];

                if (product.IsMissingImage)
                    continue;

                for(int j=0; j < 1000; j++)
                {
                    var index = rnd.Next(0, products.Count());
                    var product2 = products[index];

                    if (product2.IsMissingImage)
                        break;

                    var v1 = transform(product.ImageDescriptor);
                    var v2 = transform(product2.ImageDescriptor);

                    var dist = TanimotoDistance.GetDistance(v1, v2);

                    if (dist > maxValue)
                        maxValue = dist;

                    if (dist < minValue)
                        minValue = dist;
                }
            }

            Debug.WriteLine(string.Format("Test: {2} Min/Max {0} / {1}", minValue, maxValue, name));
        }        


#if false
        /// <summary>
        /// Compare our SQL descriptors to theirs - matched 100% with 0 distance.
        /// </summary>
        public void Test()
        {
            // to use this code, must add reference to CEDD.DLL and System.Xaml

            Func<string, string> makePath = (s) =>
            {
                return Path.Combine(@"D:\InsideRugs-Dev\images\product\large", s);
            };

            Func<BitmapSource, byte[]> zCalculateDescriptor = (myImage) =>
                {
                    CEDD_Descriptor.CEDD myCEDD = new CEDD_Descriptor.CEDD();
                    return myCEDD.getDescriptor(myImage);
                };

            Func<string, byte[]> zCalcDescriptorFromFilepath = (filename) =>
                {
                    var myImage = new BitmapImage(new Uri(makePath(filename)));
                    return zCalculateDescriptor(myImage);
                };

            var products = Store.ProductData.Products.Values.ToList();

            var rnd = new Random();
            for (int i = 0; i < 1000; i++)
            {
                var index = rnd.Next(0, products.Count());
                var product = products[index];

                if (product.IsMissingImage)
                    continue;

                var imageFilename = Path.GetFileName(product.ImageUrl);

                var cedd = zCalcDescriptorFromFilepath(imageFilename);

                var dist = TanimotoDistance.GetDistance(cedd, product.ImageDescriptor);

                Debug.WriteLine(string.Format("Distance for {0}: {1}", product.ProductID, dist));

            }
        }        
#endif

        private void SetTest(string name, int productID)
        {
            // note that this really isn't multi-threaded - test only

            if (enableTestOutput)
            {
                TestName = name;
                TargetProduct = Store.ProductData.LookupProduct(productID);
            }
        }

        private void SaveResultsAsHtml<T>(string testName, CacheProduct targetProduct, ConcurrentDictionary<int, T> dic, int maxResults)
        {
            Func<string, string> makePath = (s) =>
            {
                return Path.Combine(@"D:\InsideRugs-Dev\images\product\small", s);
            };

            Func<int, string> getImageUrl = (id) =>
            {
                var product = Store.ProductData.LookupProduct(id);
                var filepath = makePath(Path.GetFileName(product.ImageUrl));
                return filepath;
            };

            var results = dic.AsParallel().OrderBy(e => e.Value).Select(e => new
            {
                ProductID = e.Key,
                Distance = e.Value,
                Image = getImageUrl(e.Key)
            }).Take(maxResults);

            var template = File.ReadAllText(@"C:\Dev\InsideStores\Projects\InsideStoresDataService\Website\App_Data\Templates\ImageDescriptorTest.html");

            template = template.Replace("{product-id}", targetProduct.ProductID.ToString());
            template = template.Replace("{test-name}", testName);
            template = template.Replace("{min-distance}", dic.Values.Min().ToString());
            template = template.Replace("{max-distance}", dic.Values.Max().ToString());
            template = template.Replace("{url-target-image}", makePath(Path.GetFileName(targetProduct.ImageUrl)));

            // each image:
            // <div class="imageCell"><img src="{image-url}" /><div class="legend"><table><tr><td>{sku}</td><td align="right">{distance}</td></tr></table></div></div>

            var sbImages = new StringBuilder();

            int number = 1;
            foreach (var item in results)
            {
                sbImages.AppendFormat("<div class=\"imageCell\"><img src=\"{0}\" /><div class=\"legend\"><table><tr><td>{3}. {1}</td><td align=\"right\">{2}</td></tr></table></div></div>\n", item.Image, item.ProductID, item.Distance, number);
                number++;
            }

            template = template.Replace("{products}", sbImages.ToString());

            var filename = string.Format("{0}-{1}.html", targetProduct.ProductID, testName);
            File.WriteAllText(Path.Combine(@"c:\temp", filename), template);

        }

        #endregion

    }
}