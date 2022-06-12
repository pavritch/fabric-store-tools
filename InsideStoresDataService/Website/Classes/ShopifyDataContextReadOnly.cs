//------------------------------------------------------------------------------
// 
// Class: ShopifyDataContextReadOnly 
//
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Website
{
    public class ShopifyDataContextReadOnly : ShopifyDataContext
    {

        public ShopifyDataContextReadOnly()
        {
            ObjectTrackingEnabled = false;
        }

        public ShopifyDataContextReadOnly(string connection)
            : base(connection)
        {
            ObjectTrackingEnabled = false;
        }

    }
}