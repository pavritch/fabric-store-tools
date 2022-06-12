using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Website.Entities;
using Gen4.Util.Misc;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Website
{
    public class ProductCrossLinksManager<TProduct> where TProduct : class, IUpdatableProduct, new()
    {
        public enum ProductGroup
        {
            Unknown,
            Fabric,
            Wallcovering,
            Trim,
        }

        /// <summary>
        /// Queue item for a single product. Can have 1-N links to be created, but must be for same target.
        /// </summary>
        /// <remarks>
        /// Represents one unit of work in the SQL action block queue. The goal of this object is to bunch
        /// up the links needed TO the same product, so that we can be more efficient in the information
        /// gathering phase of creating the anchor text and writing out to SQL.
        /// </remarks>
        public class ProductCrossLinks
        {
            public int ToProductID { get; set; }
            public int PreviousLinkCount { get; set; }

            public HashSet<int> FromProducts { get; set; }

            public ProductCrossLinks(int toProductID, int previousLinkCount)
            {
                FromProducts = new HashSet<int>();
                this.PreviousLinkCount = previousLinkCount;
                this.ToProductID = toProductID;
            }

            public void AddFrom(int ProductID)
            {
                FromProducts.Add(ProductID);
            }
            
        }

        /// <summary>
        /// Holds whatever information we need to track for a participating product.
        /// </summary>
        /// <remarks>
        /// Fast access to these records is facilitated by a number of associated lookup dictionaries.
        /// </remarks>
        public class ProductRecord
        {
            public int ProductID { get; set; }
            public int ManufacturerID { get; set; }
            public ProductGroup Group { get; set; }
            public bool IsDiscontinued { get; set; }

            public ProductRecord(int ProductID, int ManufacturerID, string productGroup, int ShowBuyButton)
            {
                this.ProductID = ProductID;
                this.ManufacturerID = ManufacturerID;
                this.IsDiscontinued = ShowBuyButton == 0;

                ProductGroup grp;

                if (!string.IsNullOrEmpty(productGroup) && LibEnum.TryParse<ProductGroup>(productGroup, true, out grp))
                    this.Group = grp;
                else
                    this.Group = ProductGroup.Unknown;
            }
        }

        /// <summary>
        /// Number of links to strive to achieve for each product page.
        /// </summary>
        /// <remarks>
        /// Set by Run().
        /// </remarks>
        private int RequiredInboundLinks { get; set; }

        private int MaxOutboundLinks { get; set; }

        private ProductCrossLinks CurrentProductCrossLinks { get; set; }

        private ActionBlock<ProductCrossLinks> sqlActionBlock;


        /// <summary>
        /// Need a single rnd generator for each run.
        /// </summary>
        private Random RandomNumber { get; set; }

        /// <summary>
        /// The store which this is being run under. Filled by ctor.
        /// </summary>
        /// <remarks>
        /// Used primarily to get the connection string.
        /// </remarks>
        private IWebStore Store { get; set; }

        private AspStoreDataContext DataContext {get; set;}


        /// <summary>
        /// Collection of all participating products
        /// </summary>
        private Dictionary<int, ProductRecord> ProductRecords { get; set; }

        // fast lookup collections into ProductRecords

        private Dictionary<string, HashSet<int>> ProductsByManufacturer { get; set; }

        private List<int> AllManufacturersProducts { get; set; }

        /// <summary>
        /// Colleciton of outbound links for a productID - existing plus just created.
        /// </summary>
        /// <remarks>
        /// Starts out as same as SQL, but supplemented along the way
        /// with newly added links - so remains an accurate picture
        /// throughout the process.
        /// </remarks>
        private Dictionary<int, List<int>> OutboundLinks {get; set;}

        /// <summary>
        /// Colleciton of inbound links for a productID - existing plus just created.
        /// </summary>
        /// <remarks>
        /// Starts out as same as SQL, but supplemented along the way
        /// with newly added links - so remains an accurate picture
        /// throughout the process.
        /// </remarks>
        private Dictionary<int, List<int>> InboundLinks { get; set; }

        /// <summary>
        /// List composed at start of run which holds the set of products found
        /// to be in need to getting links.
        /// </summary>
        private List<int> ProductsNeedingLinks;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="store"></param>
        public ProductCrossLinksManager(IWebStore store)
        {
            this.Store = store;
        }

        /// <summary>
        /// Ensure that the SQL table is cleansed of anything that no longer exists.
        /// </summary>
        /// <remarks>
        /// Remove any row that has a ProductID or TargetProductID that is missing.
        /// </remarks>
        private void PruneDeadLinks()
        {
            // discontinued products not allowed to get inbound links - prune them out.

            // same prune also done by InsideFabricCleanup.sql
            DataContext.ExecuteCommand("delete [dbo].[ProductCrossLinks] where ProductID not in (select ProductID from Product)");
            DataContext.ExecuteCommand("delete [dbo].[ProductCrossLinks] where TargetProductID not in (select ProductID from Product where ShowBuyButton=1)");
        }


        /// <summary>
        /// Gather basic information on each participating product.
        /// </summary>
        private void PopulateProductRecords()
        {
            ProductRecords = (from p in DataContext.Products 
                             join m in DataContext.ProductManufacturers on p.ProductID equals m.ProductID 
                             select new ProductRecord(p.ProductID, m.ManufacturerID, p.ProductGroup, p.ShowBuyButton)).ToDictionary(k => k.ProductID, v => v);


            // create a fast lookup by manufacturer

            // a product may only be in this dic once. The products are broken up into groups indexed
            // by a key composed of the manufacturerID and the product group

            ProductsByManufacturer = new Dictionary<string, HashSet<int>>();

            foreach (var product in ProductRecords.Values.ToList())
            {
                HashSet<int> productsForManufacturerProductGroup;

                var key = MakeManufacturerProductSetKey(product.ProductID);

                if (!ProductsByManufacturer.TryGetValue(key, out productsForManufacturerProductGroup))
                {
                    productsForManufacturerProductGroup = new HashSet<int>();
                    ProductsByManufacturer.Add(key, productsForManufacturerProductGroup);
                }
                productsForManufacturerProductGroup.Add(product.ProductID);
            }

            // AllManufacturersProducts - used for a second pass with broader scope to seek out candidates

            AllManufacturersProducts = new List<int>();
            foreach (var list in ProductsByManufacturer.Values)
                AllManufacturersProducts.AddRange(list);
            // mix'em up
            for(int i=0; i < 3; i++)
                AllManufacturersProducts.Shuffle();
        }


        private void PopulateOutboundLinks()
        {
            // lookup by productID and get a list of productID who it links to.

            var col = (from e in DataContext.ProductCrossLinks
                       group e by e.ProductID into g
                       select new
                       {
                           ProductID = g.Key,
                           Targets = g.Select(t => t.TargetProductID).ToList(),
                       }).ToDictionary(k => k.ProductID, v => v.Targets);

            OutboundLinks = col;
        }


        private void PopulateInboundLinks()
        {
            // lookup of productID to get list of pages which point to it

            var col = (from e in DataContext.ProductCrossLinks
                       group e by e.TargetProductID into g
                       select new
                       {
                           ProductID = g.Key,
                           Sources = g.Select(s => s.ProductID).ToList(),
                       }).ToDictionary(k => k.ProductID, v => v.Sources);

            InboundLinks = col;
        }

        /// <summary>
        /// If maxed out, no need to keep around and waste time as a candidate.
        /// </summary>
        private void PruneMaxedOutCandidates()
        {
            foreach (var productSet in ProductsByManufacturer.Values)
            {
                productSet.RemoveWhere(e => OutboundLinkCount(e) == MaxOutboundLinks);
            }
        }

        /// <summary>
        /// Figure out which products are short the required number of links.
        /// </summary>
        /// <remarks>
        /// This becomes the main work list.
        /// </remarks>
        private void DetermineProductsNeedingLinks()
        {
            ProductsNeedingLinks = new List<int>();

            foreach (var product in ProductRecords.Values)
            {
                // discontinued products are not allowed to get inbound links,
                // but the can offer outbound links

                if (product.IsDiscontinued)
                    continue;

                if (LinksNeeded(product.ProductID) > 0)
                    ProductsNeedingLinks.Add(product.ProductID);
            }

            // mix'em up just to add some spice

            ProductsNeedingLinks.Shuffle();

#if DEBUG
            //ProductsNeedingLinks = ProductsNeedingLinks.Take(1000).ToList();
#endif

        }

        private void Initialize(CancellationToken cancelToken)
        {
            RandomNumber = new Random();
            CurrentProductCrossLinks = null;

            // simple clean up before starting -just in case
            PruneDeadLinks();

            if (cancelToken.IsCancellationRequested)
                return;

            // universe of participating products

            PopulateProductRecords();

            if (cancelToken.IsCancellationRequested)
                return;

            // reference lookup dictionaries for who is linking to who
            // in both directions
            PopulateOutboundLinks();

            if (cancelToken.IsCancellationRequested)
                return;

            PopulateInboundLinks();

            if (cancelToken.IsCancellationRequested)
                return;

            // flush out any that simply cannot be picked since
            // already have max number of links
            PruneMaxedOutCandidates();

            if (cancelToken.IsCancellationRequested)
                return;

            // figure out what needs to be done
            DetermineProductsNeedingLinks();
        }

        /// <summary>
        /// Release all meaningful resources used during a single run.
        /// </summary>
        private void Cleanup()
        {
            OutboundLinks = null;
            InboundLinks = null;
            ProductsNeedingLinks = null;
            ProductRecords = null;
            ProductsByManufacturer = null;
            AllManufacturersProducts = null;
            CurrentProductCrossLinks = null;

            if (DataContext != null)
                DataContext.Dispose();
        }

        /// <summary>
        /// Figure out how many links are needed for a specific ProductID.
        /// </summary>
        /// <param name="productID"></param>
        /// <returns></returns>
        private int LinksNeeded(int productID)
        {
            List<int> links = null;

            if (InboundLinks.TryGetValue(productID, out links))
            {
                if (links.Count() < RequiredInboundLinks)
                    return RequiredInboundLinks - links.Count();

                return 0;
            }

            return RequiredInboundLinks;

        }

        private bool IsExistingLink(int fromProductID, int toProductID)
        {
            List<int> links = null;

            if (OutboundLinks.TryGetValue(fromProductID, out links))
                return links.Contains(toProductID);

            return false;
        }


        private int OutboundLinkCount(int productID)
        {
            List<int> links = null;

            if (OutboundLinks.TryGetValue(productID, out links))
                return links.Count();

            return 0;
        }


        /// <summary>
        /// Create the needed number of links for a single product.
        /// </summary>
        /// <remarks>
        /// May or may not already have links. In either case, add as many
        /// as needed to reach the required number.
        /// </remarks>
        /// <param name="productID"></param>
        private int ProcessInboundLinksForSingleProduct(int productID, bool RequireCloseLinkingCandidates)
        {
            int countLinksCreated = 0;

            var linksNeeded = LinksNeeded(productID);

            // use same list of candidates for all grabs for this product

            List<int> candidates;

            if (RequireCloseLinkingCandidates)
                candidates = FindCloseLinkingCandidates(productID);
            else
                candidates = FindBroadLinkingCandidates(productID);

            // abort if not finding much to pick from - there will always be another day

            if (candidates.Count() <= 10)
                return 0;

            for (int i = 0; i < linksNeeded; i++)
            {
                int fromProductID = -1;
                for (var attempt = 0; attempt < 10; attempt++)
                {
                    var pickIndex = RandomNumber.Next(0, candidates.Count());
                    var pickedProductID = candidates[pickIndex];

                    // make sure still like this one, set fromProduct and break when good

                    // make sure the proposed from product does not already link to this one,
                    // which is actually possible given that we don't prune the list of candidates
                    // each round through -- since that would take more effort than letting it
                    // fail and retry if needed.

                    if (IsExistingLink(pickedProductID, productID))
                        continue;

                    // make sure this product wanting a link does not already link to the
                    // proposed from product

                    if (IsExistingLink(productID, pickedProductID))
                        continue;

                    fromProductID = pickedProductID;
                    break;
                }

                // if didn't get something - bail for this product

                if (fromProductID == -1)
                    return countLinksCreated;

                CreateCrossLink(fromProductID, productID);
                countLinksCreated++;

                // if the field of candidates is small, then remove the present pick
                // so won't collide again on next round. We do not do this for big lists
                // since would not be efficient - rather collide and retry

                //if (candidates.Count() <= 100)
                //    candidates.Remove(fromProductID);
            }

            return countLinksCreated;
        }

        
        /// <summary>
        /// Create a list of possible good candidates to link to the given product. Close search.
        /// </summary>
        /// <param name="ProductID"></param>
        /// <returns></returns>
        private List<int> FindCloseLinkingCandidates(int ProductID)
        {
            // try for:
            // same manufacturer
            // same product group
            // does not already have N links total
            // does not already link to this product.
            // this product does not already link to them
            
            ProductRecord targetProduct;

            if (!ProductRecords.TryGetValue(ProductID, out targetProduct))
                return new List<int>();

            var finalCandidates = new List<int>();

            // find ones from same manufacturer and same product group
            var manufacturerSetKey = MakeManufacturerProductSetKey(targetProduct.ManufacturerID, targetProduct.Group);
            if (manufacturerSetKey == null)
                return new List<int>();

            var startingCandidates = ProductsForManufacturer(manufacturerSetKey).ToList();

            startingCandidates.Shuffle();

            // for performance reasons, we don't need to qualify the entire universe of possibilities - we really
            // only need a decent working set

            int skipThis = 0;
            int takeThis = 50;
            while (skipThis < startingCandidates.Count && finalCandidates.Count < 500)
            {
                var products = startingCandidates
                    .Skip(skipThis)
                    .Take(takeThis)
                    .Where(e => OutboundLinkCount(e) < MaxOutboundLinks && !IsExistingLink(e, ProductID) && !IsExistingLink(ProductID, e))
                    .ToList();

                finalCandidates.AddRange(products);
                skipThis += takeThis;
            }

            return finalCandidates;
        }


        /// <summary>
        /// Create a list of possible good candidates to link to the given product. Broad search.
        /// </summary>
        /// <param name="ProductID"></param>
        /// <returns></returns>
        private List<int> FindBroadLinkingCandidates(int ProductID)
        {
            // try for:
            // manufacturer/group = any (this is the key diffrence between broad and close)
            // does not already have N links total
            // does not already link to this product.
            // this product does not already link to them

            ProductRecord targetProduct;

            if (!ProductRecords.TryGetValue(ProductID, out targetProduct))
                return new List<int>();

            var finalCandidates = new List<int>();

            var startingCandidates = CandidatesFromAllManufacturers();

            startingCandidates.Shuffle();

            // for performance reasons, we don't need to qualify the entire universe of possibilities - we really
            // only need a decent working set

            int skipThis = 0;
            int takeThis = 50;
            while (skipThis < startingCandidates.Count && finalCandidates.Count < 500)
            {
                var products = startingCandidates
                    .Skip(skipThis)
                    .Take(takeThis)
                    .Where(e => OutboundLinkCount(e) < MaxOutboundLinks && !IsExistingLink(e, ProductID) && !IsExistingLink(ProductID, e))
                    .ToList();

                finalCandidates.AddRange(products);
                skipThis += takeThis;
            }

            return finalCandidates;
        }


        /// <summary>
        /// Locate the product group subset for the named manufacturer.
        /// </summary>
        /// <remarks>
        /// Key is composed of manufacturerID and group.
        /// See MakeManufacturerProductSetKey()
        /// </remarks>
        /// <param name="key"></param>
        /// <returns></returns>
        HashSet<int> ProductsForManufacturer(string key)
        {
            HashSet<int> list;
            if (ProductsByManufacturer.TryGetValue(key, out list))
                return list;

            return new HashSet<int>();
        }

        int requestCount = 0;
        List<int> CandidatesFromAllManufacturers()
        {
            var countProducts = AllManufacturersProducts.Count();
            var skipThis = RandomNumber.Next(0, countProducts/2);
            var takeThis = RandomNumber.Next(countProducts/20, countProducts/15);
            var list = AllManufacturersProducts.Skip(skipThis).Take(takeThis).ToList();

            // reshuffle every so often

            requestCount++;
            if (requestCount == 1000)
            {
                AllManufacturersProducts.Shuffle();
                requestCount = 0;
            }

            return list;
        }

        /// <summary>
        /// Come up with some anchor text to use for a link to this product.
        /// </summary>
        /// <remarks>
        /// Intended to run from queue. Must be thread safe.
        /// </remarks>
        /// <param name="ProductID"></param>
        /// <returns></returns>
        private string PickAnchorText(int ProductID)
        {
            // TODO:

            // don't want to always use the traditional product name - as that is way too long tail
            // most of the time. Certainly use it for some picks, but for others, creative choose
            // text that includes just the pattern, or maybe even a color or type, etc.

            // temporary - must change!!!
            return string.Format("Link to {0}", ProductID);
        }


        private string MakeManufacturerProductSetKey(int ProductID)
        {
            ProductRecord product;
            if (ProductRecords.TryGetValue(ProductID, out product))
                return MakeManufacturerProductSetKey(product.ManufacturerID, product.Group);

            return null;
        }

        private string MakeManufacturerProductSetKey(int ManufacturerID, ProductGroup group)
        {
            var key = string.Format("M{0}:{1}", ManufacturerID, (int)group);
            return key;
        }


        private void RemoveManufacturerProductFromFutureCandidates(int ProductID)
        {
            var key = MakeManufacturerProductSetKey(ProductID);

            if (key == null)
                return;

            HashSet<int> list;

            if (!ProductsByManufacturer.TryGetValue(key, out list))
                return;

            list.Remove(ProductID);
        }

        private int AddInboundLink(int fromProductID, int toProductID)
        {
            List<int> links = null;

            if (InboundLinks.TryGetValue(toProductID, out links))
            {
                Debug.Assert(!links.Contains(fromProductID));
                var startCount = links.Count;
                if (!links.Contains(fromProductID))
                    links.Add(fromProductID);
                return startCount;
            }

            InboundLinks.Add(toProductID, new List<int>() { fromProductID});
            return 0; // there were no previous links
        }


        private void AddOutboundLink(int fromProductID, int toProductID)
        {
            List<int> links = null;

            if (OutboundLinks.TryGetValue(fromProductID, out links))
            {
                Debug.Assert(!links.Contains(toProductID));

                if (!links.Contains(toProductID))
                    links.Add(toProductID);

                // if maxed out, then remove from further consideration as a candidate

                if (links.Count() == MaxOutboundLinks)
                    RemoveManufacturerProductFromFutureCandidates(fromProductID);

                return;
            }

            OutboundLinks.Add(fromProductID, new List<int>() { toProductID });
        }


        /// <summary>
        /// Create the physical SQL link between these two products. Anchor text randomly chosen.
        /// </summary>
        /// <remarks>
        /// Generally assumes all filtering/qualification tests have already been performed
        /// by the caller. In-memory collections updated.
        /// </remarks>
        /// <param name="fromProductID"></param>
        /// <param name="toProductID"></param>
        private void CreateCrossLink(int fromProductID, int toProductID)
        {
            // add to inbound links
            // add to outbound links
            // choose anchor text
            // add to SQL

            // need to update the in-memory collections within the core thread to ensure
            // all other picking works the way it needs

            var startCount = AddInboundLink(fromProductID, toProductID);
            AddOutboundLink(fromProductID, toProductID);

            // then queue up the actual insert into SQL - which also includes picking the anchor text

            // try and see if is still for the same product, if so, then just add to the list
            if (CurrentProductCrossLinks != null)
            {
                if (CurrentProductCrossLinks.ToProductID == toProductID)
                {
                    // is for same product, just add to it
                    CurrentProductCrossLinks.AddFrom(fromProductID);
                    return;
                }

                // different product, commit the old one, then drop through
                // and begin a new record for this new target

                sqlActionBlock.Post(CurrentProductCrossLinks);
                CurrentProductCrossLinks = null;

            }

            // start a new record
            // notice that start count only comes into play when we're creating a new record

            CurrentProductCrossLinks = new ProductCrossLinks(toProductID, startCount);
            CurrentProductCrossLinks.AddFrom(fromProductID);
        }


        /// <summary>
        /// Primary entry point to begin a full sweep and create any needed links.
        /// </summary>
        /// <param name="requiredInboundLinks"></param>
        /// <param name="progressCallback"></param>
        /// <param name="cancelToken"></param>
        public void Run(int requiredInboundLinks, IProgress<int> progressCallback, CancellationToken cancelToken)
        {
            int grandTotalLinksCreated = 0; // number of individual links put into queue
            int totalLinksPersisted = 0; // number of links persisted from queue
            int countTotalEstimate = 0; // number of links we figure we're going to create, but it could be knocked down due to short counts

            bool isQueueCompleted = false; // used as a barrier to prevent progress meter completion until the queue is done

            var lockOjb = new object(); // serialize access to Report()

            int lastReportedPercent = -1;
            Action<int, int> Report = (cntCompleted, cntTotal) =>
                {
                    var pct = cntTotal == 0 ? 0 : (cntCompleted * 100) / cntTotal;

                    if (pct == lastReportedPercent)
                        return;

                    // dont' allow to go past 99% if the queue is not done
                    if (pct >= 99 && !isQueueCompleted)
                        return;

                    lastReportedPercent = pct;

                    if (progressCallback != null)
                    {
                        progressCallback.Report(pct);
                        System.Threading.Thread.Sleep(50);
                    }
                };

            try
            {
                this.RequiredInboundLinks = requiredInboundLinks;
                MaxOutboundLinks = (int) Math.Round(requiredInboundLinks * 1.3M);

                DataContext = new AspStoreDataContext(Store.ConnectionString);

                Initialize(cancelToken);

                if (cancelToken.IsCancellationRequested)
                    return;

                sqlActionBlock = new ActionBlock<ProductCrossLinks>((rec) =>
                {
                    // the input is a ProductCrossLinks object which is for a single "to" target,
                    // but can have N from sources. There is no guarantee that this is the only
                    // object for this target, but in general, will be bunched up pretty much so
                    // in effort to maximize use of the ProcessProduct() data. 

                    int countLinks=0;
                    try
                    {
                        using (var dc = new AspStoreDataContext(Store.ConnectionString))
                        {
                            ProductUpdateManager<TProduct>.ProcessProduct(Store, rec.ToProductID, (prd) =>
                                {
                                    // supposed to return the number of links requested, but guard for shortfall
                                    var anchors = prd.SpinAnchorText(rec.FromProducts.Count(), rec.PreviousLinkCount);

                                    if (anchors == null)
                                        return;

                                    var anchorIndex = 0;
                                    foreach (var fromProductID in rec.FromProducts)
                                    {
                                        if (cancelToken.IsCancellationRequested)
                                            return;

                                        if (anchorIndex >= anchors.Count())
                                            return;

                                        var linkText = anchors[anchorIndex++];
                                        dc.ProductCrossLinks.InsertProductCrossLink(fromProductID, rec.ToProductID, linkText);
                                        countLinks++;
                                    }

                                }, dc);
                        }
                    }
                    catch (Exception Ex)
                    {
                        Debug.WriteLine("Exception from queue method: " + Ex.Message);
                    }
                    finally
                    {
                        Interlocked.Add(ref totalLinksPersisted, countLinks);

                        lock (lockOjb)
                        {
                            Report(totalLinksPersisted, countTotalEstimate);
                        }
                    }

                }, new ExecutionDataflowBlockOptions()
                {
                    MaxDegreeOfParallelism = 8,
                    CancellationToken = cancelToken
                });

                // this is an estimate - to be updated once the final number becomes known.
                countTotalEstimate = ProductsNeedingLinks.Select(e => LinksNeeded(e)).Sum();

                Debug.WriteLine(string.Format("Creating inbound links for {0:N0} products.", ProductsNeedingLinks.Count));
                Debug.WriteLine(string.Format("Estimated links to create: {0:N0}", countTotalEstimate));

                // reporting of progress along the way is based on what has completed through the queue.
                Report(0, countTotalEstimate);

                // this core loop gets SQL items posted to the queue, then waits for the queue to complete

                foreach (var productID in ProductsNeedingLinks)
                {
                    if (cancelToken.IsCancellationRequested)
                        break;

                    // do the work for a single product id that needs links

                    var linksNeeded = LinksNeeded(productID);
                    int countLinksCreatedPass1 = 0;
                    int countLinksCreatedPass2 = 0;

                    // try initially to fulfill with close links, otherwise, go back with a broader search for candidates

                    countLinksCreatedPass1 = ProcessInboundLinksForSingleProduct(productID, RequireCloseLinkingCandidates:true);
                    if (countLinksCreatedPass1 < linksNeeded && !cancelToken.IsCancellationRequested)
                        countLinksCreatedPass2 = ProcessInboundLinksForSingleProduct(productID, RequireCloseLinkingCandidates: false);

                    var countLinksCreatedForThisProduct = countLinksCreatedPass1 + countLinksCreatedPass2;

                    // along the way, revise estimate - still possible that we didn't satisfy all the inbound requirements

                    var shortfall = RequiredInboundLinks - countLinksCreatedForThisProduct;
                    Interlocked.Add(ref countTotalEstimate, -shortfall);

                    //if (shortfall != 0)
                    //    Debug.WriteLine(string.Format("Short by {0} links: {1}", shortfall, productID));

                    grandTotalLinksCreated += countLinksCreatedForThisProduct;
                }

                // adjust the stats now that we for sure have the final number
                if (!cancelToken.IsCancellationRequested)
                    countTotalEstimate = grandTotalLinksCreated;

                // indicate all work items have been submitted

                if (CurrentProductCrossLinks != null)
                {
                    sqlActionBlock.Post(CurrentProductCrossLinks);
                    CurrentProductCrossLinks = null;
                }

                
                sqlActionBlock.Complete();

                // wait until all the records have been processed
                sqlActionBlock.Completion.Wait(cancelToken);

                isQueueCompleted = true;

                // set 100 pct
                Report(100, 100);

#if DEBUG
                // calc some stats

                var outboundStats = from e in OutboundLinks.Values 
                             let linkCount = e.Count()
                             group e by linkCount into g
                             orderby g.Key
                             select new
                             {
                                 LinkCount = g.Key,
                                 Products = g.Count()
                             };


                Debug.WriteLine("\n\n------- Outbound Links Stats -------");
                foreach (var stat in outboundStats)
                    Debug.WriteLine(string.Format("    {0} : {1:N0}", stat.LinkCount, stat.Products));



                var inboundStats = from e in InboundLinks.Values
                                    let linkCount = e.Count()
                                    group e by linkCount into g
                                    orderby g.Key
                                    select new
                                    {
                                        LinkCount = g.Key,
                                        Products = g.Count()
                                    };

                Debug.WriteLine("\n\n------- Inbound Links Stats -------");
                foreach (var stat in inboundStats)
                    Debug.WriteLine(string.Format("    {0} : {1:N0}", stat.LinkCount, stat.Products));

                Debug.WriteLine("\n\n");
#endif
            }
            catch (Exception Ex)
            {
                if (!cancelToken.IsCancellationRequested)
                {
                    Debug.WriteLine(Ex.Message);
                    throw;
                }
            }
            finally
            {
                Cleanup();
                Debug.WriteLine(string.Format("Total crosslinks created: {0:N0}", grandTotalLinksCreated));
                Debug.WriteLine(string.Format("Total crosslinks persisted: {0:N0}", totalLinksPersisted));
            }
        }

    }
}