using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Website.Entities;
using System.Diagnostics;
using System.Globalization;
using System.ComponentModel;
using Gen4.Util.Misc;
using System.Reflection;
using System.Collections.Specialized;
using System.Text;

namespace Website
{
    /// <summary>
    /// 
    /// </summary>
    public interface IStoreProductFeed
    {
        IWebStore Store { get; }
        void PopulateProducts(IProductFeed productFeed);
    }
}