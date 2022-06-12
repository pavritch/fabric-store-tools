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

    public partial class DeleteDiscardedBatchesInputs : UserControl
    {
        public DeleteDiscardedBatchesInputs()
        {
            InitializeComponent();
        }
    }

}

namespace ProductScanner.App.ViewModels
{
    public class DeleteDiscardedBatchesInputsViewModel : ViewModelBase
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


        private int _dayCount = 30;
        public int DayCount
        {
            get
            {
                return _dayCount;
            }
            set
            {
                Set(() => DayCount, ref _dayCount, value);
            }
        }

        /// <summary>
        /// True when only discarded batches will be selected; in combination with day count.
        /// </summary>
        private bool _isOnlyDiscardedBatches = true;
        public bool IsOnlyDiscardedBatches
        {
            get
            {
                return _isOnlyDiscardedBatches;
            }
            set
            {
                Set(() => IsOnlyDiscardedBatches, ref _isOnlyDiscardedBatches, value);
            }
        }
    }
}
