using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    public class ManufacturerIdentity
    {
        public int ManufacturerID { get; set; }

        public string ManufacturerName { get; set; }

        public StoreKeys StoreKey { get; set; }

        public ManufacturerIdentity()
        {

        }
    }
}