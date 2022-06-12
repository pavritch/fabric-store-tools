using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Windows.Media.Imaging;
using BitMiracle.LibJpeg;
using ExifLibrary;
using InsideFabric.Data;
using InsideStores.Imaging;
using InsideStores.Imaging.Descriptors;
using Website.Entities;
using AForge.Imaging.ColorReduction;

namespace Website
{
    /// <summary>
    /// Main class which knows how to process images from the image queue.
    /// </summary>
    /// <remarks>
    /// For when new or updated images are found by the product scanner. The scanner puts the productID
    /// in the SQL table queue.
    /// </remarks>
    public class InsideAvenueProductImageProcessor : ProductImageProcessor, IProductImageProcessor
    {
        public InsideAvenueProductImageProcessor(IWebStore store)
            : base(store)
        {
#if DEBUG
            //FakePopulateQueue(100);
#endif
        }

        #region ProcessSingleProduct
        /// <summary>
        /// Process the images for a single product. Will be called from the base class.
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="productInfo"></param>
        protected override void ProcessSingleProduct(AspStoreDataContext dc, ProductInfo productInfo)
        {
            Debug.WriteLine(string.Format("Processing {0}: {1}", productInfo.SKU, productInfo.Name));

            if (!productInfo.ExtData4.ContainsKey(ExtensionData4.ProductImages))
                return;

            if (!productInfo.ExtData4.ContainsKey(ExtensionData4.HomewareProductFeatures))
                return;

            var productImages = productInfo.ExtData4[ExtensionData4.ProductImages] as List<ProductImage>;

            // make sure we have some images
            // for homeware, our rule is we don't want to have products in SQL which don't have images

            if (productImages == null || productImages.Count() == 0)
                return;

            #region Actions

            Action saveExtData4 = () =>
            {
                var extData = new ExtensionData4();
                extData.Data = productInfo.ExtData4;
                var json = extData.Serialize();
                dc.Products.UpdateExtensionData4(productInfo.ProductID, json);
            };

            Action removeExistingDiskImages = () =>
            {
                try
                {
                    // remove anything from before which might be left over
                    if (productInfo.ExtData4.ContainsKey(ExtensionData4.AvailableImageFilenames))
                    {
                        var fileList = productInfo.ExtData4[ExtensionData4.AvailableImageFilenames] as List<string>;

                        if (fileList.Count() == 0)
                            return;

                        // remove files from disk
                        DeleteExistingImages(fileList);
                    }
                }
                catch { }
            };

            #endregion

            // grab all the available images at once since we need to evaluate them as a group
            // in order to detect duplicates and pick best ones for different uses

            // this is all downloaded images (including duplicates), but some are flagged as duplicates (not to go live)
            var downloadedImages = DownloadImages(productInfo.SKU, productImages);

            // make sure we have some keepers; in theory, the extra filter to remove duplicates from count should not be needed,
            // since to have a duplicates means there is a non-duplicate.

#if DEBUG
            if (downloadedImages.Count() == 0)
            {
                Debug.WriteLine(string.Format("**** Downloaded 0 of {0} images.", productImages.Count()));
                foreach(var url in productImages.Select(e => e.SourceUrl))
                    Debug.WriteLine(string.Format("                 url: {0}", url));
            }
#endif
            if (downloadedImages.Where(e => !e.IsDuplicate).Count() == 0)
                return;

            // at this point, we have some new image(s) to work with, so ensure we've got a clean starting point, beginning 
            // with deleting the older images. SQL will get cleaned up along the way.
            removeExistingDiskImages();

            var liveProductImages = new List<LiveProductImage>();

            // pick the image that will serve as the base for our listing default; duplicates excluded from consideration
            var pickedDefaultImage = PickDefaultImage(downloadedImages);

            // the default pick is used as the basis for generating an image for analysis
            var imageFeatures = MakeImageFeatures(pickedDefaultImage);

            #region Exif Data
            var exifProperties = new ProductExifProperties()
            {
                Artist = "Curated by Inside Stores, LLC",
                Description = productInfo.Name,
                Keywords = "homeware, online homeware store, insideavenue.com",
                Comment = string.Format("Our item {0}. Visit www.insideavenue.com to see full details for this product.", productInfo.SKU),
            };
            #endregion

            // save out images to folders

            // the call to SaveNonDefaultImages() has a misleading name since started with rugs. For InsideAvenue, one image
            // found by the scanner is always marked as Primary, any others are Scene. We treat them all the same, but the called
            // method will mark the Primary image as IsDefault in the LiveProductImage class. This also has the desired benefit
            // of keeping the scanner-generated image names for Primary, and this filename is used on manufacturer/category pages
            // as well as on detail pages.
            liveProductImages.AddRange(SaveNonDefaultImages(downloadedImages, exifProperties));

            // update SQL

            productInfo.ExtData4[ExtensionData4.LiveProductImages] = liveProductImages;
            // available names if more for legacy support until we obsolete across all stores
            productInfo.ExtData4[ExtensionData4.AvailableImageFilenames] = liveProductImages.Select(e => e.Filename).ToList();

            dc.Products.UpdateImageFilenameOverride(productInfo.ProductID, pickedDefaultImage.Filename);

            if (imageFeatures != null)
            {
                productInfo.ExtData4[ExtensionData4.ProductImageFeatures] = imageFeatures;
                // update ProductFeatures table
                SaveProductImageFeatures(dc, productInfo.ProductID, imageFeatures);
            }
            else if (productInfo.ExtData4.ContainsKey(ExtensionData4.ProductImageFeatures))
            {
                productInfo.ExtData4.Remove(ExtensionData4.ProductImageFeatures);
                dc.ProductFeatures.RemoveProductFeatures(productInfo.ProductID);
            }

            saveExtData4();

            return;
        }

        #endregion

        #region Private Methods


        /// <summary>
        /// Given a collection of scanned/detected images, physically download (using cache if available)
        /// and return a collection of slightly preprocessed images.
        /// </summary>
        /// <remarks>
        /// All in memory at this point. Nothing saved to disk. Includes duplicates, but they are flagged.
        /// </remarks>
        /// <param name="productImages"></param>
        /// <returns></returns>
        private List<DownloadedProductImage> DownloadImages(string SKU, List<ProductImage> productImages)
        {

            var downloadedImages = new List<DownloadedProductImage>();

            Func<string, string> normalizeSourceUrl = (u) =>
                {
                    if (SKU.StartsWith("LA-") && u.ContainsIgnoreCase(".JPG?"))
                    {
                        // in case looks like this, we need to trim
                        //http://cdn3.bigcommerce.com/s-acnlk/products/1686/images/1654/1834S__03620__07580.1444176597.600.600.JPG?c=2
                        var idx = u.IndexOf("?");
                        return u.Substring(0, idx);
                    }

                    if (SKU.StartsWith("MI") && u.ContainsIgnoreCase(".JPG?"))
                    {
                        // in case looks like this, we need to trim
                        // https://www.mirrorimagehome.com/spree/products/1232/original/30052-30052.jpg?1356121831
                        var idx = u.IndexOf("?");
                        return u.Substring(0, idx);
                    }
                    return u;
                };

#if true // parallel
            var lockObj = new object();
            Parallel.ForEach(productImages, (productImage) =>
            {
                var imageBytes = DownloadImage(normalizeSourceUrl(productImage.SourceUrl));
                // must have an image
                if (imageBytes != null)
                {
                    try
                    {
                        // will throw if not successfully preprocessed, and we'll then skip that image
                        var downloadedImage = new DownloadedProductImage(SKU, productImage, imageBytes);
                        lock (lockObj)
                        {
                            // add only if not already there by same name; really just a super safety measure
                            // since other code might get confused if duplicate filenames given that our rule
                            // is they must be unique.
                            if (downloadedImages.Where(e => e.Filename == downloadedImage.Filename).Count() == 0)
                                downloadedImages.Add(downloadedImage);
                        }
                    }
                    catch (Exception Ex)
                    {
                        Debug.WriteLine(Ex.ToString());
                    }
                }
            });
#else // not parallel
            foreach(var productImage in productImages)
            {
                var imageBytes = DownloadImage(normalizeSourceUrl(productImage.SourceUrl));
                // must have an image
                if (imageBytes != null)
                {
                    try
                    {
                        // will throw if not successfully preprocessed, and we'll then skip that image
                        var downloadedImage = new DownloadedProductImage(SKU, productImage, imageBytes);
                        // add only if not already there by same name; really just a super safety measure
                        // since other code might get confused if duplicate filenames given that our rule
                        // is they must be unique.
                        if (downloadedImages.Where(e => e.Filename == downloadedImage.Filename).Count() == 0)
                            downloadedImages.Add(downloadedImage);
                    }
                    catch(Exception Ex)
                    {
                        Debug.WriteLine(Ex.ToString());
                    }
                }
            }
#endif

            // weed out duplicates


            var finalImageList = new List<DownloadedProductImage>();

            Func<DownloadedProductImage, DownloadedProductImage, bool> hasGenerallySameAspectRatio = (first, second) =>
            {
                return Math.Abs(first.CroppedAspectRatio - second.CroppedAspectRatio) <= 0.02;
            };

            foreach (var imageVariant in downloadedImages.Select(e => e.ImageVariant).Distinct())
            {
                var imagesForVariant = downloadedImages.Where(e => e.ImageVariant == imageVariant).ToList();
                if (imagesForVariant.Count() == 1)
                {
                    finalImageList.AddRange(imagesForVariant);
                }
                else
                {
                    // there is more than one

                    // this does not necessarily mean they are duplicates of one another (yet)

                    var reviewedFilenames = new HashSet<string>();
                    foreach (var imageForVariant in imagesForVariant.ToList())
                    {
                        // if we've already taken this image into consideration, then don't 
                        // need to check again
                        if (reviewedFilenames.Contains(imageForVariant.Filename))
                            continue;

                        // this list will purposely include self to keep other processing simpler
                        var imagesWithSimilarAspectRatios = imagesForVariant.Where(e => hasGenerallySameAspectRatio(imageForVariant, e) && !reviewedFilenames.Contains(e.Filename)).ToList();

                        // regardless of anything else, any image we just found is now "reviewed" and not to be looked at again
                        imagesWithSimilarAspectRatios.ForEach(e => reviewedFilenames.Add(e.Filename));

                        if (imagesWithSimilarAspectRatios.Count() == 1)
                        {
                            finalImageList.AddRange(imagesWithSimilarAspectRatios);
                        }
                        else
                        {
                            // multiple with this aspect ratio, dig deeper, compare CEDD
                            // we know they are the same untrusted shape and have very similar aspect ratios

                            Func<DownloadedProductImage, DownloadedProductImage, bool> isExtremelySimilar = (first, second) =>
                            {
                                // it is observed that nearly all duplicates have a distance of 0.0, and the very few that are
                                // in the .007 range are in fact slightly different - maybe when a handmade pattern and you can 
                                // see that the threads are actually different, so it truly is a different image ever so slightly.

                                var distance = DistanceHelpers.CalcDistance(first.ComparisonDescriptor, second.ComparisonDescriptor);
#if false
                                    if (distance <= .01 && (first.Filename != second.Filename))
                                        Debug.WriteLine(string.Format("         Distance from {0} to {1} is {2}", first.Filename, second.Filename, distance));
#endif
                                return distance < .005;
                            };

                            Action<DownloadedProductImage> addToFinalList = (img) =>
                            {
                                // add only if not already there
                                if (!finalImageList.Any(e => e.Filename == img.Filename))
                                    finalImageList.Add(img);
                            };

                            foreach (var similarImage in imagesWithSimilarAspectRatios.ToList())
                            {
                                // will include self
                                var extremelySimilar = imagesWithSimilarAspectRatios.Where(e => isExtremelySimilar(similarImage, e)).ToList();
                                if (extremelySimilar.Count() == 1)
                                {
                                    addToFinalList(extremelySimilar.First());
                                }
                                else
                                {
                                    var biggerImage = extremelySimilar.OrderByDescending(e => e.PixelCount).First();
                                    addToFinalList(biggerImage);

                                    // any other image in this set would therefore be a duplicate of biggerImage

                                    foreach (var duplicateImage in extremelySimilar.Where(e => e.Filename != biggerImage.Filename))
                                        duplicateImage.DuplicateOf = biggerImage.Filename;
                                }
                            }
                        }

                    }
                }
            }

            // must be a list of distinct filenames
            Debug.Assert(finalImageList.Count() == finalImageList.Select(e => e.Filename).Distinct().Count());

            // the number of images we just flagged as duplicates should match delta between the two lists
            Debug.Assert((downloadedImages.Count() - finalImageList.Count()) == downloadedImages.Where(e => e.IsDuplicate).Count());

#if false
            if (finalImageList.Count() < downloadedImages.Count())
            {
                Debug.WriteLine(string.Format("       --- removed {0} duplicates", downloadedImages.Count() - finalImageList.Count()));
            }
#endif

            return downloadedImages;
        }



        /// <summary>
        /// Spin through the available images and pick the one for the basis of the default image.
        /// </summary>
        /// <remarks>
        /// The default image is not necessarily an unmodified version of this. Other logic determins what's
        /// needed - but here, we at least figure out which image will serve as the starting point.
        /// </remarks>
        /// <param name="downloadedImages"></param>
        /// <returns></returns>
        private DownloadedProductImage PickDefaultImage(List<DownloadedProductImage> downloadedImages)
        {
            return downloadedImages.Where(e => !e.IsDuplicate).OrderByDescending(e => e.ImageVariant == "Primary").ThenByDescending(e => e.PixelCount).First();
        }

        #endregion

    }



}
