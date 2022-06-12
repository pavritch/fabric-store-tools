//------------------------------------------------------------------------------
// 
// Class: RugsCategoryFilterManager 
//
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Website
{
    public class RugsCategoryFilterManager : CategoryFilterManagerBase<InsideRugsProduct>, ICategoryFilterManager
    {
        public RugsCategoryFilterManager(IWebStore Store) : base(Store)
        {
            // at this point the root node is guaranteed to exist
            // build up the tree

            Initialize();
        }

        private void Initialize()
        {
            var roots = new List<ICategoryFilterRoot<InsideRugsProduct>>()
            {
                // order here is not important
                new RugsCategoryFilterRootColor(Store),
                new RugsCategoryFilterRootMaterial(Store),
                new RugsCategoryFilterRootPileHeight(Store),
                new RugsCategoryFilterRootPrice(Store),
                new RugsCategoryFilterRootShape(Store),
                new RugsCategoryFilterRootStyle(Store),
                new RugsCategoryFilterRootSize(Store),
                new RugsCategoryFilterRootWeave(Store),
            };

            base.Initialize(roots);
        }

        /// <summary>
        /// Classifies the specified product.
        /// </summary>
        /// <param name="product">The product.</param>
        public void Classify(object product)
        {
            Debug.Assert(product is InsideRugsProduct);

            if (product is InsideRugsProduct)
                base.Classify(product as InsideRugsProduct);
        }

        /// <summary>
        /// Buld rebulding of color filters using a global-centric rather than product centric approach.
        /// </summary>
        /// <param name="cancelToken"></param>
        /// <param name="reportProgress"></param>
        /// <param name="reportStatus"></param>
        public void RebuildColorFilters(System.Threading.CancellationToken cancelToken, Action<int> reportProgress, Action<string> reportStatus = null)
        {
            var colorFilter = new RugsCategoryFilterRootColor(Store);
            colorFilter.RebuildColorFilters(cancelToken, reportProgress, reportStatus);
        }
    }
}