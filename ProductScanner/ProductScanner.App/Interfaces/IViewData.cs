using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InsideFabric.Data;
using ProductScanner.Core.Scanning.Products;

namespace ProductScanner.App
{
    /// <summary>
    /// Marker interface.
    /// </summary>
    public interface IViewData
    {
        /// <summary>
        /// returns details for this record. Mostly used by right click logic when viewing batches.
        /// </summary>
        /// <returns></returns>
        ICommitRecordDetails GetDetails();
    }
}
