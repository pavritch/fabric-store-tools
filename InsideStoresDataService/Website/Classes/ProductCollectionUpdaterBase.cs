using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Transactions;
using Ionic.Zip;
using Gen4.Util.Misc;
using System.Threading;
using Website.Entities;
using System.Data.Linq;

namespace Website
{
    public abstract class ProductCollectionUpdaterBase : IProductCollectionUpdater
    {
        public class MemberProduct
        {
            public int ProductID { get; set; }
            public bool IsDiscontinued { get; set; }
            public string ProductGroup { get; set; }
            public decimal OurPrice { get; set; }  // This does not take rug variants into account. Always fills in with our price of default variant.
            public bool OutOfStock { get; set; }
            public string ImageFilenameOverride { get; set; }
            public DateTime CreatedOn { get; set; }
        }

        private string _collectionImageFolder;

        protected bool proccessImages = true;
        protected Action<string> reportStatusCallback;
        protected CancellationToken cancelToken;
        protected int ManufacturerID { get; private set; }
        protected IWebStore Store { get; private set; }
        protected List<ProductCollection> Collections;
        protected AspStoreDataContext DataContext {get; private set;}
        protected Dictionary<string, List<MemberProduct>> GroupedProducts; // key is upper case
        protected List<ProductCollection> MatchedCollections;
        protected List<ProductCollection> DeletedCollections;
        protected List<ProductCollection> InsertedCollections;
        protected List<string> NewCollectionKeys; // upper case
        protected Manufacturer manufacturer;

        public ProductCollectionUpdaterBase(IWebStore store, int manufacturerID)
        {
            this.ManufacturerID = manufacturerID;
            this.Store = store;
            this.cancelToken = CancellationToken.None;
            this.reportStatusCallback = null;

//#if DEBUG
//            // skip creating/deleting images for debug builds
//            proccessImages = false;
//#endif
        }

        protected abstract List<ProductCollection> FetchExistingCollections();
        protected abstract Dictionary<string, List<MemberProduct>> FetchGroupedProducts();
        protected abstract void PerformMatchups();
        protected abstract void PerformUpdates();
        protected abstract void RebuildImages();
        protected abstract ProductCollection MakeNewCollectionRecord(string key);
        protected abstract string MakeImageFilenameOverride(ProductCollection rec);


        protected virtual List<ProductCollection> FetchExistingCollections(int kindOfCollection)
        {
            return DataContext.ProductCollections.Where(e => e.ManufacturerID == ManufacturerID && e.Kind == kindOfCollection).ToList();
        }

        protected virtual string DisplayPrefix
        {
            get
            {
                return string.Empty;
            }
        }

        protected Manufacturer Manufacturer
        {
            get
            {
                if (manufacturer == null)
                    manufacturer = DataContext.Manufacturers.Where(e => e.ManufacturerID == ManufacturerID).First();

                return manufacturer;
            }
        }

        protected bool IsCancelled
        {
            get
            {
                return cancelToken.IsCancellationRequested;
            }
        }

        /// <summary>
        /// This is the folder where all images should go relating to collections.
        /// </summary>
        /// <remarks>
        /// All have CollectionID in filename, so no worry about name collision.
        /// </remarks>
        protected string CollectionImageFolder
        {
            get
            {
                if (_collectionImageFolder != null)
                    return _collectionImageFolder;

                _collectionImageFolder = Path.Combine(Store.PathWebsiteRoot, "images\\collections");

                if (!Directory.Exists(_collectionImageFolder))
                    Directory.CreateDirectory(_collectionImageFolder);

                return _collectionImageFolder;
            }
        }

        protected string CleanManufacturerName
        {
            get
            {
                return Manufacturer.Name.Replace(" Fabrics", "").Replace(" Fabric", "").Replace(" Wallpapers", "").Replace(" Wallpaper", "").Replace(" Wallcoverings", "").Replace(" Wallcovering", "").Replace(" Rugs", "");
            }
        }

        protected virtual byte[] MakeSimpleImage(ProductCollection rec, List<MemberProduct> members)
        {
            // take the first image

            var firstImageName = members.Where(e => !string.IsNullOrWhiteSpace(e.ImageFilenameOverride)).Select(e => e.ImageFilenameOverride).FirstOrDefault();

            if (firstImageName == null)
                return null;

            var imageFilepath = Path.Combine(Store.PathWebsiteRoot, "images\\product", SizedImageFolderName, firstImageName);

            if (!File.Exists(imageFilepath))
                return null;

            var imageBytes = imageFilepath.ReadBinaryFile();

            return imageBytes;
        }

        protected string SizedImageFolderName
        {
            get
            {
#if DEBUG
                return Store.ImageFolderNames[2]; // icon 150 x 150
#else
                return Store.ImageFolderNames[3]; // small 225x225
#endif
            }
        }
        protected virtual byte[] MakeImage(ProductCollection rec, List<MemberProduct> members)
        {
            try
            {
                var imageFolder = Path.Combine(Store.PathWebsiteRoot, "images\\product", SizedImageFolderName);

                var memberImagesFilepaths = members
                    .Where(e => !string.IsNullOrEmpty(e.ImageFilenameOverride) && File.Exists(Path.Combine(imageFolder, e.ImageFilenameOverride)))
                    .Select(e => Path.Combine(imageFolder, e.ImageFilenameOverride)).ToList();

                if (memberImagesFilepaths.Count() == 0)
                    return null;

                memberImagesFilepaths.Shuffle();
                var selectedImagesFilepaths = memberImagesFilepaths.Take(4).ToList();

                // for 69px overlay on 150px background
                var imagePositions150 = new int[][] 
                    {
                        new int[] {4, 4},
                        new int[] {77, 4},
                        new int[] {4, 77},
                        new int[] {77, 77},
                    };

                // for 102px overlay on 225px background
                var imagePositions225 = new int[][] 
                    {
                        new int[] {7, 7},
                        new int[] {116, 7},
                        new int[] {6, 116},
                        new int[] {116, 116},
                    };

                int[][] imagePositions = null;
                int imageOverlaySize = 0;
                string backgroundImageFilepath;

                // small size (225px)
                imagePositions = imagePositions225;
                imageOverlaySize = 102;
                backgroundImageFilepath = Path.Combine(Store.PathWebsiteRoot, "images\\BlankCollectionBackground225.png");

                //{
                //    // icon size (150px)
                //    imagePositions = imagePositions150;
                //    imageOverlaySize = 69;
                //    backgroundImageFilepath = Path.Combine(Store.PathWebsiteRoot, "images\\BlankCollectionBackground.png");
                //}

                var backgroundImageBytes = backgroundImageFilepath.ReadBinaryFile();

                var imgDraw = new Neodynamic.WebControls.ImageDraw.ImageDraw();
                var backgroundImgElem = new Neodynamic.WebControls.ImageDraw.ImageElement { Source = Neodynamic.WebControls.ImageDraw.ImageSource.Binary, SourceBinary = backgroundImageBytes, X = 0, Y = 0, PreserveMetaData = false };
                imgDraw.Elements.Add(backgroundImgElem);

                int position = 0; // position to paste next image
                foreach (var imageFilepath in selectedImagesFilepaths)
                {
                    var imageBytes = imageFilepath.ReadBinaryFile();
                    if (imageBytes == null || imageBytes.Length == 0)
                        continue;

                    var resizedImageBytes = ResizeImage(imageBytes, imageOverlaySize);
                    var imgElem = new Neodynamic.WebControls.ImageDraw.ImageElement { Source = Neodynamic.WebControls.ImageDraw.ImageSource.Binary, SourceBinary = resizedImageBytes, X = imagePositions[position][0], Y = imagePositions[position][1], PreserveMetaData = false };

                    imgDraw.Elements.Add(imgElem);
                    position++;

                }

                imgDraw.ImageFormat = Neodynamic.WebControls.ImageDraw.ImageDrawFormat.Jpeg;
                imgDraw.JpegCompressionLevel = 95;
                var composedImage = imgDraw.GetOutputImageBinary();

                return composedImage;
            }
            catch (Exception Ex)
            {
                var msg = string.Format("Exception in ProductCollectionUpdaterBase.MakeImage() for Collection: {0}", rec.ProductCollectionID);
                Debug.WriteLine(msg);
                Debug.WriteLine(Ex.Message);
                var ev2 = new WebsiteRequestErrorEvent(msg, this, WebsiteEventCode.UnhandledException, Ex);
                ev2.Raise();

                return null;
            }
        }


        protected virtual void PerformMatchups(Func<ProductCollection, string> collectionKey)
        {
            // all key comparisons in upper case

#if true
            var dicExisting = new Dictionary<string, ProductCollection>();
            foreach(var item in Collections)
            {
                var key = collectionKey(item).ToUpper();
                if (!dicExisting.ContainsKey(key))
                    dicExisting[key] = item;
                else
                    Debug.WriteLine(string.Format("Duplicate collection key for {0} detected and skipped: {1}", item.Name, key));
            }
#else
            // observed a duplicate key exception
            var dicExisting = Collections.ToDictionary(k => collectionKey(k).ToUpper(), v => v);
#endif
            foreach (var key in GroupedProducts.Keys)
            {
                ProductCollection pc;

                if (dicExisting.TryGetValue(key, out pc))
                    MatchedCollections.Add(pc);
                else if (GroupedProducts[key].Count() > 1)
                    NewCollectionKeys.Add(key);
            }

            foreach (var item in Collections)
            {
                if (!GroupedProducts.ContainsKey(collectionKey(item).ToUpper()))
                    DeletedCollections.Add(item);
            }
        }

        protected virtual void PerformUpdates(Func<ProductCollection, string> collectionKey)
        {
            // doing one at a time on purpose - don't want to lock things up in SQL,
            // this is not a time sensitive process

            int displayCounter = 50;
            int countCompleted = 0;



            foreach (var record in MatchedCollections)
            {
                if (IsCancelled)
                    break;

                if (displayCounter == 50)
                {
                    DisplayStatusMessage(string.Format("{0}Updating {1:N0}", DisplayPrefix, countCompleted));
                    displayCounter = 0;
                }
                else
                    displayCounter++;

                var key = collectionKey(record).ToUpper();

                // in theory, this key should always be there due to earlier stage logic

                if (!GroupedProducts.ContainsKey(key))
                    continue;

                var members = GroupedProducts[key];

                UpdateCollectionRecord(record, members);

#if !DEBUG
                // verify image, attempt to autocorrect if things should be one but not found on disk.


                if (record.ImageFilenameOverride != null)
                {
                    var imageFilepath = Path.Combine(CollectionImageFolder, record.ImageFilenameOverride);

                    try
                    {
                        bool isGoodImage = File.Exists(imageFilepath);

                        if (!isGoodImage)
                        {
                            var imageBytes = MakeImage(record, members);
                            if (imageBytes != null)
                            {
                                if (File.Exists(imageFilepath))
                                    File.Delete(imageFilepath);

                                imageBytes.WriteBinaryFile(imageFilepath);
                                if (File.Exists(imageFilepath))
                                    isGoodImage = true;
                            }
                        }

                        if (!isGoodImage)
                            record.ImageFilenameOverride = null;
                    }
                    catch
                    {
                        record.ImageFilenameOverride = null;
                    }
                }
                else
                {
                    // no image to start with, see if might be able to
                    // make one now - maybe one of the missing product images is
                    // finally there

                    try
                    {
                        var imageFilenameOverride = MakeImageFilenameOverride(record);
                        var imageFilepath = Path.Combine(CollectionImageFolder, imageFilenameOverride);

                        bool isGoodImage = false;

                        var imageBytes = MakeImage(record, members);
                        if (imageBytes != null)
                        {
                            if (File.Exists(imageFilepath))
                                File.Delete(imageFilepath);
                
                            imageBytes.WriteBinaryFile(imageFilepath);
                            if (File.Exists(imageFilepath))
                                isGoodImage = true;
                        }

                        if (isGoodImage)
                            record.ImageFilenameOverride = imageFilenameOverride;
                    }
                    catch
                    {
                        record.ImageFilenameOverride = null; // actually, already null
                    }

                }
#endif
                DataContext.SubmitChanges();

                // self-pruning
                if (record.CountTotal < 2 || record.CountTotal == record.CountDiscontinued)
                {
                    // if completely discontinued, then schedule for delete if not already
                    if (DeletedCollections.Where(e => e.ProductCollectionID == record.ProductCollectionID).Count() == 0)
                        DeletedCollections.Add(record);
                }
                countCompleted++;
            }
        }

        protected virtual void UpdateCollectionRecord(ProductCollection rec, List<MemberProduct> members)
        {
            rec.CountTotal = members.Count;
            rec.CountDiscontinued = members.Where(e => e.IsDiscontinued).Count();
            rec.CountNoStock = members.Where(e => e.OutOfStock).Count();
            rec.Published = DeterminePublishedStatus(rec, members);

            if (members.Count() > 0)
                rec.CreatedOn = members.Select(e => e.CreatedOn).Min();

            // try to show price ranges for live products

            if (rec.CountTotal != rec.CountDiscontinued)
            {
                rec.HighPrice = members.Where(e => !e.IsDiscontinued).Max(e => e.OurPrice);
                rec.LowPrice = members.Where(e => !e.IsDiscontinued).Min(e => e.OurPrice);
            }
            else
            {
                rec.HighPrice = members.Max(e => e.OurPrice);
                rec.LowPrice = members.Min(e => e.OurPrice);
            }
        }

        protected virtual int DeterminePublishedStatus(ProductCollection rec, List<MemberProduct> members)
        {
            // since we're already pruning on all discontinued...presently, this rule will have no affect.

            // if all discontinued, then do not publish

            var allDiscontinued = members.All(e => e.IsDiscontinued);
            return (allDiscontinued || members.Count <= 1) ? 0 : 1;
        }

      
        protected virtual void PerformDeletes()
        {
            // doing one at a time on purpose - since generally shouldn't be deleting much if anything.
            // don't want to lock things up in SQL

            foreach(var record in DeletedCollections)
            {
                if (IsCancelled)
                    break;

                try
                {
                    DataContext.ProductCollections.DeleteOnSubmit(record);
                    DataContext.SubmitChanges();

                    if (!proccessImages)
                        continue;

                    if (!string.IsNullOrWhiteSpace(record.ImageFilenameOverride))
                    {
                        var imageFilepath = Path.Combine(CollectionImageFolder, record.ImageFilenameOverride);

                        if (File.Exists(imageFilepath))
                            File.Delete(imageFilepath);
                    }
                }
                catch (Exception Ex)
                {
                    var msg = string.Format("Exception in ProductCollectionUpdaterBase.Run / PerformDeletes() for {0}, ManufacturerID: {1}.", Store.StoreKey, ManufacturerID);
                    Debug.WriteLine(msg);
                    Debug.WriteLine(Ex.Message);
                    var ev2 = new WebsiteRequestErrorEvent(msg, this, WebsiteEventCode.UnhandledException, Ex);
                    ev2.Raise();
                }
            }
        }

        protected virtual void PerformInsertions()
        {
            // insert one at a time for now to not lock up SQL

            int displayCounter = 50;
            int countCompleted = 0;

            foreach (var key in NewCollectionKeys)
            {
                if (IsCancelled)
                    break;

                try
                {
                    if (displayCounter == 50)
                    {
                        DisplayStatusMessage(string.Format("{0}Inserting {1:N0}", DisplayPrefix, countCompleted));
                        displayCounter = 0;
                    }
                    else
                        displayCounter++;

                    var record = MakeNewCollectionRecord(key);

                    // if failed to create the record - bail on it
                    if (record == null)
                        continue;

                    // do not insert records where everything is discontinued
                    if (record.CountTotal < 2 || record.CountTotal == record.CountDiscontinued)
                        continue;

                    var members = GroupedProducts[key];

                    record.CreatedOn = members.Select(e => e.CreatedOn).Min(); // DateTime.Now;

                    DataContext.ProductCollections.InsertOnSubmit(record);
                    DataContext.SubmitChanges();
                    InsertedCollections.Add(record);

                    var imageFilenameOverride = MakeImageFilenameOverride(record);

#if DEBUG
                    // for debug only, even if not making real images, push out the
                    // filename it would have used

                    if (!proccessImages)
                    {
                        record.ImageFilenameOverride = imageFilenameOverride;
                        DataContext.SubmitChanges();
                    }
#endif
                    countCompleted++;

                    if (!proccessImages)
                        continue;

                    // now that we have a PkID, can do the image filename and be 
                    // guaranteed not to have duplicate names


                    // pass members in for cases where need to find one or more existing images
                    // to incorporate into the new image

                    var image = MakeImage(record, members);

                    if (image != null)
                    {
                        var imageFilepath = Path.Combine(CollectionImageFolder, imageFilenameOverride);

                        if (File.Exists(imageFilepath))
                            File.Delete(imageFilepath);

                        WriteBinaryFile(imageFilepath, image);
                        record.ImageFilenameOverride = imageFilenameOverride;
                        DataContext.SubmitChanges();
                    }
                }
                catch(Exception Ex)
                {
                    var msg = string.Format("Exception in ProductCollectionUpdaterBase.RunUpdate / PerformInsertions() for {0}, ManufacturerID: {1}.", Store.StoreKey, ManufacturerID);
                    Debug.WriteLine(msg);
                    Debug.WriteLine(Ex.Message);
                    var ev2 = new WebsiteRequestErrorEvent(msg, this, WebsiteEventCode.UnhandledException, Ex);
                    ev2.Raise();
                }
            }
        }


        private void WriteBinaryFile(string filepath, byte[] data)
        {
            using (var fs = new FileStream(filepath, FileMode.CreateNew, FileAccess.Write))
            {
                fs.Write(data, 0, data.Length);
            }
        }

        protected string DetermineProductGroupFromMembers(List<MemberProduct> members)
        {
            var productGroups = members.Where(e => e.ProductGroup != null).GroupBy(e => e.ProductGroup).ToDictionary(k => k.Key, v => v.Count());

            if (productGroups.Count() == 0)
                return "Fabric"; // default

            var bestMatch = productGroups.OrderByDescending(e => e.Value).First().Key; // take the group with the most members

            return bestMatch;
        }

        protected string GetMostUsedLabel(string[] names)
        {
            var dic = GetLabelCounts(names);

            if (dic.Count == 0)
                return null;

            var mostUsed = dic.OrderByDescending(e => e.Value).First().Key;
            return mostUsed;
        }

        protected Dictionary<string, int> GetLabelCounts(string[] names)
        {

            var dic = (from pl in DataContext.ProductLabels
                       where names.Contains(pl.Label)
                       join pm in DataContext.ProductManufacturers on pl.ProductID equals pm.ProductID
                       where pm.ManufacturerID == ManufacturerID
                       group pl by pl.Label into grp
                       select new
                       {
                           Label = grp.Key,
                           LabelCount = grp.Count(),
                       }).ToDictionary(k => k.Label, v => v.LabelCount);

            return dic;
        }


        protected virtual void PreInitialize()
        {
            // not generally expected to be overriden - but is so, must be called too

            // all important collections should be initialized to empty collection
            GroupedProducts = new Dictionary<string, List<MemberProduct>>();
            Collections = new List<ProductCollection>();
            MatchedCollections = new List<ProductCollection>();
            DeletedCollections = new List<ProductCollection>();
            NewCollectionKeys = new List<string>();
            InsertedCollections = new List<ProductCollection>();
        }

        /// <summary>
        /// Give others a chance to set things up.
        /// </summary>
        /// <remarks>
        /// DataContext is ready, SQL hits okay.
        /// </remarks>
        protected virtual void Initialize()
        {
            DisplayStatusMessage(DisplayPrefix);
        }

        /// <summary>
        /// Chance for others to do any local cleanup.
        /// </summary>
        /// <remarks>
        /// If overridden, must call base.
        /// </remarks>
        protected virtual void Cleanup()
        {
            GroupedProducts = null;
            Collections = null;
            MatchedCollections = null;
            DeletedCollections = null;
            NewCollectionKeys = null;
            InsertedCollections = null;
        }

        protected void DisplayStatusMessage(string msg)
        {
            if (reportStatusCallback == null)
                return;

            reportStatusCallback(msg);
        }

        protected virtual void RebuildImages(Func<ProductCollection, string> collectionKey)
        {
            // uses existing filename

            int displayCounter = 50;
            int countCompleted = 0;

            foreach(var record in Collections)
            {
                if (IsCancelled)
                    break;

                try
                {
                    if (displayCounter == 50)
                    {
                        DisplayStatusMessage(string.Format("{0}Rebuild Images {1:N0}", DisplayPrefix, countCompleted));
                        displayCounter = 0;
                    }
                    else
                        displayCounter++;

                    var members = GroupedProducts[collectionKey(record)];

#if !DEBUG
                    if (!proccessImages)
                        continue;
#endif

                    if (!string.IsNullOrWhiteSpace(record.ImageFilenameOverride))
                    {
                        // pass members in for cases where need to find one or more existing images
                        // to incorporate into the new image

                        var imageFilepath = Path.Combine(CollectionImageFolder, record.ImageFilenameOverride);

                        try
                        {
                            var image = MakeImage(record, members);

                            if (image != null)
                            {

                                if (File.Exists(imageFilepath))
                                    File.Delete(imageFilepath);

                                WriteBinaryFile(imageFilepath, image);
                            }
                            else
                            {
                                record.ImageFilenameOverride = null;
                                if (File.Exists(imageFilepath))
                                    File.Delete(imageFilepath);
                            }
                        }
                        catch
                        {
                            record.ImageFilenameOverride = null; // actually, already null
                            if (File.Exists(imageFilepath))
                                File.Delete(imageFilepath);
                        }
                    }
                    else
                    {
                        // no image to start with

                        try
                        {
                            var imageFilenameOverride = MakeImageFilenameOverride(record);
                            var imageFilepath = Path.Combine(CollectionImageFolder, imageFilenameOverride);

                            var imageBytes = MakeImage(record, members);
                            if (imageBytes != null)
                            {
                                if (File.Exists(imageFilepath))
                                    File.Delete(imageFilepath);

                                imageBytes.WriteBinaryFile(imageFilepath);
                                if (File.Exists(imageFilepath))
                                    record.ImageFilenameOverride = imageFilenameOverride;
                            }
                        }
                        catch
                        {
                            record.ImageFilenameOverride = null; // actually, already null
                        }
                    }

                    DataContext.SubmitChanges();

                }
                catch (Exception Ex)
                {
                    var msg = string.Format("Exception in ProductCollectionUpdaterBase.RebuildImages for {0}, ManufacturerID: {1}.", Store.StoreKey, ManufacturerID);
                    Debug.WriteLine(msg);
                    Debug.WriteLine(Ex.Message);
                    var ev2 = new WebsiteRequestErrorEvent(msg, this, WebsiteEventCode.UnhandledException, Ex);
                    ev2.Raise();

                    // if error, then clean up the record.
                    record.ImageFilenameOverride = null;
                    DataContext.SubmitChanges();
                }

                countCompleted++;

            }
        }

        /// <summary>
        /// This is the main call to do a complete pass through for the given vendor.
        /// </summary>
        /// <remarks>
        /// Must be idempotent. Never leave SQL in some bad state if cancelled. Should
        /// also be able to run multiple times on same object, even though we don't do that.
        /// Should also be idempotent if an exception is thrown.
        /// </remarks>
        /// <param name="cancelToken"></param>
        public virtual string RunUpdate(CancellationToken cancelToken, Action<string> reportStatusCallback = null)
        {
            try
            {
                this.cancelToken = cancelToken;
                this.reportStatusCallback = reportStatusCallback;

                PreInitialize();
                DataContext = new AspStoreDataContext(Store.ConnectionString);
                Initialize();

                
                if (IsCancelled)
                    return "Cancelled";

                Collections = FetchExistingCollections();

                if (IsCancelled)
                    return "Cancelled";

                DisplayStatusMessage(DisplayPrefix + "Discovery");
                GroupedProducts = FetchGroupedProducts();

                if (IsCancelled)
                    return "Cancelled";

                DisplayStatusMessage(DisplayPrefix + "Matching");
                PerformMatchups();

                if (IsCancelled)
                    return "Cancelled";

                // we now have fully populated collections for what matched up, what's new, what's to be deleted

                DisplayStatusMessage(DisplayPrefix + "Updating");
                PerformUpdates();

                if (IsCancelled)
                    return "Cancelled";

                DisplayStatusMessage(DisplayPrefix + "Deleting");
                PerformDeletes();

                if (IsCancelled)
                    return "Cancelled";

                DisplayStatusMessage(DisplayPrefix + "Inserting");
                PerformInsertions();

                if (IsCancelled)
                    return "Cancelled";

                return "Success";
            }
            catch(Exception Ex)
            {
                var msg = string.Format("Exception in ProductCollectionUpdaterBase.RunUpdate for {0}, ManufacturerID: {1}.", Store.StoreKey, ManufacturerID);
                Debug.WriteLine(msg);
                Debug.WriteLine(Ex.Message);
                var ev2 = new WebsiteRequestErrorEvent(msg, this, WebsiteEventCode.UnhandledException, Ex);
                ev2.Raise();

                return string.Format("Exception for manufacturer {0}.", ManufacturerID);
            }
            finally
            {
                Cleanup();
                DataContext.Connection.Close();
                DataContext = null;
            }
        }

        public virtual string RunRebuildImages(CancellationToken cancelToken, Action<string> reportStatusCallback = null)
        {
            try
            {
                this.cancelToken = cancelToken;
                this.reportStatusCallback = reportStatusCallback;

                PreInitialize();
                DataContext = new AspStoreDataContext(Store.ConnectionString);
                Initialize();


                if (IsCancelled)
                    return "Cancelled";

                Collections = FetchExistingCollections();

                if (IsCancelled)
                    return "Cancelled";

                DisplayStatusMessage(DisplayPrefix + "Discovery");
                GroupedProducts = FetchGroupedProducts();

                if (IsCancelled)
                    return "Cancelled";

                DisplayStatusMessage(DisplayPrefix + "Rebuild Images");
                RebuildImages();

                return "Success";
            }
            catch (Exception Ex)
            {
                var msg = string.Format("Exception in ProductCollectionUpdaterBase.RunRebuildImages for {0}, ManufacturerID: {1}.", Store.StoreKey, ManufacturerID);
                Debug.WriteLine(msg);
                Debug.WriteLine(Ex.Message);
                var ev2 = new WebsiteRequestErrorEvent(msg, this, WebsiteEventCode.UnhandledException, Ex);
                ev2.Raise();

                return string.Format("Exception for manufacturer {0}.", ManufacturerID);
            }
            finally
            {
                Cleanup();
                DataContext.Connection.Close();
                DataContext = null;
            }
        }



        private byte[] ResizeImage(byte[] originalContent, int newDimension)
        {
            // the new dimension is used for both width and height

            // we want to return an image which uses the full dimension in each direction - so a non-square image will still
            // return a square
#if true
            // the target KB size passed in here is not important since want to keep quality high, and compression is done later on the entire composed image
            byte[] resizedImage = ExtensionMethods.MakeOptimizedSquareImage(originalContent, newDimension, new int[] { 98 }, new int[] { 1000 });
            return resizedImage;
#else

            int? Width;
            int? Height;

            GetImageDimensions(originalContent, out Width, out Height);

            var imgElem = new Neodynamic.WebControls.ImageDraw.ImageElement { Source = Neodynamic.WebControls.ImageDraw.ImageSource.Binary, SourceBinary = originalContent, PreserveMetaData = false };

            var imgDraw = new Neodynamic.WebControls.ImageDraw.ImageDraw();
            imgDraw.Canvas.AutoSize = true;

            if (Width != Height)
            {
                var smallestDimension = Math.Min(Width.GetValueOrDefault(), Height.GetValueOrDefault());

                var crop = new Neodynamic.WebControls.ImageDraw.Crop
                {
                    Width = smallestDimension,
                    Height = smallestDimension,
                };
                imgElem.Actions.Add(crop);

            }

            //Create an instance of Resize class
            var resize = new Neodynamic.WebControls.ImageDraw.Resize
            {
                Width = newDimension,
                LockAspectRatio = Neodynamic.WebControls.ImageDraw.LockAspectRatio.WidthBased
            };
            imgElem.Actions.Add(resize);


            imgDraw.Elements.Add(imgElem);
            imgDraw.ImageFormat = Neodynamic.WebControls.ImageDraw.ImageDrawFormat.Png;

            var resizedImage = imgDraw.GetOutputImageBinary();
            return resizedImage;
#endif
        }


        private void GetImageDimensions(byte[] ContentData, out int? Width, out int? Height)
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

    }
}