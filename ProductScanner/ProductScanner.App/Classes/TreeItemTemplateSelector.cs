using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Data;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace ProductScanner.App
{
    /// <summary>
    /// Select the appropriate data template for the left navigation tree view node items.
    /// </summary>
    public class TreeViewDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate VendorTemplate
        {
            get;
            set;
        }

        public DataTemplate NotImplementedVendorTemplate
        {
            get;
            set;
        }

        public HierarchicalDataTemplate StoreTemplate
        {
            get;
            set;
        }

        public TreeViewDataTemplateSelector()
        {
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            base.SelectTemplate(item, container);

            if (item is IStoreModel)
            {
                return this.StoreTemplate;
            }
            else if (item is IVendorModel)
            {
                var vendor = item as IVendorModel;
                return vendor.IsFullyImplemented ? this.VendorTemplate : this.NotImplementedVendorTemplate;
            }

            return null;
        }
    }


}
