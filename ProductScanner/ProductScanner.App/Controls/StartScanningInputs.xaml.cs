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
using GalaSoft.MvvmLight;
using ProductScanner.Core.Scanning.Pipeline;
using Utilities;

namespace ProductScanner.App.Controls
{
    /// <summary>
    /// Interaction logic for StartScanningInputs.xaml
    /// </summary>
    public partial class StartScanningInputs : UserControl
    {
        public StartScanningInputs()
        {
            InitializeComponent();
        }
    }

}

namespace ProductScanner.App.ViewModels
{
    public class StartScanningInputsViewModel : ViewModelBase
    {

        public StartScanningInputsViewModel()
        {
            PopulateOptions();
        }

        private bool _isDisabled = false;
        public bool IsDisabled
        {
            get
            {
                return _isDisabled;
            }
            set
            {
                if (Set(() => IsDisabled, ref _isDisabled, value))
                    SetOptionsEnabledState(!value);
            }
        }


        private void PopulateOptions()
        {
            var list = new List<ProductScanner.App.ViewModels.VendorScanViewModel.ScanOptionViewModel>();

            foreach (var opt in LibEnum.GetValues<ScanOptions>())
                list.Add(new ProductScanner.App.ViewModels.VendorScanViewModel.ScanOptionViewModel(opt));

            Options = new ObservableCollection<ProductScanner.App.ViewModels.VendorScanViewModel.ScanOptionViewModel>(list);
            SetOptionsEnabledState(true);
        }

        private void SetOptionsEnabledState(bool isEnabled)
        {
            if (Options == null)
                return;

            foreach (var item in Options)
                item.IsEnabled = isEnabled;
        }

        private ObservableCollection<ProductScanner.App.ViewModels.VendorScanViewModel.ScanOptionViewModel> _options = null;
        public ObservableCollection<ProductScanner.App.ViewModels.VendorScanViewModel.ScanOptionViewModel> Options
        {
            get
            {
                return _options;
            }
            set
            {
                Set(() => Options, ref _options, value);
            }
        }

 
    }
}
