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
using Telerik.Windows.Controls;

namespace ProductScanner.App.Controls
{
    /// <summary>
    /// Interaction logic for CommitRecordDlg.xaml
    /// </summary>
    public partial class CommitRecordDlg : RadWindow
    {
        private ICommitRecordDetails record;

        public CommitRecordDlg(ICommitRecordDetails record)
        {
            this.Owner = ProductScanner.App.MainWindow.Current;
            this.record = record;
            Header = record.Title;
            Loaded += CommitRecordDlg_Loaded;
            InitializeComponent();
        }

        void CommitRecordDlg_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = new ProductScanner.App.ViewModels.CommitRecordDlgViewModel(record);
        }
    }
}
