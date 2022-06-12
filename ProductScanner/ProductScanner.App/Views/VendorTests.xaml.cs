using System;
using System.Collections.Generic;
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

namespace ProductScanner.App.Views
{
    /// <summary>
    /// Interaction logic for VendorTests.xaml
    /// </summary>
    public partial class VendorTests : UserControl
    {
        public VendorTests()
        {
            InitializeComponent();
        }


        private void RadGridView_SelectionChanging(object sender, Telerik.Windows.Controls.SelectionChangingEventArgs e)
        {
            e.Cancel = true;
        }

    }
}
