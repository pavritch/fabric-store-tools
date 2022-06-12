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
    public class PatternProductCollectionUpdater : ProductCollectionUpdaterBase
    {
        public PatternProductCollectionUpdater(IWebStore store, int manufacturerID)
            : base(store, manufacturerID)
        {

        }

        protected override List<ProductCollection> FetchExistingCollections()
        {
            return FetchExistingCollections(1); // 1-patterns
        }

        protected override string DisplayPrefix
        {
            get
            {
                return string.Format("{0} patterns...", CleanManufacturerName);
            }
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// Creates a dictionary with uppercase pattern correlator as key, and list of member products.
        /// </summary>
        /// <returns></returns>
        protected override Dictionary<string, List<MemberProduct>> FetchGroupedProducts()
        {
#if true
            var supportedGroups = Store.SupportedProductGroups.Select(e => e.DescriptionAttr()).ToList();
            var dic = new Dictionary<string, List<MemberProduct>>();

            try
            {

                var distinctPatterns = (from p in DataContext.Products where p.Published == 1 && p.Deleted == 0 && supportedGroups.Contains(p.ProductGroup)
                                        join pm in DataContext.ProductManufacturers on p.ProductID equals pm.ProductID where pm.ManufacturerID == ManufacturerID
                                        select p.ManufacturerPartNumber).Distinct().ToList();
                
                int displayCounter = 50;
                int countCompleted = 0;

                foreach (var pattern in distinctPatterns)
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


                    var products = (from p in DataContext.Products
                                    where p.ManufacturerPartNumber == pattern && p.Published == 1 && p.Deleted == 0 && supportedGroups.Contains(p.ProductGroup)
                                    join pv in DataContext.ProductVariants on p.ProductID equals pv.ProductID
                                    where pv.IsDefault == 1
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

                    var trimKey = pattern.ToUpper().Trim();

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
            // this works - but takes a very long time to execute

            var dic = (from p in DataContext.Products
                where p.Published == 1
                join pm in DataContext.ProductManufacturers on p.ProductID equals pm.ProductID where pm.ManufacturerID == ManufacturerID
                join pv in DataContext.ProductVariants on pm.ProductID equals pv.ProductID where pv.IsDefault == 1
                group new MemberProduct
                {
                    ProductID = p.ProductID,
                    IsDiscontinued = p.ShowBuyButton == 0,
                    ProductGroup = p.ProductGroup,
                    OurPrice = pv.Price,
                    OutOfStock = pv.Inventory == 0,
                    ImageFilenameOverride = p.ImageFilenameOverride,
                }
                by p.ManufacturerPartNumber into grp
                select new
                {
                    Pattern = grp.Key,
                    Items = grp.ToList(),
                }).ToDictionary(k => k.Pattern.ToUpper(), v => v.Items);

            return dic;
#endif
        }

        private string GetPureName(string pattern)
        {
            // the input key is the pattern correlator, which is p.MPN, which possibly could be a GUID,
            // and maybe the target grouping does not have a formal name for the pattern.

            var allowedLabels = new string[] { "Pattern Name", "Pattern", "Pattern Number" };

            var names = (from pl in DataContext.ProductLabels
                         where allowedLabels.Contains(pl.Label)
                         join p in DataContext.Products on pl.ProductID equals p.ProductID
                         where p.ManufacturerPartNumber == pattern
                         group pl by pl.Value into grp
                         select new
                         {
                             Key = grp.Key,
                             ItemCount = grp.Count()
                         }).ToList();

            if (names.Count() == 0)
            {
                // could be a GUID - which we don't want for an actual name
                if (pattern.Length == 36 && pattern.Where(e => e == '-').Count() == 4)
                {
                    // is guid, conjour up a numeric value

                    var rnd = new Random(pattern.ToSeed());
                    var selectedName = string.Format("P{0}", rnd.Next(10000, 99999));

                    // it technically doesn't matter if a duplicate here, because urls in the end always
                    /// have the collectionID, and the true correlator is always used for membership.
                    /// So this name here is really more only of a vanity name.
                    /// 
                    return selectedName;
                }

                return pattern;
            }

            Func<string, int> countLetters = (s) =>
            {
                int count = 0;
                foreach (var c in s)
                    if (Char.IsLetter(c))
                        count++;
                return count;
            };

            // take the one with the most letters, figuring we'd prefer to use text names over numbers where possible

            string bestName = names.OrderByDescending(e => countLetters(e.Key)).Select(e => e.Key).First();

            return bestName.Trim();
        }

        private string MakeName(string pureName, string productGroup)
        {
            var name = string.Format("{1} {2} Pattern {0}", pureName, CleanManufacturerName, productGroup);
            return name;
        }

        protected override ProductCollection MakeNewCollectionRecord(string key)
        {
            // the input key is the pattern correlator, which is p.MPN, which possibly could be a GUID,
            // and maybe the target grouping does not have a formal name for the pattern.

            var rec = new ProductCollection();
            var members = GroupedProducts[key];

            // pure name is before adding fluff around the text 
            var pureName = GetPureName(key);

            // filter out anythign that seems wrong
            if (string.IsNullOrWhiteSpace(pureName) || pureName.Length < 2)
                return null;

            rec.Kind = 1; // 1-patterns
            rec.ProductGroup = DetermineProductGroupFromMembers(members);
            rec.ManufacturerID = ManufacturerID;
            rec.Name = MakeName(pureName, rec.ProductGroup);
            rec.ShortName = pureName;
            rec.PropName = null;
            rec.PropValue = null;
            rec.PatternCorrelator = key;
            rec.SEName = rec.Name.MakeSafeSEName().ToLower();

            if (Store.StoreKey == StoreKeys.InsideRugs)
            {
                rec.SETitle = string.Format("{0} | Area Rug Superstore", rec.Name);
                rec.SEDescription = string.Format("Lowest prices on {0} {1} rugs. Shop thousands of area rugs. Fast delivery. Free shipping!", CleanManufacturerName, pureName);
            }
            else
            {
                rec.SETitle = string.Format("{0} | {1} Superstore", rec.Name, rec.ProductGroup == "Wallcovering" ? "Wallpaper" : "Fabric");
                rec.SEDescription = string.Format("Lowest prices on {1} {2} pattern {0}. Shop thousands of patterns. Fast delivery. Free shipping!", pureName, CleanManufacturerName, rec.ProductGroup.ToLower());
            }
            rec.ImageFilenameOverride = null; // filled in after have PkID by caller

            // fill in the statistics
            UpdateCollectionRecord(rec, members);

            return rec;
        }

        protected override string MakeImageFilenameOverride(ProductCollection rec)
        {
            var filename = string.Format("{0}-{1}-{2}.jpg", "pattern", rec.ProductCollectionID, rec.SEName);
            return filename.ToLower();
        }


        //protected override byte[] MakeImage(ProductCollection rec, List<MemberProduct> members)
        //{
        //    return null; // TODO:
        //}

        protected override void PerformUpdates()
        {
            PerformUpdates(e =>
            {
                return e.PatternCorrelator;
            });
        }

        protected override void PerformMatchups()
        {
            // the core logic is common provided a way to dynamically return a key value to
            // use for comparison

            PerformMatchups(e =>
            {
                return e.PatternCorrelator;
            });
        }


        protected override void RebuildImages()
        {
            RebuildImages(e =>
            {
                return e.PatternCorrelator;
            });
        }

    }
}