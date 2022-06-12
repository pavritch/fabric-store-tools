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

    public partial class DeleteCachedFilesInputs : UserControl
    {
        public DeleteCachedFilesInputs()
        {
            InitializeComponent();
        }
    }

}

namespace ProductScanner.App.ViewModels
{
    public class DeleteCachedFilesInputsViewModel : ViewModelBase
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


        private int _dayCount = 0;
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

    }
}
