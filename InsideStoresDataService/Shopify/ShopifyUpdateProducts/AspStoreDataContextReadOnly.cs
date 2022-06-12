//------------------------------------------------------------------------------
// 
// Class: AspStoreDataContextReadOnly 
//
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace ShopifyUpdateProducts
{
    public class AspStoreDataContextReadOnly : AspStoreDataContext
    {

        public AspStoreDataContextReadOnly()
        {
            ObjectTrackingEnabled = false;
        }

        public AspStoreDataContextReadOnly(string connection)
            : base(connection)
        {
            ObjectTrackingEnabled = false;
        }

    }
}