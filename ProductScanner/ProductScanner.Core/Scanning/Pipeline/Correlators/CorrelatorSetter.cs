using System;
using System.Collections.Generic;
using System.Linq;
using ProductScanner.Core.Scanning.ProductProperties;
using ProductScanner.Core.Scanning.Products;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Sessions;
using Utilities.Extensions;

namespace ProductScanner.Core.Scanning.Pipeline.Correlators
{
    public class CorrelatorSetter<T> : ICorrelatorSetter<T> where T : Vendor
    {
        private readonly IVendorScanSessionManager<T> _sessionManager;

        public CorrelatorSetter(IVendorScanSessionManager<T> sessionManager)
        {
            _sessionManager = sessionManager;
        }

        public void SetCorrelators(List<ScanData> vendorProducts, List<StoreProduct> sqlProducts)
        {
            var patternGroups = GetPatternGroups(vendorProducts);
            _sessionManager.ForEachNotify("Setting correlators", vendorProducts, product =>
            {
                if (product.HasCorrelator()) return;

                var sqlProduct = sqlProducts.SingleOrDefault(x => x.SKU == product[ScanField.SKU]);

                // if there's a matching sql product, we want to grab that correlator
                if (sqlProduct != null)
                {
                    product[ScanField.PatternCorrelator] = sqlProduct.Correlator;
                    return;
                }

                // if there's no matching sql product, it's a new product
                // find the product's group
                var group = patternGroups.FirstOrDefault(x => x.Contains(product[ScanField.ManufacturerPartNumber].ToLower(), true));

                // if there's no group then just assign a correlator for just that one, since may actually
                // have some friends on another update run.
                if (group == null)
                {
                    product[ScanField.PatternCorrelator] = Guid.NewGuid().ToString();
                    return;
                }

                // see if any items in this group have a correlator
                var groupCorrelator = FindGroupCorrelator(group, sqlProducts);
                if (groupCorrelator != null)
                {
                    product[ScanField.PatternCorrelator] = groupCorrelator;
                    return;
                }

                // otherwise we have no correlator so we need to create one and assign it to the group
                AssignGroupCorrelator(group, vendorProducts);
            });
        }

        private List<List<string>> GetPatternGroups(List<ScanData> vendorProducts)
        {
            var allGroups = vendorProducts.Select(x => x.RelatedProducts.Where(p => p != null).Select(p => p.ToLower()).OrderBy(p => p));
            var distinctGroups = new List<List<string>>();
            _sessionManager.ForEachNotify("Grouping patterns", allGroups, group =>
            {
                if (!distinctGroups.Any(group.SequenceEqual)) distinctGroups.Add(group.ToList());
            });
            return distinctGroups;
        }

        private void AssignGroupCorrelator(List<string> mpns, IEnumerable<ScanData> products)
        {
            var correlator = Guid.NewGuid();
            var groupProducts = products.Where(x => mpns.Contains(x[ScanField.ManufacturerPartNumber], true));
            groupProducts.ForEach(x => x[ScanField.PatternCorrelator] = correlator.ToString());
        }

        private string FindGroupCorrelator(IEnumerable<string> mpns, List<StoreProduct> sqlProducts)
        {
            foreach (var mpn in mpns)
            {
                // TODO: Fix
                //var sqlProduct = sqlProducts.SingleOrDefault(x => x.ManufacturerPartNumber == mpn);
                //if (sqlProduct != null)
                //{
                    //if (!string.IsNullOrWhiteSpace(sqlProduct.Correlator))
                    //{
                        //return sqlProduct.Correlator;
                    //}
                //}
            }
            return null;
        }
    }
}