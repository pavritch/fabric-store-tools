using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fabricut.Discovery;
using ProductScanner.Core;
using ProductScanner.Core.Scanning.Pipeline.Metadata;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Sessions;

namespace Fabricut.Metadata
{
    public class FabricutMetadataCollector : FabricutMetadataCollector<FabricutVendor>
    {
        public FabricutMetadataCollector(FabricutStockFileLoader<FabricutVendor> stockFileLoader, IVendorScanSessionManager<FabricutVendor> session)
            : base(stockFileLoader, session) { }
    }

    public class SHarrisMetadataCollector : FabricutMetadataCollector<SHarrisVendor>
    {
        public SHarrisMetadataCollector(FabricutStockFileLoader<SHarrisVendor> stockFileLoader, IVendorScanSessionManager<SHarrisVendor> session)
            : base(stockFileLoader, session) { }
    }

    public class StroheimMetadataCollector : FabricutMetadataCollector<StroheimVendor>
    {
        public StroheimMetadataCollector(FabricutStockFileLoader<StroheimVendor> stockFileLoader, IVendorScanSessionManager<StroheimVendor> session)
            : base(stockFileLoader, session) { }
    }

    public class TrendMetadataCollector : FabricutMetadataCollector<TrendVendor>
    {
        public TrendMetadataCollector(FabricutStockFileLoader<TrendVendor> stockFileLoader, IVendorScanSessionManager<TrendVendor> session)
            : base(stockFileLoader, session) { }
    }

    public class VervainMetadataCollector : FabricutMetadataCollector<VervainVendor>
    {
        public VervainMetadataCollector(FabricutStockFileLoader<VervainVendor> stockFileLoader, IVendorScanSessionManager<VervainVendor> session)
            : base(stockFileLoader, session) { }
    }

    public class FabricutMetadataCollector<T> : IMetadataCollector<T> where T : Vendor
    {
        private readonly FabricutStockFileLoader<T> _stockFileLoader;
        private readonly IVendorScanSessionManager<T> _sessionManager; 

        public FabricutMetadataCollector(FabricutStockFileLoader<T> stockFileLoader, IVendorScanSessionManager<T> sessionManager)
        {
            _stockFileLoader = stockFileLoader;
            _sessionManager = sessionManager;
        }

        public Task<List<ScanData>> PopulateMetadata(List<ScanData> products)
        {
            var stockData = _stockFileLoader.LoadStockData();
            _sessionManager.ForEachNotify("Loading stock data", products, product =>
            {
                // find the matching stock data
                var matchingStockData = stockData.FirstOrDefault(x => x[ScanField.ManufacturerPartNumber] == product[ScanField.ItemNumber]);
                if (matchingStockData == null)
                {
                    // from Fabricut - ones that are missing from the stock file are discontinued
                    product[ScanField.IsDiscontinued] = "Yes";
                    return;
                }

                product[ScanField.StockCount] = matchingStockData[ScanField.StockCount];
                product[ScanField.Cost] = matchingStockData[ScanField.Cost];
                product[ScanField.RetailPrice] = matchingStockData[ScanField.RetailPrice];

                if (matchingStockData[ScanField.Status] == "D")
                {
                    if (product[ScanField.StockCount].ToDoubleSafe() > 0)
                    {
                        product[ScanField.IsLimitedAvailability] = true.ToString();
                        product.IsClearance = true;
                    }
                    else
                    {
                        // also, if status is discontinued, and there is 0 stock from stock file, then it's discontinued as well
                        product[ScanField.IsDiscontinued] = "Yes";
                    }
                }
            });
            return Task.FromResult(products);
        }
    }
}