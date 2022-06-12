using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using GalaSoft.MvvmLight.CommandWpf;
using ProductScanner.App.ViewModels;
using Utilities;
using Utilities.Extensions;

namespace ProductScanner.App.Controls
{
    /// <summary>
    /// Interaction logic for VendorScanTuningTab.xaml
    /// </summary>
    public partial class VendorScanTuningTab : UserControl
    {

        public VendorScanTuningTab()
        {
            InitializeComponent();
        }

        private VendorScanTuningTabViewModel VM
        {
            get
            {
                return this.DataContext as VendorScanTuningTabViewModel;
            }
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
            typeof(VendorScanTuningTab),
        new UIPropertyMetadata(null, new PropertyChangedCallback(VendorChanged)));

        protected static void VendorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var vendor = e.NewValue as IVendorModel;
            var ctrl = (VendorScanTuningTab)d;
            ctrl.VM.SetVendor(vendor);
        }
        #endregion
    }
}


namespace ProductScanner.App.ViewModels
{
    public class VendorScanTuningTabViewModel : ViewModelBase
    {
        private const double POWERCURVE_END_VALUE = 1200.0;
        private const double POWERCURVE_START_VALUE = 1.0;

        /// <summary>
        /// Computed array of N entries on a curve for values ranging from POWERCURVE_START_VALUE to POWERCURVE_END_VALUE.
        /// </summary>
        private List<double> powerCurve;
        
        public VendorScanTuningTabViewModel()
        {
            powerCurve = ComputePowerCurve(1000); // 1K entries on curve
        }

        public void SetVendor(IVendorModel vendor)
        {
            this.Vendor = vendor;
            Refresh();
        }

        #region Local Methods

        private void Refresh()
        {
            if (Vendor == null)
                return;

            // populate accordingly based on the Vendor...
            UpdateMSDelay(Vendor.DelayMSBetweenVendorWebsiteRequests);

            ResetDefaultMaxErrorsCountTooltip = string.Format("Default: {0:N0}", Vendor.DefaultMaximumScanningErrorCount);

            if (Vendor.DefaultDelayMSBetweenVendorWebsiteRequests == 0)
                ResetDefaultRequestsPerMinuteTooltip = "Default: No Limit";
            else
                ResetDefaultRequestsPerMinuteTooltip = string.Format("Default: {0:N0} requests/min", MakeRequestsPerMinute(Vendor.DefaultDelayMSBetweenVendorWebsiteRequests));

            InvalidateCommands();
        }

        private void InvalidateCommands()
        {
        }

        /// <summary>
        /// Create the array used to xlate ticks in range 0 to 200|300 into requests per minute, in range POWERCURVE_START_VALUE to POWERCURVE_END_VALUE.
        /// </summary>
        /// <param name="maxTick"></param>
        /// <returns></returns>
        private List<double> ComputePowerCurve(int maxTick)
        {
            // no duplicates
            var set = new HashSet<double>();

            var startValue = POWERCURVE_START_VALUE;
            var endValue = POWERCURVE_END_VALUE;

            for(int t=0; t <= maxTick; t++)
            {
                // CircEaseIn
                // CubicEaseIn
                var v = PennerDoubleAnimation.CircEaseIn(t, startValue, endValue - startValue, maxTick);
                 var intValue = (int)Math.Round(v);
                 set.Add(intValue);
            }
            return set.OrderBy(e => e).ToList();
        }

        private int MakeRequestsPerMinute(int msDelay)
        {
            if (msDelay == 0)
                return int.MaxValue;

            var rpm = (int)Math.Round(60000.0 / (double)msDelay);

            return rpm;            
        }

        #endregion


        #region Public Properties

        private IVendorModel _vendor = null;
        public IVendorModel Vendor
        {
            get
            {
                return _vendor;
            }
            set
            {
                var old = _vendor;

                if (Set(() => Vendor, ref _vendor, value))
                {
                    if (old != null && old.GetType().Implements<INotifyPropertyChanged>())
                        (old as INotifyPropertyChanged).PropertyChanged -= Vendor_PropertyChanged;

                    if (value != null && value.GetType().Implements<INotifyPropertyChanged>())
                        (value as INotifyPropertyChanged).PropertyChanged += Vendor_PropertyChanged;    
                }
            }
        }


        void Vendor_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsScanning")
            {

            }

            if (e.PropertyName == "DelayMSBetweenVendorWebsiteRequests")
            {
                UpdateMSDelay(Vendor.DelayMSBetweenVendorWebsiteRequests);
            }

            InvalidateCommands();
        }


        private void UpdateMSDelay(int ms)
        {
            ApproxDelayMSText = string.Format("Delay {0:N0}ms", ms);
            RequestsPerMinute = MakeRequestsPerMinute(ms); // text gets updated automatically 
            RequestsSliderValue = MakeSliderValue(RequestsPerMinute);
        }

        /// <summary>
        /// Compute the value for the slider from a given MS delay based on the easing power curve.
        /// </summary>
        /// <param name="rpm">Requests per minute</param>
        /// <returns></returns>
        private double MakeSliderValue(int rpm)
        {
            if (rpm == int.MaxValue)
                return 1.0;
            
            int tickValue = 0;

            for (tickValue = 0; tickValue < powerCurve.Count(); tickValue++)
            {
                if (powerCurve[tickValue] >= rpm)
                    break;
            }

            // the needed slider value is on a scale of 0.0 to 1.0, so we need to take this tick
            // value, which is in the range 0 to N, and translate it into the correct scale.

            return tickValue / (double)powerCurve.Count();
        }


        /// <summary>
        /// This is the true value in the slider from 0.0 to 1.0.
        /// </summary>
        private double _requestsSliderValue = 0.0;
        public double RequestsSliderValue
        {
            get
            {
                return _requestsSliderValue;
            }
            set
            {
                if (Set(() => RequestsSliderValue, ref _requestsSliderValue, value))
                {
                    if (powerCurve == null || powerCurve.Count == 0 || Vendor==null)
                        return;

                    var index = (int)Math.Round(Math.Min(value, 1.0) * (double)powerCurve.Count());
                    if (index >= powerCurve.Count)
                        index = powerCurve.Count - 1;

                    var rpm = powerCurve[index];

                    int msDelay;

                    if (rpm >= POWERCURVE_END_VALUE)
                        msDelay = 0; // which would be the NO LIMIT value
                    else
                        msDelay = (int)Math.Round(60000.0 / rpm);

                    if (Vendor != null)
                        Vendor.DelayMSBetweenVendorWebsiteRequests = msDelay;
                }
            }
        }

        /// <summary>
        /// Display approximate MS delay with current setting.
        /// </summary>
        private string _approxDelayMSText = null;
        public string ApproxDelayMSText
        {
            get
            {
                return _approxDelayMSText;
            }
            set
            {
                Set(() => ApproxDelayMSText, ref _approxDelayMSText, value);
            }
        }

        /// <summary>
        /// The text value displayed above the slider.
        /// </summary>
        private string _requestsPerMinuteText = null;
        public string RequestsPerMinuteText
        {
            get
            {
                return _requestsPerMinuteText;
            }
            set
            {
                Set(() => RequestsPerMinuteText, ref _requestsPerMinuteText, value);
            }
        }

        /// <summary>
        /// This is the true value which can be directly reversed back into a MS delay.
        /// </summary>
        private int _requestsPerMinute = 0;
        public int RequestsPerMinute
        {
            get
            {
                return _requestsPerMinute;
            }
            set
            {
                if (Set(() => RequestsPerMinute, ref _requestsPerMinute, value))
                {
                    // then update the text indicator

                    if (value == int.MaxValue)
                    {
                        RequestsPerMinuteText = "NO LIMIT";
                    }
                    else
                    {
                        RequestsPerMinuteText = string.Format("{0:N0}", value);
                    }
                }
            }
        }



        private string _resetDefaultRequestsPerMinuteTooltip = null;
        public string ResetDefaultRequestsPerMinuteTooltip
        {
            get
            {
                return _resetDefaultRequestsPerMinuteTooltip;
            }
            set
            {
                Set(() => ResetDefaultRequestsPerMinuteTooltip, ref _resetDefaultRequestsPerMinuteTooltip, value);
            }
        }

        private string _resetDefaultMaxErrorsCountTooltip = null;
        public string ResetDefaultMaxErrorsCountTooltip
        {
            get
            {
                return _resetDefaultMaxErrorsCountTooltip;
            }
            set
            {
                Set(() => ResetDefaultMaxErrorsCountTooltip, ref _resetDefaultMaxErrorsCountTooltip, value);
            }
        }

        #endregion

        #region Commands

        private RelayCommand _resetDefaultRequestsPerMinute;

        /// <summary>
        /// Gets the ResetDefaultRequestsPerMinute.
        /// </summary>
        public RelayCommand ResetDefaultRequestsPerMinute
        {
            get
            {
                return _resetDefaultRequestsPerMinute
                    ?? (_resetDefaultRequestsPerMinute = new RelayCommand(
                    () =>
                    {
                        Vendor.DelayMSBetweenVendorWebsiteRequests = Vendor.DefaultDelayMSBetweenVendorWebsiteRequests;
                    }));
            }
        }

        private RelayCommand _resetDefaultMaxErrorsCount;
        public RelayCommand ResetDefaultMaxErrorsCount
        {
            get
            {
                return _resetDefaultMaxErrorsCount
                    ?? (_resetDefaultMaxErrorsCount = new RelayCommand(
                    () =>
                    {
                        Vendor.MaximumScanningErrorCount = Vendor.DefaultMaximumScanningErrorCount;
                    }));
            }
        }
        #endregion


    }
}

