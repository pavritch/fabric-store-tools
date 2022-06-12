using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using InsideFabric.Data;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Scanning.Storage;
using Utilities.Extensions;

namespace Momeni
{
    /*
    public class MomeniMetadataCollector : IMetadataCollector<MomeniVendor>
    {
        // urls:
        // list here
        // ftp://momeni.ws/1-%20HIGHRES_IMAGES/ROUND-RUNNER-CLOSE-UPS/CLOSE%20UPS/

        // list here - not sure how useful though
        // ftp://momeni.ws/1-%20HIGHRES_IMAGES/Roomshots/

        // list of collections here
        // ftp://momeni.ws/1-%20HIGHRES_IMAGES/ROUND-RUNNER-CLOSE-UPS/ROUNDS/

        // list of collections here
        // ftp://momeni.ws/1-%20HIGHRES_IMAGES/ROUND-RUNNER-CLOSE-UPS/RUNNERS/

        // list of collections here
        // ftp://momeni.ws/1-%20HIGHRES_IMAGES/Rectangulars/

        // ftp://momeni.ws
        // user id - mediadl
        // password -Download!60 

        private readonly IPageFetcher<MomeniVendor> _pageFetcher;
        private readonly NetworkCredential _credential = new NetworkCredential("mediadl", "Download!60");

        public MomeniMetadataCollector(IPageFetcher<MomeniVendor> pageFetcher)
        {
            _pageFetcher = pageFetcher;
        }

        public async Task<List<ScanData>> PopulateMetadata(List<ScanData> variants)
        {
            var allUrls = new List<string>();

            var closeupUrls = await _pageFetcher.FetchFtpAsync("ftp://mediadl:Download!60@momeni.ws/1-%20HIGHRES_IMAGES/2-ROUND-RUNNER-CLOSE-UPS/CLOSE%20UPS/", CacheFolder.Images, "closeups", _credential);
            allUrls.AddRange(closeupUrls.Select(x => "ftp://mediadl:Download!60@momeni.ws/1-%20HIGHRES_IMAGES/2-ROUND-RUNNER-CLOSE-UPS/CLOSE%20UPS/" + x));

            var roomshotUrls = await _pageFetcher.FetchFtpAsync("ftp://mediadl:Download!60@momeni.ws/1-%20HIGHRES_IMAGES/3-Roomshots/", CacheFolder.Images, "roomshots", _credential);
            allUrls.AddRange(roomshotUrls.Select(x => "ftp://mediadl:Download!60@momeni.ws/1-%20HIGHRES_IMAGES/3-Roomshots/" + x));

            var roundUrls = await CollectUrls("ftp://mediadl:Download!60@momeni.ws/1-%20HIGHRES_IMAGES/2-ROUND-RUNNER-CLOSE-UPS/ROUNDS/", "round");
            var runnerUrls = await CollectUrls("ftp://mediadl:Download!60@momeni.ws/1-%20HIGHRES_IMAGES/2-ROUND-RUNNER-CLOSE-UPS/RUNNERS/", "runners");
            var rectUrls = await CollectUrls("ftp://mediadl:Download!60@momeni.ws/1-%20HIGHRES_IMAGES/1-Rectangulars/", "rect");

            allUrls.AddRange(roundUrls);
            allUrls.AddRange(runnerUrls);
            allUrls.AddRange(rectUrls);

            foreach (var variant in variants)
            {
                // find urls that match this product (column sku)
                var imageKey = variant[ScanField.SKU];

                var images = FindImagesForKey(allUrls, imageKey);

                // a couple are different
                imageKey = imageKey.Replace("69ORG", "69ORA");
                images.AddRange(FindImagesForKey(allUrls, imageKey));

                images.ForEach(x => variant.AddImage(x));
            }
            return variants;
        }

        private List<ScannedImage> FindImagesForKey(List<string> allUrls, string key)
        {
            var images = new List<ScannedImage>();
            var matchedImages = allUrls.Where(x => x.Contains(key)).ToList();

            //matchedImages.Where(x => x.ContainsIgnoreCase("close%20ups")).ForEach(x => product.AddImage(new ScannedImage(ImageVariantType.Alternate, x, _credential)));
            matchedImages.Where(x => x.ContainsIgnoreCase("scene")).ForEach(x => 
                images.Add(new ScannedImage(ImageVariantType.Scene, x, _credential)));

            matchedImages.Where(x => x.ContainsIgnoreCase("runners")).ForEach(x => 
                images.Add(new ScannedImage(ImageVariantType.Runner, x, _credential)));

            matchedImages.Where(x => x.ContainsIgnoreCase("rectangulars")).ForEach(x => 
                images.Add(new ScannedImage(ImageVariantType.Rectangular, x, _credential))
            );
            matchedImages.Where(x => x.ContainsIgnoreCase("roomshots")).ForEach(x => images.Add(new ScannedImage(ImageVariantType.Scene, x, _credential)));
            matchedImages.Where(x => x.ContainsIgnoreCase("rounds")).ForEach(x => images.Add(new ScannedImage(ImageVariantType.Round, x, _credential)));
            return images;
        }

        private async Task<List<string>> CollectUrls(string rootUrl, string folder)
        {
            var allUrls = new List<string>();
            var collections = (await _pageFetcher.FetchFtpAsync(rootUrl, CacheFolder.Images, folder, _credential)).Where(x => !x.EndsWith(".lnk"));
            foreach (var collection in collections)
            {
                var urls = await _pageFetcher.FetchFtpAsync(rootUrl + collection.Replace(" ", "%20") + "/", CacheFolder.Images, folder + "-" + collection, _credential);
                allUrls.AddRange(urls.Select(x => rootUrl + collection + "/" + x));
            }
            return allUrls;
        }
    }
    */
}