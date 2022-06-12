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
    public class VendorRadContextMenu : RadContextMenu
    {

        public VendorRadContextMenu()
        {
            Loaded += VendorRadContextMenu_Loaded;
        }

        #region Vendor Property
        /// <summary>
        /// The <see cref="Vendor" /> dependency property's name.
        /// </summary>
        public const string VendorPropertyName = "Vendor";

        /// <summary>
        /// Gets or sets the value of the <see cref="Vendor" />
        /// property. This is a dependency property.
        /// </summary>
        public IVendorModel Vendor
        {
            get
            {
                return (IVendorModel)GetValue(VendorProperty);
            }
            set
            {
                SetValue(VendorProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="Vendor" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty VendorProperty = DependencyProperty.Register(
            VendorPropertyName,
            typeof(IVendorModel),
            typeof(VendorRadContextMenu),
        new UIPropertyMetadata(null, new PropertyChangedCallback(VendorChanged)));

        protected static void VendorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //var vendor = e.NewValue as IVendorModel;
            //var ctrl = (VendorRadContextMenu)d;
        }
        #endregion

        void VendorRadContextMenu_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var menu = sender as RadContextMenu;

            if (Vendor == null)
                Vendor = menu.DataContext as IVendorModel;

            if (Vendor == null)
                return;

            var dc = new ProductScanner.App.ViewModels.VendorContextMenuViewModel(Vendor);
            menu.DataContext = dc;    
        
            // doing this here rather than in Xaml is simply to prevent all the binding errors reported
            // to output when transitioning the datacontext to the vm we want

            foreach(RadMenuItem item in menu.Items)
            {
                if (item.Header == null)
                    continue;

                switch(item.Header as string)
                {
                    // scanning 

                    case "Start":
                        item.Command = dc.StartCommand;
                        break;

                    case "Suspend":
                        item.Command = dc.SuspendCommand;
                        break;

                    case "Resume":
                        item.Command = dc.ResumeCommand;
                        break;

                    case "Cancel":
                        item.Command = dc.CancelCommand;
                        break;

                    // pending batches

                    case "Commit Pending":
                        item.Command = dc.CommitPendingBatchesCommand;
                        break;

                    case "Discard Pending":
                        item.Command = dc.DiscardPendingBatchesCommand;
                        break;

                    case "Delete Batches":
                        item.Command = dc.DeletePendingBatchesCommand;
                        break;

                    // misc 

                    case "Clear Log":
                        item.Command = dc.ClearLogCommand;
                        break;

                    case "Clear Warning":
                        item.Command = dc.ClearWarningCommand;
                        break;

                    case "Delete Cache":
                        item.Command = dc.DeleteCachedFilesCommand;
                        break;

                    // navigation

                    case "Scanner Page":
                        item.Command = dc.ScannerPageCommand;
                        break;
                }
            }
        }
    }
}
