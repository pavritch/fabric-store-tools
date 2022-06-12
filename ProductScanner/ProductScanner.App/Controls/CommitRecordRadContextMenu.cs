using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using ProductScanner.App.ViewModels;
using Telerik.Windows.Controls;
using Utilities.Extensions;

namespace ProductScanner.App.Controls
{
    public class CommitRecordRadContextMenu : RadContextMenu
    {

        public CommitRecordRadContextMenu()
        {
            Loaded += CommitRecordRadContextMenu_Loaded;
        }

        #region CommitRecordDetails Property
        /// <summary>
        /// The <see cref="CommitRecordDetails" /> dependency property's name.
        /// </summary>
        public const string CommitRecordDetailsPropertyName = "CommitRecordDetails";

        /// <summary>
        /// Gets or sets the value of the <see cref="CommitRecordDetails" />
        /// property. This is a dependency property.
        /// </summary>
        public ICommitRecordDetails CommitRecordDetails
        {
            get
            {
                return (ICommitRecordDetails)GetValue(CommitRecordDetailsProperty);
            }
            set
            {
                SetValue(CommitRecordDetailsProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="Vendor" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty CommitRecordDetailsProperty = DependencyProperty.Register(
            CommitRecordDetailsPropertyName,
            typeof(ICommitRecordDetails),
            typeof(CommitRecordRadContextMenu),
        new UIPropertyMetadata(null, new PropertyChangedCallback(CommitRecordDetailsChanged)));

        protected static void CommitRecordDetailsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //var details = e.NewValue as ICommitRecordDetails;
            //var ctrl = (CommitRecordRadContextMenu)d;
        }
        #endregion

        void CommitRecordRadContextMenu_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var menu = sender as RadContextMenu;

            if (CommitRecordDetails == null)
            {
                var viewData = menu.DataContext as IViewData;
                if (viewData == null)
                    return;

                CommitRecordDetails = viewData.GetDetails();

                if (CommitRecordDetails == null)
                    return;
            }

            var dc = new ProductScanner.App.ViewModels.CommitRecordContextMenuViewModel(CommitRecordDetails);
            menu.DataContext = dc;    
        
            // doing this here rather than in Xaml is simply to prevent all the binding errors reported
            // to output when transitioning the datacontext to the vm we want

            foreach(RadMenuItem item in menu.Items)
            {
                if (item.Header == null)
                    continue;

                switch(item.Header as string)
                {
                    case "Store Product":
                        item.Command = dc.BrowseStoreUrl;
                        break;

                    case "Vendor Product":
                        item.Command = dc.BrowseVendorUrl;
                        break;

                    case "Show Details":
                        item.Command = dc.ShowDetails;
                        break;
                }
            }
        }
    }
}
