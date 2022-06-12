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
    public class InsideRugsProductImageProcessor : ProductImageProcessor, IProductImageProcessor
    {
        public InsideRugsProductImageProcessor(IWebStore store)
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
            Debug.WriteLine(string.Format("Processing: {0}", productInfo.Name));

            if (!productInfo.ExtData4.ContainsKey(ExtensionData4.ProductImages))
                return;

            if (!productInfo.ExtData4.ContainsKey(ExtensionData4.RugProductFeatures))
                return;

            var productImages = productInfo.ExtData4[ExtensionData4.ProductImages] as List<ProductImage>;

            // make sure we have some images
            // for rugs, our rule is we don't want to have products in SQL which don't have images

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

            Action validateInput = () =>
            {
#if false
                var distinctCount = productImages.Select(e => e.SourceUrl).Distinct().Count();
                var totalCount = productImages.Count();
                if (distinctCount != totalCount)
                {
                    Debug.WriteLine(string.Format("     *** contains {0} duplicate URLs in ProductImages collection.", totalCount - distinctCount));
                }

                distinctCount = productImages.Select(e => e.Filename).Distinct().Count();
                totalCount = productImages.Count();
                if (distinctCount != totalCount)
                {
                    Debug.WriteLine(string.Format("     @@@ contains {0} duplicate filenames in ProductImages collection.", totalCount - distinctCount));
                }
#endif
            };

            #endregion

            validateInput(); // just outputs warnings to debug console.

            // grab all the available images at once since we need to evaluate them as a group
            // in order to detect duplicates and pick best ones for different uses

            // this is all downloaded images (including duplicates), but some are flagged as duplicates (not to go live)
            var downloadedImages = DownloadImages(productInfo.SKU, productImages);

            // make sure we have some keepers; in theory, the extra filter to remove duplicates from count should not be needed,
            // since to have a duplicates means there is a non-duplicate.

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
                Description = productInfo.Name.Replace(" by ", " Rug by "),
                Keywords = "rug, area rugs, discount rugs, online rug store, insiderugs.com",
                Comment = string.Format("Our item {0}. Visit www.insiderugs.com to see full details for this product.", productInfo.SKU),
            };
            #endregion

            // save out images to folders

            var liveDefaultImage = SaveDefaultImage(pickedDefaultImage, exifProperties);
            liveProductImages.Add(liveDefaultImage);

            liveProductImages.AddRange(SaveNonDefaultImages(downloadedImages.Cast<DownloadedProductImage>().ToList(), exifProperties));

            // update SQL

            productInfo.ExtData4[ExtensionData4.LiveProductImages] = liveProductImages;
            // available names if more for legacy support until we obsolete across all stores
            productInfo.ExtData4[ExtensionData4.AvailableImageFilenames] = liveProductImages.Select(e => e.Filename).ToList();

            dc.Products.UpdateImageFilenameOverride(productInfo.ProductID, liveDefaultImage.Filename);

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

            UpdateProductVariantImageAssociations(dc, productInfo, downloadedImages, pickedDefaultImage);

            return;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Make sure each variant is matched with the best available candidate. Update Ext4.
        /// </summary>
        /// <remarks>
        /// It's possible that some scanned ref'd images were removed as duplicates, so then use its keeper image.
        /// Stay within same shape, and make every attempt to defer to the original scanned reference image since
        /// it's possible that for Round (likely others too) the pattern on the rug actually changes based on the width.
        /// </remarks>
        /// <param name="dc"></param>
        /// <param name="productInfo"></param>
        /// <param name="downloadedImages"></param>
        private void UpdateProductVariantImageAssociations(AspStoreDataContext dc, ProductInfo productInfo, List<DownloadedRugProductImage> downloadedImages, DownloadedRugProductImage pickedDefaultImage)
        {
            var variants = dc.ProductVariants.Where(e => e.ProductID == productInfo.ProductID).Select(e => new {
                e.VariantID,
                e.ExtensionData4,
                e.Dimensions, // generally: Sample, Rectangular, Octagon, Oval, Round, Runner, Star, Square, Heart
            }).ToList();

            foreach(var variant in variants)
            {
                #region Actions
                Dictionary<string, object> _extensionData4 = null;
                Func<Dictionary<string, object>> ExtData4 = () =>
                {
                    if (_extensionData4 == null)
                    {
                        var extData = InsideFabric.Data.ExtensionData4.Deserialize(variant.ExtensionData4);

                        _extensionData4 = extData.Data;
                    }

                    return _extensionData4;
                };

                Action saveExtData4 = () =>
                {
                    var extData = new ExtensionData4();
                    extData.Data = ExtData4();
                    var json = extData.Serialize();
                    dc.ProductVariants.UpdateExtensionData4(variant.VariantID, json);
                }; 
                #endregion

                // if this hits, something is wrong since our rule is that this exists
                if (string.IsNullOrWhiteSpace(variant.ExtensionData4) || !ExtData4().ContainsKey(ExtensionData4.RugProductVariantFeatures))
                {
                    Debug.WriteLine(string.Format("**** VariantID {0} missing pv.ExtensionData4", variant.VariantID));
                    continue;
                }

                var variantFeatures = ExtData4()[ExtensionData4.RugProductVariantFeatures] as RugProductVariantFeatures;

                var originalFilename = variantFeatures.ImageFilename;

                if (variantFeatures.IsSample)
                {
                    variantFeatures.ImageFilename = pickedDefaultImage.Filename;
                }
                else
                {
                    // not sample, stay within shape

                    var imagesWithSameShape = downloadedImages.Where(e => !e.IsDuplicate && e.Shape == variantFeatures.Shape).ToList();
                    if (imagesWithSameShape.Count() == 0)
                    {
                        // nothing with same shape, so no choice but to go with default
                        variantFeatures.ImageFilename = pickedDefaultImage.Filename;
                    }
                    else if (imagesWithSameShape.Count() == 1)
                    {
                        // just one with same shape, so go with that
                        variantFeatures.ImageFilename = imagesWithSameShape.First().Filename;
                    }
                    else
                    {
                        // more than one with same shape, use scanned reference as our best hint if sensible

                        DownloadedRugProductImage hintImage = null;
                        if (string.IsNullOrWhiteSpace(variantFeatures.ImageFilename))
                            hintImage = downloadedImages.Where(e => e.Filename == variantFeatures.ImageFilename && e.Shape == variantFeatures.Shape).FirstOrDefault();

                        // if the reference we found is an image which was removed as a duplicate, get the reference to the replacement
                        if (hintImage != null && hintImage.IsDuplicate)
                            hintImage = downloadedImages.Where(e => e.Filename == hintImage.DuplicateOf && !e.IsDuplicate).FirstOrDefault();

                        if (hintImage != null)
                        {
                            // if we have a hint at this point, use it
                            variantFeatures.ImageFilename = hintImage.Filename;
                        }
                        else
                        {
                            // no good hint, take our best shot amongst those of the same shape

                            switch (variantFeatures.Shape)
                            {
                                case "Rectangular":
                                case "Runner":
                                case "Oval":

                                    // find closest based on aspect ratio (the smallest difference in aspect ratios)

                                    // normalize the aspect ratio so always vertical (just in case)
                                    var variantAspectRatio = Math.Min(variantFeatures.Width, variantFeatures.Length) / Math.Max(variantFeatures.Width, variantFeatures.Length);

                                    DownloadedRugProductImage best = null;
                                    foreach (var img in imagesWithSameShape)
                                    {
                                        if (best == null)
                                        {
                                            best = img;
                                            continue;
                                        }

                                        if (Math.Abs(img.CroppedAspectRatio - variantAspectRatio) < Math.Abs(best.CroppedAspectRatio - variantAspectRatio))
                                            best = img;
                                    }

                                    variantFeatures.ImageFilename = best.Filename;

                                    break;

                                default:
                                    variantFeatures.ImageFilename = imagesWithSameShape.First().Filename;
                                    break;
                            }

                        }
                    }
                }

                // save back if different

                if (variantFeatures.ImageFilename != originalFilename)
                    saveExtData4();
            }
        }

        /// <summary>
        /// Given a collection of scanned/detected images, physically download (using cache if available)
        /// and return a collection of slightly preprocessed images.
        /// </summary>
        /// <remarks>
        /// All in memory at this point. Nothing saved to disk. Includes duplicates, but they are flagged.
        /// </remarks>
        /// <param name="productImages"></param>
        /// <returns></returns>
        private List<DownloadedRugProductImage> DownloadImages(string SKU, List<ProductImage> productImages)
        {

            var downloadedImages = new List<DownloadedRugProductImage>();

#if true // parallel
            var lockObj = new object();
            Parallel.ForEach(productImages, (productImage) =>
            {
                var imageBytes = DownloadImage(productImage.SourceUrl);
                // must have an image
                if (imageBytes != null)
                {
                    try
                    {
                        // will throw if not successfully preprocessed, and we'll then skip that image
                        var downloadedImage = new DownloadedRugProductImage(SKU, productImage, imageBytes);
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
                var imageBytes = DownloadImage(productImage.SourceUrl);
                // must have an image
                if (imageBytes != null)
                {
                    try
                    {
                        // will throw if not successfully preprocessed, and we'll then skip that image
                        var downloadedImage = new DownloadedProductImage(productImage, imageBytes);
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

            // at the highest level, all review for duplicates is done at the shape level (cannot be duplicate if different shape),
            // then looks at aspect ratios, then finally (for when even aspect ratios are the same) CEDD distance

            var untrustedShapes = new List<string> { "Rectangular", "Square", "Oval", "Round", "Runner" };

            var finalImageList = new List<DownloadedRugProductImage>();

            Func<DownloadedRugProductImage, DownloadedRugProductImage, bool> hasGenerallySameAspectRatio = (first, second) =>
            {
                return Math.Abs(first.CroppedAspectRatio - second.CroppedAspectRatio) <= 0.02;
            };

            foreach(var shape in downloadedImages.Select(e => e.Shape).Distinct())
            {
                var imagesForShape = downloadedImages.Where(e => e.Shape == shape).ToList();
                if (!untrustedShapes.Contains(shape) || imagesForShape.Count() == 1)
                {
                    finalImageList.AddRange(imagesForShape);
                }
                else
                {
                    // it is an untrusted shape and there is more than one
                    
                    // this does not necessarily mean they are duplicates of one another (yet), because it's 
                    // frequent that designs change based on which exact rug size so they include an image of each.

                    var reviewedFilenames = new HashSet<string>();
                    foreach(var imageForShape in imagesForShape.ToList())
                    {
                        // if we've already taken this image into consideration, then don't 
                        // need to check again
                        if (reviewedFilenames.Contains(imageForShape.Filename))
                            continue;

                        // this list will purposely include self to keep other processing simpler
                        var imagesWithSimilarAspectRatios = imagesForShape.Where(e => hasGenerallySameAspectRatio(imageForShape, e) && !reviewedFilenames.Contains(e.Filename)).ToList();

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

                            Func<DownloadedRugProductImage, DownloadedRugProductImage, bool> isExtremelySimilar = (first, second) =>
                                {
                                    // it is observed that nearly all duplicates have a distance of 0.0, and the very few that are
                                    // in the .007 range are in fact slightly different - maybe when a handmade pattern and you can 
                                    // see that the threads are actually different, so it truly is a different image ever so slightly.

                                    var distance = DistanceHelpers.CalcDistance(first.ComparisonDescriptor, second.ComparisonDescriptor);
#if false
                                    if (distance <= .01 && (first.Filename != second.Filename))
                                        Debug.WriteLine(string.Format("         Distance from {0} to {1} is {2}", first.Filename, second.Filename, distance));
#endif
                                    return  distance < .005;
                                };

                            Action<DownloadedRugProductImage> addToFinalList = (img) =>
                                {
                                    // add only if not already there
                                    if (!finalImageList.Any(e => e.Filename == img.Filename))
                                        finalImageList.Add(img);
                                };

                            foreach(var similarImage in imagesWithSimilarAspectRatios.ToList())
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
        private DownloadedRugProductImage PickDefaultImage(List<DownloadedRugProductImage> downloadedImages)
        {
            // try for shapes in this order of priority
            var priorityShapes = new List<string>() { "Rectangular", "Oval", "Square", "Round", "Runner"};

            foreach (var shape in priorityShapes)
            {
                var candidates = downloadedImages.Where(e => e.Shape == shape && !e.IsDuplicate).ToList();
                if (candidates.Count() == 0)
                    continue;

                // we have one or more matches on this shape, take the one with the largest aspect ratio

                var pick = candidates.OrderByDescending(e => e.CroppedAspectRatio).ThenByDescending(e => e.PixelCount).First();
                return pick;
            }

            // not one of the pref shapes, start bottom feeding

            var secondRoundPick = downloadedImages.Where(e => e.Shape != "Scene" && e.Shape != "Sample" && !e.IsDuplicate).OrderByDescending(e => e.CroppedAspectRatio).ThenByDescending(e => e.PixelCount).FirstOrDefault();
            if (secondRoundPick != null)
                return secondRoundPick;

            // totally desparate, take anything
            return downloadedImages.Where(e => !e.IsDuplicate).OrderByDescending(e => e.CroppedAspectRatio).ThenByDescending(e => e.PixelCount).First();
        }


        #endregion 

    }



}
