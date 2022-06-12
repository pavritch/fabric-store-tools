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
using Website.Entities;

namespace Website
{
    public class CollectionProductCollectionUpdater : ProductCollectionUpdaterBase
    {

        private string mostUsedLabel = null;

        public CollectionProductCollectionUpdater(IWebStore store, int manufacturerID)
            : base(store, manufacturerID)
        {

        }


        protected override List<ProductCollection> FetchExistingCollections()
        {
            return FetchExistingCollections(2); // 2-collections
        }

        /// <summary>
        /// Creates a dic with key as collection name, with list of member products.
        /// </summary>
        /// <returns></returns>
        protected override Dictionary<string, List<MemberProduct>> FetchGroupedProducts()
        {

#if true
            var supportedGroups = Store.SupportedProductGroups.Select(e => e.DescriptionAttr()).ToList();
            var dic = new Dictionary<string, List<MemberProduct>>();

            try
            { 
                var labelValues = (from pl in DataContext.ProductLabels where pl.Label == mostUsedLabel
                            join pm in DataContext.ProductManufacturers on pl.ProductID equals pm.ProductID where pm.ManufacturerID == ManufacturerID
                            join p in DataContext.Products on pm.ProductID equals p.ProductID where supportedGroups.Contains(p.ProductGroup)
                            select pl.Value).Distinct().ToList();

                int displayCounter = 50;
                int countCompleted = 0;

                foreach(var labelValue in labelValues)
                {
                    if (IsCancelled)
                        break;

                    if (displayCounter == 50)
                    {
                        DisplayStatusMessage(string.Format("{0}Discovery {1:N0}", DisplayPrefix, countCompleted));
                        displayCounter = 0;
                    }
                    else
                        displayCounter++;


                    var products = (from pl in DataContext.ProductLabels where pl.Label == mostUsedLabel && pl.Value == labelValue
                            join pm in DataContext.ProductManufacturers on pl.ProductID equals pm.ProductID where pm.ManufacturerID == ManufacturerID
                            join p in DataContext.Products on pm.ProductID equals p.ProductID where p.Published == 1 && p.Deleted == 0 && supportedGroups.Contains(p.ProductGroup)
                            join pv in DataContext.ProductVariants on pm.ProductID equals pv.ProductID where pv.IsDefault == 1
                            select new MemberProduct
                                {
                                    ProductID = p.ProductID,
                                    IsDiscontinued = p.ShowBuyButton == 0,
                                    ProductGroup = p.ProductGroup,
                                    OurPrice = pv.Price,
                                    OutOfStock = pv.Inventory == 0,
                                    ImageFilenameOverride = p.ImageFilenameOverride,
                                    CreatedOn = p.CreatedOn
                                }).ToList();

                        countCompleted++;

                        // must have at least 2 members to be considered here

                        if (products.Count() < 2)
                            continue;

                        var trimKey = labelValue.ToUpper().Trim();

                        if (trimKey.Length < 2)
                            continue;

                        dic[trimKey] = products;
                }

            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.ToString());
                throw;
            }

            return dic;

#else
            // works - but slow

            var dic = (from pl in DataContext.ProductLabels where pl.Label == mostUsedLabel
                        join p in DataContext.Products on pl.ProductID equals p.ProductID where p.Published == 1
                        join pm in DataContext.ProductManufacturers on p.ProductID equals pm.ProductID
                        where pm.ManufacturerID == ManufacturerID
                        join pv in DataContext.ProductVariants on pm.ProductID equals pv.ProductID
                        where pv.IsDefault == 1
                        group new MemberProduct
                        {
                            ProductID = p.ProductID,
                            IsDiscontinued = p.ShowBuyButton == 0,
                            ProductGroup = p.ProductGroup,
                            OurPrice = pv.Price,
                            OutOfStock = pv.Inventory == 0,
                            ImageFilenameOverride = p.ImageFilenameOverride,
                        }
                        by pl.Value into grp
                        select new
                        {
                            Key = grp.Key,
                            Items = grp.ToList(),
                        }).ToDictionary(k => k.Key.ToUpper(), v => v.Items);


            return dic;
#endif           
        }

        protected override string DisplayPrefix
        {
            get
            {
                return string.Format("{0} collections...", CleanManufacturerName);
            }
        }

        protected override void Initialize()
        {
            base.Initialize();
            mostUsedLabel = "Collection"; 
        }

        private string GetPureName(string key, List<MemberProduct> members)
        {
            // key is name of collection, but is upper case - so need to go get the mixed case name

            var memberCollection = members.Select(e => e.ProductID).Take(100).ToList();
            var names = (from pl in DataContext.ProductLabels
                         where pl.Label == "Collection" && pl.Value == key && memberCollection.Contains(pl.ProductID)
                         select pl.Value).Distinct().ToList();

            if (names.Count() == 0)
                return key; // will be upper case, but can't be choosey here

            // save the first name, it will be mixed case, but sometimes might contain a comma if in more than one collection

            var firstName = names.First();

            // strip out names with commas, if none left, return what we found above.

            names.RemoveAll(e => e.ContainsIgnoreCase(",") || e.ContainsIgnoreCase(";"));

            if (names.Count == 0)
                return firstName.RemovePattern(@"(,|;).*").Trim();

            return names.First().Trim();
        }

        private string MakeName(string pureName, string productGroup)
        {
            var name = string.Format("{1} {2} Collection {0}", pureName, CleanManufacturerName, productGroup);
            return name;
        }

        protected override ProductCollection MakeNewCollectionRecord(string key)
        {
            var rec = new ProductCollection();
            var members = GroupedProducts[key];

            // pure name is before adding fluff around the text 
            var pureName = GetPureName(key, members);

            // filter out anythign that seems wrong
            if (string.IsNullOrWhiteSpace(pureName) || pureName.Length < 2)
                return null;

            rec.Kind = 2; // 2-collections
            rec.ProductGroup = DetermineProductGroupFromMembers(members);
            rec.ManufacturerID = ManufacturerID;
            rec.Name = MakeName(pureName, rec.ProductGroup);
            rec.ShortName = pureName;
            rec.PropName = "COLLECTION";
            rec.PropValue = key; 
            rec.PatternCorrelator = null;
            rec.SEName = rec.Name.MakeSafeSEName().ToLower();
            if (Store.StoreKey == StoreKeys.InsideRugs)
            {
                rec.SETitle = string.Format("{0} | Area Rug Superstore", rec.Name);
                rec.SEDescription = string.Format("Lowest prices on {0} {1} rugs. Shop thousands of area rugs. Fast delivery. Free shipping!", CleanManufacturerName, pureName);
            }
            else
            {
                rec.SETitle = string.Format("{0} | {1} Superstore", rec.Name, rec.ProductGroup == "Wallcovering" ? "Wallpaper" : "Fabric");
                rec.SEDescription = string.Format("Lowest prices on {1} {2} collection {0}. Shop thousands of patterns. Fast delivery. Free shipping!", pureName, CleanManufacturerName, rec.ProductGroup.ToLower());
            }
            rec.ImageFilenameOverride = null; // filled in after have PkID by caller

            // fill in the statistics
            UpdateCollectionRecord(rec, members);

            return rec;
        }

        protected override string MakeImageFilenameOverride(ProductCollection rec)
        {
            var filename = string.Format("{0}-{1}-{2}.jpg", "collection", rec.ProductCollectionID, rec.SEName);
            return filename.ToLower();
        }


        //protected override byte[] MakeImage(ProductCollection rec, List<MemberProduct> members)
        //{
        //    return null;
        //}


        protected override void PerformUpdates()
        {
            PerformUpdates(e =>
                {
                    return e.PropValue;
                });
        }

        protected override void PerformMatchups()
        {
            // the core logic is common provided a way to dynamically return a key value to
            // use for comparison

            PerformMatchups(e =>
                {
                    return e.PropValue;
                });
        }

        protected override void RebuildImages()
        {
            RebuildImages(e =>
            {
                return e.PropValue;
            });
        }
    }
}