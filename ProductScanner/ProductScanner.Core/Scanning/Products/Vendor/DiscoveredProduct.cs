using System;

namespace ProductScanner.Core.Scanning.Products.Vendor
{
    public class DiscoveredProduct
    {
        public ScanData ScanData { get; set; }

        public Uri DetailUrl { get; set; }
        public string DiscoveryId { get; set; }

        private string _mpn;
        public string MPN
        {
            get
            {
                if (ScanData != null && ScanData.ContainsKey(ScanField.ManufacturerPartNumber)) return ScanData[ScanField.ManufacturerPartNumber];
                return _mpn;
            }
            set { _mpn = value; }
        }

        public string ProductGroup { get; set; }

        public DiscoveredProduct() { }

        public DiscoveredProduct(ScanData dataData)
        {
            ScanData = dataData;
        }

        public DiscoveredProduct(Uri detailUrl)
        {
            DetailUrl = detailUrl;
            ScanData = new ScanData();
        }

        public DiscoveredProduct(Uri detailUrl, string mpn, string productGroup)
        {
            MPN = mpn;
            DetailUrl = detailUrl;
            ProductGroup = productGroup;
        }

        public DiscoveredProduct(Uri detailUrl, string mpn)
        {
            MPN = mpn;
            DetailUrl = detailUrl;
            ScanData = new ScanData();
        }

        public DiscoveredProduct(string mpn)
        {
            MPN = mpn;
            ScanData = new ScanData();
        }
    }
}