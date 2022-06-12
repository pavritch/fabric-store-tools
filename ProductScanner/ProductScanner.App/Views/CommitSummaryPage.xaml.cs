using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using ProductScanner.App.ViewModels;
using ProductScanner.Core.DataInterfaces;

namespace ProductScanner.App.Views
{
    /// <summary>
    /// Interaction logic for CommitSummaryPage.xaml
    /// </summary>
    public partial class CommitSummaryPage : UserControl
    {
        public CommitSummaryPage()
        {
            InitializeComponent();
        }

        private CommitSummaryPageViewModel VM
        {
            get
            {
                return this.DataContext as CommitSummaryPageViewModel;
            }
        }

        #region CommitsItemsSource Property

        // since this control is intended to be used from three places (global, store, vendor),
        // the caller is responsible to pass in a pre-filtered set of data suitable for the use.

        public const string CommitsItemsSourcePropertyName = "CommitsItemsSource";
        public IEnumerable<CommitBatchSummary> CommitsItemsSource
        {
            get
            {
                return (IEnumerable<CommitBatchSummary>)GetValue(CommitsItemsSourceProperty);
            }
            set
            {
                SetValue(CommitsItemsSourceProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="CommitsItemsSource" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty CommitsItemsSourceProperty = DependencyProperty.Register(
            CommitsItemsSourcePropertyName,
            typeof(IEnumerable<CommitBatchSummary>),
            typeof(CommitSummaryPage),
           new UIPropertyMetadata(null, new PropertyChangedCallback(CommitsItemsSourceChanged)));

        protected static void CommitsItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = e.NewValue as IEnumerable<CommitBatchSummary>;
            var ctrl = (CommitSummaryPage)d;
            ctrl.VM.SetItemsSource(value);
        }

        #endregion

        #region ShowVendorName Property

        // Allow caller to indicate if the RadGrid should display the vendor name column.

        public const string ShowVendorNamePropertyName = "ShowVendorName";
        public bool ShowVendorName
        {
            get
            {
                return (bool)GetValue(ShowVendorNameProperty);
            }
            set
            {
                SetValue(ShowVendorNameProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="ShowVendorName" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty ShowVendorNameProperty = DependencyProperty.Register(
            ShowVendorNamePropertyName,
            typeof(bool),
            typeof(CommitSummaryPage),
           new UIPropertyMetadata(false, new PropertyChangedCallback(ShowVendorNameChanged)));

        protected static void ShowVendorNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (bool)e.NewValue;
            var ctrl = (CommitSummaryPage)d;
            ctrl.VM.ShowVendorName = value;

        }
        
        #endregion

        #region ShowStoreName Property

        // Allow caller to indicate if the RadGrid should display the store name column.

        public const string ShowStoreNamePropertyName = "ShowStoreName";
        public bool ShowStoreName
        {
            get
            {
                return (bool)GetValue(ShowStoreNameProperty);
            }
            set
            {
                SetValue(ShowStoreNameProperty, value);
            }
        }

        public static readonly DependencyProperty ShowStoreNameProperty = DependencyProperty.Register(
            ShowStoreNamePropertyName,
            typeof(bool),
            typeof(CommitSummaryPage),
           new UIPropertyMetadata(false, new PropertyChangedCallback(ShowStoreNameChanged)));

        protected static void ShowStoreNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (bool)e.NewValue;
            var ctrl = (CommitSummaryPage)d;
            ctrl.VM.ShowStoreName = value;
        }

        #endregion

        private void RadGridView_SelectionChanging(object sender, Telerik.Windows.Controls.SelectionChangingEventArgs e)
        {
            e.Cancel = true;
        }

    }
}
