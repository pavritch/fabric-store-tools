using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ProductScanner.App
{

    /// <summary>
    /// Used to instruct the tree view to change the current selection.
    /// </summary>
    /// <remarks>
    /// If both store and vendor are null, then set selection to nothing.
    /// </remarks>
    class ChangeTreeViewSelection : IMessage
    {

        public IStoreModel Store { get; set; }
        public IVendorModel Vendor { get; set; }

        public ChangeTreeViewSelection()
        {

        }

        public ChangeTreeViewSelection(IStoreModel store)
        {
            this.Store = store;
        }

        public ChangeTreeViewSelection(IVendorModel vendor)
        {
            this.Vendor = vendor;
        }
    }
}
