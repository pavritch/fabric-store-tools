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
    /// Main class which knows how to process images from the image queue. Generally to be a base class,
    /// although a bit fuzzy during transition to how we ingest images.
    /// </summary>
    /// <remarks>
    /// For when new or updated images are found by the product scanner. The scanner puts the productID
    /// in the SQL table queue.
    /// </remarks>
    public class ProductImageProcessor 
    {
        protected class ProductInfo
        {
            private Dictionary<string, object> _extensionData4;

            public int ProductID { get; set; }
            public string SKU { get; set; }
            public string Name { get; set; }
            public string ProductGroup { get; set; }
            public string ImageFilenameOverride { get; set; }
            public string ExtensionData4 { get; set; }

            public Dictionary<string, object> ExtData4
            {
                get
                {
                    if (_extensionData4 == null)
                    {
                        var extData = InsideFabric.Data.ExtensionData4.Deserialize(ExtensionData4);

                        _extensionData4 = extData.Data;
                    }

                    return _extensionData4;
                }
            }
        }

        #region ImageSizingInfo Class & Data
        protected class ImageSizingInfo
        {
            public string FolderName { get; set; }
            public int? SquareDimension { get; set; }
            public int? MaxBytes { get; set; }
            public Func<byte[], byte[]> Resize { get; set; }
            public bool IncludeExifProperties { get; set; }

            public ImageSizingInfo(string folderName, int? squareDimension, int? maxBytes, Func<byte[], byte[]> resizerMethod, bool includeExifProperties)
            {
                this.FolderName = folderName;
                this.SquareDimension = squareDimension;
                this.MaxBytes = maxBytes;
                this.Resize = resizerMethod;
                this.IncludeExifProperties = includeExifProperties;
            }
        }

        // note that these sizes presently ignore system/store level configuration settings.
        // for now, assumes one-size fits all.
        protected static ImageSizingInfo[] SizingInformation = new ImageSizingInfo[]
        {
            new ImageSizingInfo("Micro", 50, 3 * 1024, (e) => e.MakeMicroImage50(), false), // square, ~1.5 to 2KB
            new ImageSizingInfo("Mini", 100, 15 * 1024, (e) => e.MakeMiniImage100(), false), // square, mostly 5 to 6KB, some 7KB, some as little as 2KB
            new ImageSizingInfo("Icon", 150, 30 * 1024, (e) => e.MakeIconImage150(), false), // square, mostly in 8 to 12KB; 4 across
            new ImageSizingInfo("Small", 225, 40 * 1024, (e) => e.MakeSmallImage225(), false), // square, mostly 14KB to 24KB; 3 across (really need to keep at least to 14KB, smaller looks a bit fuzzy)
            new ImageSizingInfo("Medium", 350, 60 * 1024, (e) => e.ResizeImage(350, 85), true), // mostly 30K to 100K, but maybe about 10% are up to 200KB
            new ImageSizingInfo("Large", 800, null, (e) => e.ResizeImage(800, 80), true), // bulk of it in the 200K to 400KB range
        };
        
        #endregion

        protected IWebStore Store;

        protected bool fakeOutQueue = false; 

        public ProductImageProcessor(IWebStore store)
        {
            this.Store = store;
        }

        /// <summary>
        /// Used to put a bunch of productIDs into the queue.
        /// </summary>
        /// <remarks>
        /// Pretty much none will have a populated SourceUrl - so need to fake out that too in the caller below.
        /// </remarks>
        /// <param name="countToPopulate"></param>
        protected void FakePopulateQueue(int countToPopulate)
        {
            using (var dc = new AspStoreDataContext(Store.ConnectionString)) 
            {
                dc.ImageProcessingQueues.TruncateTable();
                //var products = dc.Products.Where(e => e.ImageFilenameOverride != null && e.ShowBuyButton == 1 && e.SKU.StartsWith("SY-")).Select(e => e.ProductID).Take(countToPopulate).ToList();
                //var products = dc.Products.Where(e => e.ImageFilenameOverride != null && e.ShowBuyButton == 1 && e.SKU.StartsWith("SV-")).Select(e => e.ProductID).ToList();
                var products = dc.Products.Where(e => e.ImageFilenameOverride == null && e.SKU.StartsWith("KR-")).Select(e => e.ProductID).ToList();

                foreach(var productID in products)
                {
                    var item = new ImageProcessingQueue()
                    {
                        ProductID = productID,
                        CreatedOn = DateTime.Now,
                    };

                    dc.ImageProcessingQueues.InsertOnSubmit(item);
                    dc.SubmitChanges();
                }
            }
        }

        #region Main Loop

        public void ProcessQueue(CancellationToken cancelToken, IProgress<int> progressCallback, Action<string> reportStatusCallback = null)
        {
            if (fakeOutQueue)
                FakePopulateQueue(5);

            using (var dc = new AspStoreDataContext(Store.ConnectionString))
            {
                // keep picking up new batches of products until empty

                while (true)
                {
                    int lastReportedProgress = 0;
                    int countCompleted = 0;
                    var productList = dc.ImageProcessingQueues.Select(e => e.ProductID).ToList();

#if DEBUG
                    //productList.Shuffle();
                    //productList.Shuffle();
                    //productList.Shuffle();
#endif

                    int countTotal = productList.Count();

                    // make sure we have work to do
                    if (countTotal == 0)
                        break;

                    #region Actions (reporting)

                    Action reportPercentComplete = () =>
                    {
                        if (progressCallback == null)
                            return;

                        var pct = countTotal == 0 ? 0 : (countCompleted * 100) / countTotal;

                        if (lastReportedProgress != pct)
                        {
                            lastReportedProgress = pct;
                            progressCallback.Report(pct);
                            System.Threading.Thread.Sleep(20);
                        }
                    };

                    Action<string> reportStatus = (msg) =>
                    {
                        if (reportStatusCallback != null)
                        {
                            reportStatusCallback(msg);
                            System.Threading.Thread.Sleep(2);
                        }
                    };

                    #endregion

                    reportStatus(string.Format("Processing {0:N0} products...", productList.Count()));
                    System.Threading.Thread.Sleep(1000);

#if false // PETER 7/16/18 because seem to have issues with BF
                    var options = new ParallelOptions()
                    {
                        CancellationToken = CancellationToken.None,
                        MaxDegreeOfParallelism = 20,
                        TaskScheduler = TaskScheduler.Default,
                    };

                    Parallel.ForEach(productList, options, (int productID, ParallelLoopState loopState) =>
                    {

                        if (cancelToken.IsCancellationRequested)
                            loopState.Stop();

                        using (var dc2 = new AspStoreDataContext(Store.ConnectionString))
                        {
                            try
                            {
                                // make sure is a real product (still)


                                var productInfo = dc2.Products.Where(e => e.ProductID == productID && e.Deleted == 0)
                                        .Select(e => new ProductInfo()
                                        {
                                            ProductID = e.ProductID,
                                            SKU = e.SKU,
                                            Name = e.Name,
                                            ProductGroup = e.ProductGroup,
                                            ExtensionData4 = e.ExtensionData4,
                                            ImageFilenameOverride = e.ImageFilenameOverride
                                        }).FirstOrDefault();


                                // allow for individual stores to customize this processing flow
                                if (productInfo != null)
                                    ProcessSingleProduct(dc2, productInfo);

                            }
                            catch (Exception Ex)
                            {
                                Debug.WriteLine("Exception: " + Ex.Message);
                            }
                            finally
                            {
                                // always need to delete from queue, else infinite loop
                                dc2.DeleteImageProcessingQueueRecord(productID);
                            }
                        }


                        countCompleted++;
                        reportPercentComplete();
                        reportStatus(string.Format("{0:N0} products remaining...", productList.Count() - countCompleted));

                        if (cancelToken.IsCancellationRequested)
                            loopState.Stop();

                    });
                
#else

                    foreach (var productID in productList)
                    {
                        if (cancelToken.IsCancellationRequested)
                            break;

                        try
                        {
                            // make sure is a real product (still)

                            var productInfo = dc.Products.Where(e => e.ProductID == productID && e.Deleted == 0)
                                    .Select(e => new ProductInfo() { ProductID = e.ProductID, SKU = e.SKU, Name = e.Name, ProductGroup = e.ProductGroup, 
                                        ExtensionData4 = e.ExtensionData4, ImageFilenameOverride = e.ImageFilenameOverride }).FirstOrDefault();

                            // allow for individual stores to customize this processing flow
                            if (productInfo != null)
                                ProcessSingleProduct(dc, productInfo);

                        }
                        catch(Exception Ex)
                        {
                            Debug.WriteLine("Exception: " + Ex.Message);
                        }
                        finally
                        {
                            // always need to delete from queue, else infinite loop
                            dc.DeleteImageProcessingQueueRecord(productID);
                        }

                        countCompleted++;
                        reportPercentComplete();
                        reportStatus(string.Format("{0:N0} products remaining...", productList.Count() - countCompleted));
                    }

                    if (cancelToken.IsCancellationRequested)
                        break;
#endif
                }

            }
        }

        /// <summary>
        /// Process the images for a single product.
        /// </summary>
        /// <remarks>
        /// So far, this logic remains identical to legacy IF processing - to avoid risk. Rugs/InsideAve has an override.
        /// </remarks>
        /// <param name="dc"></param>
        /// <param name="productInfo"></param>
        protected virtual void ProcessSingleProduct(AspStoreDataContext dc, ProductInfo productInfo)
        {
            bool _isExtensionData4Dirty = false;

            #region Actions for ExtensionData
            Dictionary<string, object> _extensionData4 = null;

            Action markExtData4Dirty = () =>
            {
                _isExtensionData4Dirty = true;
            };

            Func<int, Dictionary<string, object>> getExtData4 = (id) =>
            {
                if (_extensionData4 == null)
                {
                    var extDataText = productInfo.ExtensionData4;
                    var extData = ExtensionData4.Deserialize(extDataText);
                    _extensionData4 = extData.Data;
                }

                return _extensionData4;
            };

            Action<int> saveExtData4 = (id) =>
            {
                if (!_isExtensionData4Dirty)
                    return;

                var extData = new ExtensionData4();
                extData.Data = getExtData4(id);

                var json = extData.Serialize();

                dc.Products.UpdateExtensionData4(id, json);
                _isExtensionData4Dirty = false;
            };
            #endregion

            #region bigTryBlock
            try
            {
                _extensionData4 = null;
                _isExtensionData4Dirty = false;
                List<ProductImage> productImages = new List<ProductImage>();

                try
                {
                    var ext = getExtData4(productInfo.ProductID);
                    object obj;
                    if (!ext.TryGetValue(ExtensionData4.ProductImages, out obj))
                        return;

                    productImages = obj as List<ProductImage>;

                    foreach (var productImage in productImages.OrderByDescending(e => e.ImageVariant == "Primary").ThenBy(e => e.DisplayOrder).ToList())
                    {
                        if (string.IsNullOrWhiteSpace(productImage.SourceUrl))
                            continue;

                        var filename = productImage.Filename;
                        var srcUrl = productImage.SourceUrl.Replace(" ", "%20"); // clean up for a common mistake observed in some scanner output

                        #region Duplicates
                        // see if same name exists anywhere in SQL for another product - primary
                        bool isDuplicate = dc.Products.Where(e => e.ProductID != productInfo.ProductID && e.ImageFilenameOverride == productImage.Filename).Count() > 0;

                        if (isDuplicate || !IsSafeFilename(filename))
                        {
                            // use productID for forming valid image if possible

                            if (productImage.IsDefault)
                                filename = string.Format("{0}.jpg", productInfo.ProductID);
                            else
                                filename = string.Format("{0}.jpg", Guid.NewGuid().ToString().Replace("-", ""));

                            productImage.Filename = filename;
                            markExtData4Dirty();
                        }
                        #endregion

                        #region Exif Logic
                        string exifKeywords = null;
                        string byReplacement = null;
                        switch (productInfo.ProductGroup)
                        {
                            case "Fabric":
                                exifKeywords = "fabric, upholstery fabric, drapery fabric, discount fabric, online fabric store, insidefabric.com";
                                byReplacement = " Fabric by ";
                                break;

                            case "Wallpaper":
                                exifKeywords = "wallpaper, wallcovering, discount wallpaper, online wallpaper store, insidewallpaper.com";
                                byReplacement = " Wallpaper by ";
                                break;

                            case "Rug":
                                exifKeywords = "rug, area rugs, discount rugs, online rug store, insiderugs.com";
                                byReplacement = " Rug by ";
                                break;

                            default:
                                exifKeywords = null;
                                byReplacement = " by "; // original value
                                break;
                        }

                        var ExifProperties = new ProductExifProperties()
                        {
                            Artist = "Curated by Inside Stores, LLC",
                            Description = productInfo.Name.Replace(" by ", byReplacement),
                            Keywords = exifKeywords,
                            Comment = string.Format("Our item {0}. Visit www.{1} to see full details for this product.", productInfo.SKU, Store.Domain),
                        };

                        #endregion

                        if (ProcessSingleImage(productInfo.ProductID, productInfo.SKU, ExifProperties, filename, srcUrl))
                        {
                            // for the default image, poke the name into the product row, compute image features (CEDD, etc.)

                            if (productImage.IsDefault)
                            {
                                var existingFilename = dc.Products.Where(e => e.ProductID == productInfo.ProductID).Select(e => e.ImageFilenameOverride).FirstOrDefault();

                                // if changing over from one filename to another, then delete files for the old name - already just wrote out new images under the new name
                                if (!string.IsNullOrWhiteSpace(existingFilename) && !string.Equals(existingFilename, filename, StringComparison.OrdinalIgnoreCase))
                                    DeleteImageFilename(existingFilename);

                                if (!string.Equals(existingFilename, filename, StringComparison.OrdinalIgnoreCase))
                                    dc.Products.UpdateImageFilenameOverride(productInfo.ProductID, filename);

                                // no need to calculate and persist for InsideAvenue which does not rely on CEDD image features

                                if (Store.IsImageSearchEnabled)
                                {
                                    var features = MakeImageFeatures(filename);
                                    if (features != null)
                                    {
                                        ext[ExtensionData4.ProductImageFeatures] = features;
                                        markExtData4Dirty();
                                        // update ProductFeatures table
                                        SaveProductImageFeatures(dc, productInfo.ProductID, features);
                                    }
                                    else if (ext.ContainsKey(ExtensionData4.ProductImageFeatures))
                                    {
                                        ext.Remove(ExtensionData4.ProductImageFeatures);
                                        markExtData4Dirty();
                                        dc.ProductFeatures.RemoveProductFeatures(productInfo.ProductID);
                                    }
                                }
                            }
                        }
                        else
                        {
                            // something went wrong

                            productImage.SourceUrl = null;
                            markExtData4Dirty();

                            // if was given a different filename, but we didn't get the web image, then
                            // we need to behave as a rename - to make the ImageFilenameOverride and existing physical image
                            // match what was given here since we need to assume this is the desired name

                            if (productImage.IsDefault)
                            {
                                var existingFilename = dc.Products.Where(e => e.ProductID == productInfo.ProductID).Select(e => e.ImageFilenameOverride).FirstOrDefault();

                                try
                                {
                                    if (!string.IsNullOrWhiteSpace(existingFilename) && !string.Equals(existingFilename, filename, StringComparison.OrdinalIgnoreCase) && ImageFileNameExists(existingFilename))
                                    {
                                        // make sure the rename won't collide with something that's already there
                                        DeleteImageFilename(filename);

                                        RenameImageFile(existingFilename, filename);
                                        dc.Products.UpdateImageFilenameOverride(productInfo.ProductID, filename);

                                        // if doing a rename, then also should rename the file reference within the features data
                                        // so won't be out of sync. The other features would not change since we simply renamed an existing
                                        // already-analyzed file.

                                        if (ext.ContainsKey(ExtensionData4.ProductImageFeatures))
                                        {
                                            var features = ext[ExtensionData4.ProductImageFeatures] as ImageFeatures;
                                            features.Filename = filename;
                                        }
                                    }
                                }
                                catch
                                { }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception: " + ex.Message);
                }
                finally
                {
                    // figure out exactly what we do have at this point and keep a list of those filenames which truly physically exist

                    var possibleFiles = productImages.Where(e => !string.IsNullOrWhiteSpace(e.Filename)).Select(e => e.Filename).ToList();
                    var foundFiles = FindExistingImageFiles(possibleFiles);

                    var ext = getExtData4(productInfo.ProductID);

                    ext[ExtensionData4.AvailableImageFilenames] = foundFiles;
                    ext[ExtensionData4.ProductImages] = productImages;
                    markExtData4Dirty();

                    // finalize and move on to next product
                    saveExtData4(productInfo.ProductID);
                }
            }
            catch (Exception Ex)
            {
                // must have thrown in finally above - can never let it kill the loop
                Debug.WriteLine(Ex.Message);
            }
            #endregion

            return;
        }
        #endregion

        #region Public Methods



        /// <summary>
        /// Given list of prospect names, find out which ones exist and return that list
        /// </summary>
        /// <param name="filenames"></param>
        /// <returns></returns>
        public List<string> FindExistingImageFiles(List<string> filenames)
        {
            var foundFiles = new List<string>();

            foreach (var filename in filenames)
            {
                if (ImageFileNameExists(filename))
                    foundFiles.Add(filename);
            }

            return foundFiles;
        }


        #endregion

        #region Process Single File

        /// <summary>
        /// Processes a single file without regard for duplicates, etc. Url must be HTTP or FTP.
        /// </summary>
        /// <remarks>
        /// Remains as same general logic as legacy IF and early IR - to avoid risk. See override in IR subclass.
        /// Fetches original image from url, saves in Original folder. Then resizes for Small/Medium/Large.
        /// Overwrites anything it finds.
        /// </remarks>
        /// <param name="filename">Name to be saved as in the set of folders.</param>
        /// <param name="imageUrl">Source image url.</param>
        /// <returns>True if successful.</returns>
        protected virtual bool ProcessSingleImage(int productID, string SKU, ProductExifProperties exifProperties,  string filename, string imageUrl)
        {
            try
            {
                if (!IsSafeFilename(filename))
                    throw new Exception(string.Format("Unsafe filename for {0}: {1}", filename, imageUrl));

                if (!imageUrl.IsValidAbsoluteUrl())
                    throw new Exception(string.Format("Invalid image url for {0}: {1}", filename, imageUrl));

                byte[] image = null;
                int? Width = null;
                int? Height = null;

                #region Acquire Image from Cache or Web

                bool haveImageFile = false;
                bool saveToCache = false;
                string cacheFilepath = null;

                if (Store.UseImageDownloadCache)
                {
                    // the cache file is in the original format jpg|png|tif from vendor, using 
                    // a GUID-like name, but with the correct extension. No processing at all is done
                    // other than the rename to avoid conflicts.

                    cacheFilepath = MakeCacheFilepath(imageUrl);
                    if (File.Exists(cacheFilepath))
                    {
                        image = cacheFilepath.ReadBinaryFile();
                        haveImageFile = true;
                    }
                    else
                    {
                        saveToCache = true;
                    }
                }

                if (!haveImageFile)
                {
                    // testing
                    //imageUrl = "ftp://onlinevendors:soho712@ftp.safavieh.com/DROP%20SHIP%20VENDORS/RUG%20IMAGES%20-%20COMPLETE%20COLLECTION/PROMOTIONAL%20RUG%20images%20-%20NO%20MAP/Adirondack/ADR101B-6R.jpg";
                    //imageUrl = "http://www.insidefabric.com/images/product/original/175891-painted-turtles-shell-by-fschumacher.jpg";

                    // BF with top and bottom borders
                    // imageUrl = "ftp://insideave:inside00@file.kravet.com/BF/HIRES/JAG-50048_1550.JPG";

                    try
                    {
                        // tiny retry loop
                        for (int i = 1; i < 3; i++)
                        {
                            image = imageUrl.GetImageFromWeb();
                            if (image != null)
                                break;
                            Thread.Sleep(i * 10 * 1000);
                        }
                    }
                    catch
                    {
                        // presently, called web access does not throw - so this will not be called
                        throw new Exception(string.Format("No image for {0} at url {1}", filename, imageUrl));
                    }
                }

                if (image == null)
                    throw new Exception(string.Format("No image for {0} at url {1}", filename, imageUrl));

                if (Store.UseImageDownloadCache && saveToCache && cacheFilepath != null)
                {
                    if (File.Exists(cacheFilepath))
                        File.Delete(cacheFilepath);

                    image.WriteBinaryFile(cacheFilepath);
                }

	            #endregion


                // note that image[] could be jpg|png|tif -- do not assume original image is a jpg, even
                // though we'll subsequently make everything into a jpg file.

                if (!image.HasJpgImagePreamble())
                {
                    // is not a jpg, but we pretty much need jpg due to how legacy code works,
                    // so change here as needed

                    image = image.ToJpeg(95); // convert any image format to a jpg with this quality
                }

                // sanity check on image, bail if not looking right...will throw if not image, that's okay too

                GetImageDimensions(image, out Width, out Height);

                if (Width.GetValueOrDefault() < 5 || Height.GetValueOrDefault() < 5)
                    throw new Exception(string.Format("Image size less than 5x5 pixels for {0} at url {1}", filename, imageUrl));

#if false
                // reduce size of input if totally big.
                if (image.Length > 1 * 1000000 || Width.Value > 2500)
                {
                    double factor = Math.Min(1.0, 1000.0 / Width.Value);

                    using (var bmp = image.FromImageByteArrayToBitmap())
                    {
                        if (bmp == null)
                            throw new Exception(string.Format("Unable to reduce size for {0} at url {1}", filename, imageUrl));

                        using (var resized = new Bitmap(bmp, new Size((int)(bmp.Width * factor), (int)(bmp.Height * factor))))
                        {
                            if (resized == null)
                                throw new Exception(string.Format("Unable to reduce size for {0} at url {1}", filename, imageUrl));

                            image = resized.ToJpeg(85);
                            if (image.Length > 500000)
                                image = resized.ToJpeg(80);
                            if (image.Length > 500000)
                                image = resized.ToJpeg(70);
                        }
                    }

                    GetImageDimensions(image, out Width, out Height);
                }
#endif
                // TODO: move processing 
                // The save of original must be truly the original, with the only thing changed possibly is being converted to JPG.
                // The cropping/processing stuff should be moved after that original save - so that it applies only to our resized
                // images. Further - we need to do processing to come up with the CEDD image bits.

                #region Special Processing (cropping, etc.)

                // vendors with images which tend to have white space
                var knownSKUsWithWhiteSpace = new string[] { "BL", "CS", "GP", "GW", "KR", "LA", "LJ", "MB", "PT", "TH", "RL", "WT", "BF" };

                var knownSKUsWithTopBottomWhiteSpace = new string[] { "BF" };

                // vendors with images which are way horizonatal and need cropping
                var knownSKUsWithHorizontalImages = new string[] { "IN", "RA", "BH" };

                Func<string[], bool> isMatchedSKUPrefix = (skuPrefixList) =>
                {
                    foreach (var skuPrefix in skuPrefixList)
                    {
                        if (SKU.StartsWith(string.Format("{0}-", skuPrefix), StringComparison.OrdinalIgnoreCase))
                            return true;
                    }
                    return false;
                };

                // Example:
                // productID: 1106536
                //http://s3.amazonaws.com/images2.eprevue.net/p4dbimg/400/image1024/22947_16.jpg

                //standard white on just the sides
                //http://s3.amazonaws.com/images2.eprevue.net/p4dbimg/400/image1024/22905_19.jpg
                //http://s3.amazonaws.com/images2.eprevue.net/p4dbimg/400/image1024/5403_71.jpg
                //http://s3.amazonaws.com/images2.eprevue.net/p4dbimg/400/image1024/17456_4.jpg

                //Has white and drop shadow...white white on top/bottom too
                //http://s3.amazonaws.com/images2.eprevue.net/p4dbimg/400/image1024/8189_7.jpg

                // has drop shadow
                // http://www.insidefabric.com/p-1106258-19105-324-flirtation-carnival-by-kravet-couture.aspx

                // 850x486
                // D:\VendorImages\Kravet\KF\HIRES\3529_16.JPG
                // http://scanner.insidefabric.com/vendors/Kravet/KF/HIRES/3529_16.JPG

                // anything from http://s3.amazonaws.com/images.eprevue.net is Kravet or Ralph Lauren needing to be cropped

                // BF has white on top and bottom
                // ftp://file.kravet.com/BF/HIRES/JAG-50048_1550.JPG

                bool requiresCropWhiteSpace = false;

                // need cropping?

                if (isMatchedSKUPrefix(knownSKUsWithWhiteSpace))
                {
                    // image[] at this point is the JPG
                    if (image.HasWhiteSpaceAroundImage())
                        requiresCropWhiteSpace = true;
                }

                if (requiresCropWhiteSpace)
                {
                    // adjust contents of image[] as needed

                    var bmp = image.FromImageByteArrayToBitmap();
                    if (bmp != null)
                    {
                        // remember dim so can tell if cropped
                        var originalWidth = bmp.Width;
                        var originalHeight = bmp.Height;

                        // the images are not quite 100% white
                        var croppedBmp = bmp.CropWhiteSpace(0.998f);


                        Func<bool> wasCropped = () =>
                            {
                                if (croppedBmp.Width == originalWidth && croppedBmp.Height == originalHeight)
                                    return false;

                                return true;
                            };

                        if (wasCropped())
                        {
                            if (isMatchedSKUPrefix(knownSKUsWithTopBottomWhiteSpace))
                            {
                                croppedBmp = croppedBmp.CropTopAndBottom(1);
                            }
                            else
                            {

                                // based on the premise that these ones needing to be cropped are cropped from the sides only,
                                // take another pixel just from the sides.

                                // the white space crop gets within 1px (seems due to anti-aliasing) - so we 
                                // than force one more px off each side. Then back to jpg.

                                croppedBmp = croppedBmp.CropSides(1);

                                if (croppedBmp.HasKravetDropShadow())
                                {
                                    // has a shadow, crop 12px from the bottom, 8px from the right
                                    // based on our experience
                                    croppedBmp.Dispose();
                                    croppedBmp = croppedBmp.CropDropShadow(12, 8);
                                }

                            }

                            image = croppedBmp.ToJpeg(95);

                            // recompute dimensions now that the image has changed
                            GetImageDimensions(image, out Width, out Height);
                            croppedBmp.Dispose();
                        }

                        bmp.Dispose();
                    }
                }

                // many images (but not all) from seabrook need to be cropped
                //if (imageUrl.Contains("seabrookwallpaper"))
                //{
                //    var bmp = image.FromImageByteArrayToBitmap();

                //    // if the corners are gray then crop 1px off the sides
                //    if (bmp != null && bmp.HasGrayCorners())
                //    {
                //        var croppedBmp = bmp.CropBorder(1);

                //        // crop off whitespace
                //        croppedBmp = croppedBmp.CropWhiteSpace(0.998f);

                //        image = croppedBmp.ToJpeg(80);

                //        // recompute dimensions now that the image has changed
                //        GetImageDimensions(image, out Width, out Height);
                //    }
                //}

                // if totally a horizontal image - should crop
                // the 100 is just to make sure we have something real - no real merit to that specific number
                if ((isMatchedSKUPrefix(knownSKUsWithHorizontalImages) || isMatchedSKUPrefix(knownSKUsWithWhiteSpace)) && Width.Value > 100)
                {
                    // because Kravet seems to have some horizontal images too

                    if (Width.Value > Height.Value * 1.5)
                    {
                        // crop evenly from both left and right

                        var bmp = image.FromImageByteArrayToBitmap();
                        if (bmp != null)
                        {
                            var cropFromSides = (Width.Value - Height.Value) / 2;
                            image = bmp.CropSides(cropFromSides).ToJpeg(95);
                            // recompute dimensions now that the image has changed
                            GetImageDimensions(image, out Width, out Height);
                            bmp.Dispose();
                        }
                    }
                }

                #endregion

                if (Width < 5 || Height < 5)
                    throw new Exception("Cropped image too small. Something is wrong.");

                // image[] is now good in memory as our original

                var productImageOriginal = GetImageFilepathForFolder(filename, "Original"); 

                if (File.Exists(productImageOriginal))
                    File.Delete(productImageOriginal);

                WriteBinaryFile(productImageOriginal, image);
 

                //Debug.WriteLine(productImageOriginal);

                Action<string> doOperation = (folderName) =>
                    {
                        var info = SizingInformation.Single(e => e.FolderName.Equals(folderName, StringComparison.OrdinalIgnoreCase));

                        var imageFilepath = GetImageFilepathForFolder(filename, folderName);

                        if (File.Exists(imageFilepath))
                            File.Delete(imageFilepath);

                        byte[] resizedImage = info.Resize(image);

                        if (resizedImage != null)
                        {
                            // only the larger sizes get have properties embedded

                            if (info.IncludeExifProperties)
                            {
                                //Debug.WriteLine(string.Format("Writing EXIF properties to: {0}", imageFilepath));
                                resizedImage = EmbedExifProperties(resizedImage, exifProperties);
                            }
                            resizedImage.WriteBinaryFile(imageFilepath);
                        }
                    };

#if false
                foreach (var folderName in Store.ImageFolderNames.Where(e => e != "Original"))
                {
                    doOperation(folderName);
                }
#else
                Parallel.ForEach(Store.ImageFolderNames.Where(e => e != "Original"), (folderName) =>
                {
                    doOperation(folderName);
                });
#endif
                return true;

            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.ToString());
                return false;
            }
        }
        
        #endregion

        #region Static Methods

        /// <summary>
        /// Embed the provided properties as meta data into the image, then return the updated image bytes.
        /// </summary>
        /// <param name="imageBytes"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static byte[] EmbedExifProperties(byte[] imageBytes, ProductExifProperties properties)
        {
            try
            {
                // if not looking right, do no harm, just return what we got in the first place
                if (properties == null || imageBytes == null || !imageBytes.HasJpgImagePreamble())
                    return imageBytes;

                ImageFile jpg = null;

                using (var stream = new MemoryStream(imageBytes.Length))
                {
                    stream.Write(imageBytes, 0, imageBytes.Length);
                    stream.Position = 0;

                    jpg = JPEGFile.FromStream(stream);
                }

                Action<ExifTag, string> SetProp = (tag, str) =>
                {
                    if (!string.IsNullOrWhiteSpace(str))
                        jpg.Properties.Set(tag, str);

                };

                // clear out what's there, add in the new settings

                jpg.Properties.Clear();

                SetProp(ExifTag.Artist, properties.Artist);
                SetProp(ExifTag.ImageDescription, properties.Description);
                SetProp(ExifTag.WindowsKeywords, properties.Keywords);
                SetProp(ExifTag.UserComment, properties.Comment);

                // return the updated image bytes

                using (MemoryStream ms = new MemoryStream())
                {
                    jpg.Save(ms);
                    return ms.ToArray();
                }

            }
            catch (Exception Ex)
            {
                // if anything goes wrong, punt and return what we rec'd on input

                Debug.WriteLine("Exception: " + Ex.Message);
                return imageBytes;
            }
        }

        /// <summary>
        /// Make a deterministic filename in guid like format from a url.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string MakeCacheFilename(string url)
        {
            // ***** this note may not be true! All might be fine just as it is.
            // NOTE - for Jaipur, the orignal cache name for utility was based on having LRes in the URL. This will
            // need to be manipulated such that when the code is fixed to correctly look for HRes, that we deal with
            // the fact that the cachename for cached files is based on the LRes and not Hres. So if not handled,
            // we'll think we don't have an image that we actually do have.

            var lowerUrl = url.ToLower();
            var digest = lowerUrl.SHA256Digest().ToLower();
            var ext = Path.GetExtension(url).ToLower();
            var filename = string.Format("{0}{1}", digest, ext);
            return filename;
        }

        protected static void GetImageDimensions(byte[] ContentData, out int? Width, out int? Height)
        {
            Width = null;
            Height = null;

            if (ContentData.Length > 0)
            {
                try
                {
                    using (var bmp = ContentData.FromImageByteArrayToBitmap())
                    {
                        Width = bmp.Width;
                        Height = bmp.Height;
                    }
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Persist some of this data to a fast table for quick bulk loading by data service.
        /// </summary>
        /// <remarks>
        /// Also called from some of the product maint loops when making changes to persist.
        /// </remarks>
        /// <param name="dc"></param>
        /// <param name="productID"></param>
        /// <param name="features"></param>
        public static void SaveProductImageFeatures(AspStoreDataContext dc, int productID, ImageFeatures features)
        {
            try
            {
                var sbColors = new StringBuilder();

                bool isFirstColor = true;
                foreach (var color in features.DominantColors)
                {
                    if (!isFirstColor)
                        sbColors.Append(";");

                    sbColors.Append(color.Replace("#FF", "#")); // from ARGB to RGB

                    isFirstColor = false;
                }

                var productFeatures = dc.ProductFeatures.Where(e => e.ProductID == productID).FirstOrDefault();
                if (productFeatures == null)
                {
                    // insert
                    productFeatures = new ProductFeature()
                    {
                        ProductID = productID,
                        TinyImageDescriptor = (int)features.TinyCEDD,
                        ImageDescriptor = features.CEDD,
                        Colors = sbColors.ToString(),
                    };
                    dc.ProductFeatures.InsertOnSubmit(productFeatures);
                    dc.SubmitChanges();
                }
                else
                {
                    // update
                    productFeatures.TinyImageDescriptor = (int)features.TinyCEDD;
                    productFeatures.ImageDescriptor = features.CEDD;
                    productFeatures.Colors = sbColors.ToString();
                    dc.SubmitChanges();
                }
            }
            catch
            { }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// When we're using a cache folder to store pre-downloaded vendor images, this 
        /// will yield a complete filepath using a munged name in a cache folder.
        /// </summary>
        /// <remarks>
        /// The file ext is preserved.
        /// </remarks>
        /// <param name="url"></param>
        /// <returns></returns>
        protected string MakeCacheFilepath(string url)
        {
            // startup methods have ensured this folder exists
            return Path.Combine(Store.ImageDownloadCacheFolder, MakeCacheFilename(url));
        }

        /// <summary>
        /// Gives a full filepath for an image which lives in the specified folder.
        /// </summary>
        /// <param name="imageFilename"></param>
        /// <param name="folderName"></param>
        /// <returns></returns>
        protected string GetImageFilepathForFolder(string imageFilename, string folderName)
        {
            var webRoot = Store.PathWebsiteRoot;

            if (string.IsNullOrWhiteSpace(webRoot))
                throw new Exception("Missing website root path.");

            return Path.Combine(new string[] { webRoot, "images", "product", folderName, imageFilename });
        }



        /// <summary>
        /// Presently called for IF and IW to re-visit images and create/fill-in sizes. Not presently used for ingesting images.
        /// </summary>
        /// <remarks>
        /// This sizing stuff likely can all be removed since we're migrating to something better (Oct 1, 2015).
        /// </remarks>
        /// <param name="Store"></param>
        /// <param name="ImageFilenameOverride"></param>
        /// <param name="forceAll"></param>
        public void RefreshImages(string ImageFilenameOverride, bool forceAll)
        {

            // exit if don't have an image in the first place
            if (string.IsNullOrWhiteSpace(ImageFilenameOverride))
                return;

            try
            {

                // get best source image available, try downgrading if needed
                // dev PCs will frequently not have the original images to work from - see if maybe medium is around
                var originalImageFilepath = GetImageFilepathForFolder(ImageFilenameOverride, "Original");

                if (!File.Exists(originalImageFilepath))
                {
                    originalImageFilepath = GetImageFilepathForFolder(ImageFilenameOverride, "Large");
                    if (!File.Exists(originalImageFilepath))
                    {
                        originalImageFilepath = GetImageFilepathForFolder(ImageFilenameOverride, "Medium");
                        if (!File.Exists(originalImageFilepath))
                            return;
                    }
                }

                var originalImage = originalImageFilepath.ReadBinaryFile();

                Action<string> doOperation = (folderName) =>
                    {
                        var info = SizingInformation.Single(e => e.FolderName.Equals(folderName, StringComparison.OrdinalIgnoreCase));

                        var imageFilepath = GetImageFilepathForFolder(ImageFilenameOverride, folderName);

                        byte[] imageBytes = null;

                        if (!forceAll && File.Exists(imageFilepath))
                        {
                            // nothing to be done if exists and is already the right size

                            imageBytes = imageFilepath.ReadBinaryFile();
                            if ((!info.MaxBytes.HasValue || imageBytes.Length < info.MaxBytes.Value) && (!info.SquareDimension.HasValue || imageBytes.IsSquareImage(info.SquareDimension.Value)))
                                return;
                        }

                        if (File.Exists(imageFilepath))
                            File.Delete(imageFilepath);

                        byte[] resizedImage = info.Resize(originalImage);

                        if (resizedImage != null)
                            resizedImage.WriteBinaryFile(imageFilepath);
                    };

#if false
                foreach (var folderName in Store.ImageFolderNames.Where(e => e != "Original"))
                {
                    doOperation(folderName);
                }
#else
                Parallel.ForEach(Store.ImageFolderNames.Where(e => e != "Original"), (folderName) =>
                {
                    doOperation(folderName);
                });

                // performance about the same
                //var tasks = new List<Task>();
                //foreach (var folderName in Store.ImageFolderNames.Where(e => e != "Original"))
                //    tasks.Add(Task.Run(() => doOperation(folderName)));

                //Task.WhenAll(tasks).Wait();
#endif
            }

            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.ToString());
            }
        }


        /// <summary>
        /// List of all image folder paths (micro, mini, icon, small, medium, large, etc.) for the given image filename but not including original.
        /// </summary>
        protected List<string> ImageFolderPathsWithoutOriginal(string imageFilename)
        {
            var list = new List<string>();

            foreach(var folderName in Store.ImageFolderNames.Where(e => e != "Original"))
            {
                var filepath = GetImageFilepathForFolder(imageFilename, folderName);
                list.Add(filepath);
            }
                
            return list;

        }

        /// <summary>
        /// List of all image folder paths (micro, mini, icon, small, medium, large, etc.) for the given image filename including original.
        /// </summary>
        protected List<string> ImageFolderPathsIncludingOriginal(string imageFilename)
        {
            var list = new List<string>();

            foreach (var folderName in Store.ImageFolderNames)
            {
                var filepath = GetImageFilepathForFolder(imageFilename, folderName);
                list.Add(filepath);
            }

            return list;

        }

        /// <summary>
        /// Delete named image from all folders.
        /// </summary>
        /// <param name="imageFilename"></param>
        protected void DeleteImageFilename(string imageFilename)
        {
            try
            {
                foreach (var filePath in ImageFolderPathsIncludingOriginal(imageFilename))
                {
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                }
            }
            catch { }
        }



        /// <summary>
        /// Does this image name exist in any folder?
        /// </summary>
        /// <param name="imageFilename"></param>
        /// <returns></returns>
        protected bool ImageFileNameExists(string imageFilename)
        {
            foreach (var filePath in ImageFolderPathsWithoutOriginal(imageFilename))
            {
                if (File.Exists(filePath))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Rename this image across all folders, including original.
        /// </summary>
        /// <param name="oldImageFilename"></param>
        /// <param name="newImageFilename"></param>
        protected void RenameImageFile(string oldImageFilename, string newImageFilename)
        {
            try
            {
                if (!ImageFileNameExists(oldImageFilename) || ImageFileNameExists(newImageFilename))
                    throw new Exception("Incompatible image rename parameters.");

                var oldNames = ImageFolderPathsIncludingOriginal(oldImageFilename);
                var newNames = ImageFolderPathsIncludingOriginal(newImageFilename);

                for(int i=0; i < oldNames.Count(); i++)
                {
                    if (File.Exists(oldNames[i]))
                        File.Move(oldNames[i], newNames[i]);
                }
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex);
                throw new Exception("Error renaming image file.");
            }
        }


        /// <summary>
        /// Create an image features property set for the given filename.
        /// </summary>
        /// <remarks>
        /// The file is expected to already exist in the suite of various-sized folders.
        /// Will prefer to use the 800x (large) image.
        /// </remarks>
        /// <returns></returns>
        public ImageFeatures MakeImageFeatures(string filename)
        {
            try
            {

                // the features are all computed by the imaging library, but we don't really want the library
                // to have any unnecessary dependencies - so each of these properties are computed separately
                // rather than having the imaging class do it within.

                var startTime = DateTime.Now;

                var filepath = GetImageFilepathForFolder(filename, "Large");

                if (!File.Exists(filepath))
                    return null;

                var imageBytes = filepath.ReadBinaryFile();

                int? width;
                int? height;

                GetImageDimensions(imageBytes, out width, out height);

                // always use 800x800 for CEDD calculations
                if (width.GetValueOrDefault() != 800 || height.GetValueOrDefault() != 800)
                    imageBytes = imageBytes.ResizeImageAsSquare(800, 90);

                using (var bmp = imageBytes.FromImageByteArrayToBitmap())
                {
                    var bmsrc = bmp.ToBitmapSource();
                    var cedd = bmsrc.CalculateDescriptor();

                    // use smaller size for dominant colors since takes so long
                    var filepath2 = GetImageFilepathForFolder(filename, "Small");
                    var imageBytes2 = filepath2.ReadBinaryFile();

                    using (var bmp2 = imageBytes2.FromImageByteArrayToBitmap())
                    {

                        ColorImageQuantizer ciq = new ColorImageQuantizer(new MedianCutQuantizer());
                        var domColors = ciq.CalculatePalette(bmp2, 4); // 4-color palette
                        var singleColor = ciq.CalculatePalette(bmp2, 1); // single best color representing image

                        Func<System.Drawing.Color, string> colorToString = (c) =>
                        {
                            // save as #AARRGGB
                            return string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", c.A, c.R, c.G, c.B);
                        };

                        var features = new ImageFeatures()
                        {
                            Filename = filename,
                            CEDD = cedd,
                            TinyCEDD = TinyDescriptors.MakeTinyDescriptor(cedd),
                            DominantColors = domColors.Select(e => colorToString(e)).ToList(),
                            BestColor = colorToString(singleColor[0]),
                        };

                        var endTime = DateTime.Now;
                        Debug.WriteLine(string.Format("Image features for {0}: {1}", filename, endTime - startTime));

                        return features;
                    }
                }
            }
            catch (Exception Ex)
            {
                Debug.WriteLine("Exception: " + Ex.Message);
                return null;
            }
        }


        /// <summary>
        /// Write the bytes (typically image bytes) to disk.
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="data"></param>
        protected void WriteBinaryFile(string filepath, byte[] data)
        {
            using (var fs = new FileStream(filepath, FileMode.CreateNew, FileAccess.Write))
            {
                fs.Write(data, 0, data.Length);
            }
        }


        /// <summary>
        /// Give a bitmap from the provided image bytes. Must be disposed.
        /// </summary>
        /// <param name="ContentData"></param>
        /// <returns></returns>
        protected Bitmap FromImageByteArrayToBitmap(byte[] ContentData)
        {
            try
            {
                if (ContentData.Length > 0)
                {
                    using (var stream = new MemoryStream(ContentData.Length))
                    {
                        stream.Write(ContentData, 0, ContentData.Length);
                        stream.Position = 0;

                        var bmp = new Bitmap(stream);
                        return bmp;
                    }
                }
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Determines if we will allow this filename for an image.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        protected bool IsSafeFilename(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return false;

            foreach (var c in filename)
            {
                if (c >= 'A' && c <= 'Z')
                    continue;

                if (c >= 'a' && c <= 'z')
                    continue;

                if (c >= '0' && c <= '9')
                    continue;

                if (".-_".Contains(c))
                    continue;

                return false;
            }

            return true;
        }




        protected LiveProductImage SaveDefaultImage(DownloadedProductImage pickedDefaultImage, ProductExifProperties exifProperties)
        {


            var image = pickedDefaultImage.GetListingImage();

            // we're generating a new file on the fly, needs a name
            var filename = string.Format("{0}.jpg", Guid.NewGuid());

            var productImageOriginal = GetImageFilepathForFolder(filename, "Original");

            if (File.Exists(productImageOriginal))
                File.Delete(productImageOriginal);

            WriteBinaryFile(productImageOriginal, image);

            Action<string> doOperation = (folderName) =>
            {
                var info = SizingInformation.Single(e => e.FolderName.Equals(folderName, StringComparison.OrdinalIgnoreCase));

                var imageFilepath = GetImageFilepathForFolder(filename, folderName);

                if (File.Exists(imageFilepath))
                    File.Delete(imageFilepath);

                byte[] resizedImage = info.Resize(image);

                if (resizedImage != null)
                {
                    // only the larger sizes get have properties embedded

                    if (info.IncludeExifProperties)
                    {
                        resizedImage = EmbedExifProperties(resizedImage, exifProperties);
                    }
                    resizedImage.WriteBinaryFile(imageFilepath);
                }
            };

            Parallel.ForEach(Store.ImageFolderNames.Where(e => e != "Original"), (folderName) =>
            {
                doOperation(folderName);
            });

            int? width;
            int? height;
            GetImageDimensions(image, out width, out height);

            var liveImage = new LiveProductImage()
            {
                Filename = filename,
                IsGenerated = true,
                IsDefault = true,
                IsDisplayedOnDetailPage = false,
                ImageVariant = "Listing",
                CreatedOn = DateTime.Now,
                CroppedWidth = width.Value,
                CroppedHeight = height.Value,
                SourceUrl = null,
            };

            return liveImage;
        }

        /// <summary>
        /// Save the set of images that will show in detail pages.
        /// </summary>
        /// <remarks>
        /// Smaller ones are on white squares, showing full cropped image to fit, and
        /// medium/large are constrained to bounding box.
        /// </remarks>
        /// <param name="downloadedImages"></param>
        protected List<LiveProductImage> SaveNonDefaultImages(List<DownloadedProductImage> downloadedImages, ProductExifProperties exifProperties)
        {
            var liveProductImages = new List<LiveProductImage>();
            foreach (var downloadedImage in downloadedImages.Where(e => !e.IsDuplicate))
            {
                var productImageOriginal = GetImageFilepathForFolder(downloadedImage.Filename, "Original");

                if (File.Exists(productImageOriginal))
                    File.Delete(productImageOriginal);

                WriteBinaryFile(productImageOriginal, downloadedImage.OriginalAsJpeg);

                // get a high-res square image to work with, cropped image is embedded on white background
                var squareImage = downloadedImage.GetSquareCroppedImage();

                Action<string> doOperation = (folderName) =>
                {
                    byte[] resizedImage = null;
                    bool includeExifProperties = false;
                    switch (folderName)
                    {
                        case "Micro":
                            resizedImage = squareImage.MakeMicroImage50();
                            break;

                        case "Mini":
                            resizedImage = squareImage.MakeMiniImage100();
                            break;

                        case "Icon":
                            resizedImage = squareImage.MakeIconImage150();
                            break;

                        case "Small":
                            resizedImage = squareImage.MakeSmallImage225();
                            break;

                        case "Medium":
                            resizedImage = downloadedImage.GetSizeConstrainedCroppedImage(350, 450, true, 85);
                            includeExifProperties = true;
                            break;

                        case "Large":
                            resizedImage = downloadedImage.GetSizeConstrainedCroppedImage(1000, 1800, false, 80);
                            includeExifProperties = true;
                            break;

                    }

                    var imageFilepath = GetImageFilepathForFolder(downloadedImage.Filename, folderName);

                    if (File.Exists(imageFilepath))
                        File.Delete(imageFilepath);

                    if (resizedImage != null)
                    {
                        // only the larger sizes get have properties embedded

                        if (includeExifProperties)
                            resizedImage = EmbedExifProperties(resizedImage, exifProperties);

                        resizedImage.WriteBinaryFile(imageFilepath);
                    }
                };

                Parallel.ForEach(Store.ImageFolderNames.Where(e => e != "Original"), (folderName) =>
                {
                    doOperation(folderName);
                });

                var isDefault = downloadedImage.ImageVariant == "Primary" ? true : false;
                liveProductImages.Add(downloadedImage.MakeLiveProductImage(isDefault: isDefault, isDisplayedOnDetailPage: true));
            }

            return liveProductImages;
        }

        /// <summary>
        /// Get the bytes for a single URL, using cache when possible.
        /// </summary>
        /// <param name="imageUrl"></param>
        /// <returns>Bytes or null. Does not throw.</returns>
        protected byte[] DownloadImage(string imageUrl)
        {
            byte[] imageBytes = null;
            string cacheFilepath = MakeCacheFilepath(imageUrl);

            if (Store.UseImageDownloadCache)
            {
                // the cache file is in the original format jpg|png|tif from vendor, using 
                // a GUID-like name, but with the correct extension. No processing at all is done
                // other than the rename to avoid conflicts.

                if (File.Exists(cacheFilepath))
                {
                    imageBytes = cacheFilepath.ReadBinaryFile();
                    if (imageBytes.HasImagePreamble())
                        return imageBytes;
                }
            }

            // tiny retry loop (twice max)

            for (int i = 1; i <= 2; i++)
            {
                imageBytes = imageUrl.GetImageFromWeb();
                if (imageBytes != null)
                    break;
                Thread.Sleep(i * 10 * 1000);
            }

            // imageBytes is guaranteed to be null or true image

            if (Store.UseImageDownloadCache && imageBytes != null)
            {
                if (File.Exists(cacheFilepath))
                    File.Delete(cacheFilepath);

                imageBytes.WriteBinaryFile(cacheFilepath);
            }

            return imageBytes;
        }

        /// <summary>
        /// Delete any existing image across all folders.
        /// </summary>
        /// <param name="filenames"></param>
        protected void DeleteExistingImages(List<string> filenames)
        {
            foreach (var filename in filenames)
                DeleteImageFilename(filename);
        }


        protected ImageFeatures MakeImageFeatures(DownloadedProductImage image)
        {
            try
            {
                // the features are all computed by the imaging library, but we don't really want the library
                // to have any unnecessary dependencies - so each of these properties are computed separately
                // rather than having the imaging class do it within.

                using (var bmp = image.GetAnalysisImage().FromImageByteArrayToBitmap())
                {
                    var bmsrc = bmp.ToBitmapSource();
                    var cedd = bmsrc.CalculateDescriptor();

                    // dom color stuff computed using a smaller image since faster that way
                    double factor = 250.0 / (double)bmp.Width;

                    using (var bmp2 = new Bitmap(bmp, new Size((int)(bmp.Width * factor), (int)(bmp.Height * factor))))
                    {
                        ColorImageQuantizer ciq = new ColorImageQuantizer(new MedianCutQuantizer());
                        var domColors = ciq.CalculatePalette(bmp2, 4); // 4-color palette
                        var singleColor = ciq.CalculatePalette(bmp2, 1); // single best color representing image

                        Func<System.Drawing.Color, string> colorToString = (c) =>
                        {
                            // save as #AARRGGB
                            return string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", c.A, c.R, c.G, c.B);
                        };

                        var features = new ImageFeatures()
                        {
                            Filename = image.Filename, // technically, for rugs, this file exists only in memory
                            CEDD = cedd,
                            TinyCEDD = TinyDescriptors.MakeTinyDescriptor(cedd),
                            DominantColors = domColors.Select(e => colorToString(e)).ToList(),
                            BestColor = colorToString(singleColor[0]),
                        };

                        return features;
                    }
                }
            }
            catch (Exception Ex)
            {
                Debug.WriteLine("Exception: " + Ex.Message);
                return null;
            }
        }


        #endregion
    }

}