using System;
using System.Collections.Generic;
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
using GalaSoft.MvvmLight;

namespace ProductScanner.App.Controls
{
    /// <summary>
    /// Interaction logic for CommitBatchInputs.xaml
    /// </summary>
    public partial class CommitBatchInputs : UserControl
    {
        public CommitBatchInputs()
        {
            InitializeComponent();
        }
    }

}

namespace ProductScanner.App.ViewModels
{
    public class CommitBatchInputsViewModel : ViewModelBase
    {

        private bool _isDisabled = false;
        public bool IsDisabled
        {
            get
            {
                return _isDisabled;
            }
            set
            {
                Set(() => IsDisabled, ref _isDisabled, value);
            }
        }

        private bool _ignoreDuplicates = false;
        public bool IgnoreDuplicates
        {
            get
            {
                return _ignoreDuplicates;
            }
            set
            {
                Set(() => IgnoreDuplicates, ref _ignoreDuplicates, value);
            }
        }
    }
}
