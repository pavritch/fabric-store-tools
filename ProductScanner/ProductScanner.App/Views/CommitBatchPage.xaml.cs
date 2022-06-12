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

namespace ProductScanner.App.Views
{
    /// <summary>
    /// Interaction logic for CommitBatchPage.xaml
    /// </summary>
    public partial class CommitBatchPage : UserControl
    {
        public CommitBatchPage()
        {
            InitializeComponent();
        }

        private CommitBatchPageViewModel VM
        {
            get
            {
                return this.DataContext as CommitBatchPageViewModel;
            }
        }

        #region BatchID Property
        public const string BatchIDPropertyName = "BatchID";
        public int? BatchID
        {
            get
            {
                return (int?)GetValue(BatchIDProperty);
            }
            set
            {
                SetValue(BatchIDProperty, value);
            }
        }

        public static readonly DependencyProperty BatchIDProperty = DependencyProperty.Register(
            BatchIDPropertyName,
            typeof(int?),
            typeof(CommitBatchPage),
           new UIPropertyMetadata(null, new PropertyChangedCallback(BatchIDChanged)));

        protected static void BatchIDChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (int?)e.NewValue;
            var ctrl = (CommitBatchPage)d;
            ctrl.VM.SetBatchID(value);
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
            typeof(CommitBatchPage),
           new UIPropertyMetadata(false, new PropertyChangedCallback(ShowVendorNameChanged)));

        protected static void ShowVendorNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (bool)e.NewValue;
            var ctrl = (CommitBatchPage)d;
            ctrl.VM.ShowVendorName = value;
        }

        #endregion

        private void RadGridView_SelectionChanging(object sender, Telerik.Windows.Controls.SelectionChangingEventArgs e)
        {
            e.Cancel = true;
        }

    }
}
