using System;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Data;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using ProductScanner.Core.PlatformEntities;
using ProductScanner.Core.Scanning.EventLogs;
using Telerik.Windows.Controls.TransitionControl;
using Utilities;
using System.Diagnostics;

namespace ProductScanner.App
{

    /// <summary>   
    /// A type converter for visibility and bool values.   
    /// </summary>   
    public class BoolVisibilityConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            Visibility nonVisibleSetting = Visibility.Collapsed;
            if (parameter != null)
            {
                var pValue = parameter as string;
                if (string.Equals(parameter, Visibility.Hidden.ToString()))
                    nonVisibleSetting = Visibility.Hidden;
            }

            bool v = (bool)value;
            return v ? Visibility.Visible : nonVisibleSetting;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            Visibility visibility = (Visibility)value;
            return visibility == Visibility.Visible;
        }
    }

    /// <summary>   
    /// A type converter for visibility and bool values.   
    /// </summary>   
    public class InverseBoolVisibilityConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {

            Visibility nonVisibleSetting = Visibility.Collapsed;
            if (parameter != null)
            {
                var pValue = parameter as string;
                if (string.Equals(parameter, Visibility.Hidden.ToString()))
                    nonVisibleSetting = Visibility.Hidden;
            }


            bool v = (bool)value;
            return v ? nonVisibleSetting : Visibility.Visible;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            Visibility visibility = (Visibility)value;
            return visibility == Visibility.Collapsed;
        }
    }


    /// <summary>   
    /// Returns the inverse of the bool input.   
    /// </summary>   
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {

            bool v = (bool)value;
            return !v;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            throw new NotImplementedException("Cannot convert back.");
        }
    }



    /// <summary>   
    /// A type converter for visibility based on if string has a value.   
    /// </summary>   
    public class StringVisibilityConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            string v = (string)value;
            return !string.IsNullOrWhiteSpace(v) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            throw new NotImplementedException("StringVisibilityConverter");
        }
    }



    /// <summary>   
    /// Convert nulls to empty string.   
    /// </summary>   
    public class NullToEmptyStringConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            string str = (string)value;
            return str == null ? string.Empty : str;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            string str = (string)value;
            return string.IsNullOrWhiteSpace(str) ? null : str;
        }
    }


    /// <summary>   
    /// Convert nulls to empty string.   
    /// </summary>   
    public class NullToCharsStringConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            string str = (string)value;
            string param = (string)parameter;
            return str == null ? param : str;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            string str = (string)value;
            string param = (string)parameter;
            return (string.IsNullOrWhiteSpace(str) || str == param) ? null : str;
        }
    }


    /// <summary>   
    /// Convert nulls to empty string.   
    /// </summary>   
    public class NullOrWhiteSpaceToCharsStringConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            string str = (string)value;
            string param = (string)parameter;
            return string.IsNullOrWhiteSpace(str) ? param : str;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            string str = (string)value;
            string param = (string)parameter;
            return (string.IsNullOrWhiteSpace(str) || str == param) ? null : str;
        }
    }

    /// <summary>
    /// Given an object, return its FullName of the type.
    /// </summary>
    /// <remarks>
    /// For example: ProductScanner.App.NotImplementedVendorModel
    /// </remarks>
    public class ObjectToTypeStringConverter : IValueConverter
    {
        public object Convert(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            return value.GetType().FullName;
        }

        public object ConvertBack(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("Can't convert back");
        }
    }


    /// <summary>
    /// Used in treeview to disable vendor nodes for vendors which are not yet fully supported.
    /// </summary>
    public class IsNotFullyImplementedConverter : IValueConverter
    {
        public object Convert(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            var vendor = value as IVendorModel;
            if (vendor == null)
                return false;

            return !vendor.IsFullyImplemented;
        }

        public object ConvertBack(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("Can't convert back");
        }
    }

    /// <summary>
    /// Returns true if the object implements the indicated interface.
    /// </summary>
    public class ImplementsInterfaceConverter : IValueConverter
    {
        public object Convert(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            // example:
            // <DataTrigger Binding="{Binding Converter={StaticResource ImplementsInterfaceConverter}, ConverterParameter={x:Type local:IVendorModel}}" Value="True" >

            if (!(parameter is Type))
                throw new Exception("Parameter must be an interface type.");

            if ((parameter as Type).IsAssignableFrom(value.GetType()))
                return true;

            return false;
        }

        public object ConvertBack(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("Can't convert back");
        }
    }



    /// <summary>
    /// Returns an icon image for the associated ScanState.
    /// </summary>
    /// <remarks>
    /// Takes optional paremeter which is the icon size.
    /// </remarks>
    public class ScanStateIconConverter : IValueConverter
    {
        public object Convert(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            // example:
            // Binding="{Binding Converter={StaticResource ScanStateIconConverter}, ConverterParameter=16}"

            try
            {
                string url = null;
                var state = (ScannerState)value;
                var size = parameter as string;

                url = string.Format("pack://application:,,,/ProductScanner.App;component/Assets/Images/ScannerState/{0}{1}.png", state, size);

                var isc = new ImageSourceConverter();

                // example url needed
               // "pack://application:,,,/ProductScanner.App;component/Assets/Images/ScannerState/Scanning16.png";

                var img = isc.ConvertFromString(url);

                return img;

            }
            catch
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("Can't convert back");
        }
    }


    /// <summary>
    /// Returns an icon image for the associated ScanningStatus.
    /// </summary>
    public class ScanningStatusIconConverter : IValueConverter
    {
        public object Convert(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
   
            try
            {
                string url = null;
                var pageStatus = (ProductScanner.App.ScanningStatus)value;

                int iconSize = 24;
                int.TryParse(parameter as string, out iconSize);

                url = string.Format("pack://application:,,,/ProductScanner.App;component/Assets/Images/ScanningStatus/{0}{1}.png", pageStatus, iconSize);

                var isc = new ImageSourceConverter();

                var img = isc.ConvertFromString(url);

                return img;

            }
            catch
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("Can't convert back");
        }
    }


    public class CommitBatchStatusIconConverter : IValueConverter
    {
        public object Convert(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                string url = null;

                var size = parameter as string;

                url = string.Format("pack://application:,,,/ProductScanner.App;component/Assets/Images/CommitBatchStatus/{0}{1}.png", (CommitBatchStatus)value, size);

                var isc = new ImageSourceConverter();

                // example url needed
                // "pack://application:,,,/ProductScanner.App;component/Assets/Images/ScannerState/Scanning16.png";

                var img = isc.ConvertFromString(url);

                return img;

            }
            catch
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("Can't convert back");
        }
    }

    public class ScannerStatusColorConverter : IValueConverter
    {
        public object Convert(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                var status = value as string;
                string rgb;

                switch(status)
                {
                    case "Scanning":
                        rgb = "#FF8EC441";
                        break;

                    case "Failed":
                        rgb = "#FFCB0F6D";
                        break;

                    default:
                        rgb = "#FF808080";
                        break;
                }

                return rgb.ToSolidBrush();
            }
            catch
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("Can't convert back");
        }
    }

    public class StartScanningButtonBackgroundConverter : IValueConverter
    {
        public object Convert(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                var vendor = value as IVendorModel;

                if (!vendor.HasPendingCommitBatches)
                    return DependencyProperty.UnsetValue;

                string rgb = "#18C65F8C";

                return rgb.ToSolidBrush();
            }
            catch
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("Can't convert back");
        }
    }



    public class ActivityResultIconConverter : IValueConverter
    {
        public object Convert(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                var activityResult = (ActivityResult)value;
                string url = null;

                url = string.Format("pack://application:,,,/ProductScanner.App;component/Assets/Images/ActivityResult/{0}.png", activityResult);

                var isc = new ImageSourceConverter();

                var img = isc.ConvertFromString(url);

                return img;
            }
            catch
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("Can't convert back");
        }
    }


    /// <summary>
    /// Returns the icon to display depending if bool is true or false.
    /// </summary>
    /// <remarks>
    /// Returns empty for null so nothing displayed.
    /// </remarks>
    public class SuccessFailIconConverter : IValueConverter
    {
        public object Convert(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                var v = (bool?)value;

                if (!v.HasValue)
                    return DependencyProperty.UnsetValue;

                var size = parameter as string; // 16 or 24
                var filename = v.Value ? "GreenCheckRound" : "Error";
                
                var url = string.Format("pack://application:,,,/ProductScanner.App;component/Assets/Images/{0}{1}.png", filename, size);

                var isc = new ImageSourceConverter();
                var img = isc.ConvertFromString(url);

                return img;

            }
            catch
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("Can't convert back");
        }
    }




    /// <summary>
    /// Takes a progress 0-100 percent and turns into an angle 0-360.
    /// </summary>
    public class ProgressToAngleConverter : System.Windows.Data.IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double progress = (double)values[0];
            System.Windows.Controls.ProgressBar bar = values[1] as System.Windows.Controls.ProgressBar;

            return 359.999 * (progress / (bar.Maximum - bar.Minimum));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    /// <summary>
    /// This converter translates between our short list of known transitions and the various values needed
    /// for transition, duration and easing used by the telerik control.
    /// </summary>
    /// <remarks>
    /// Our values for ContentPageTransition are unaware of durations, easing, etc.
    /// The parameter passed in is a string which indicates which property we're looking for given the current effect.
    /// </remarks>
    public class PageTransitionToTelerikTransitionConverter : IValueConverter
    {
        public object Convert(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                var transitionType = (ContentPageTransition)value;
                var targetProperty = parameter as string;

                switch(transitionType)
                {
                    // no transition
                    case ContentPageTransition.None:
                        return DependencyProperty.UnsetValue;

                    // properties for fade transition
                    case ContentPageTransition.Fade:
                        switch (targetProperty)
                        {
                            case "Transition":
                                return new Telerik.Windows.Controls.TransitionEffects.FadeTransition();

                            case "Duration":
                                return TimeSpan.FromMilliseconds(200);

                            case "Easing":
                                return DependencyProperty.UnsetValue;

                            default:
                                // developer error if ever gets here.
                                return DependencyProperty.UnsetValue;
                        }

                    // properties for zoom transition
                    case ContentPageTransition.Zoom:
                        switch (targetProperty)
                        {
                            case "Transition":
                                return new Telerik.Windows.Controls.TransitionEffects.MotionBlurredZoomTransition();

                            case "Duration":
                                return TimeSpan.FromMilliseconds(100);

                            case "Easing":
                                return DependencyProperty.UnsetValue;

                            default:
                                // developer error if ever gets here.
                                return DependencyProperty.UnsetValue;

                        }

                    default:
                        // developer error if ever gets here.
                        return DependencyProperty.UnsetValue;
                }
            }
            catch
            {
                // developer error if ever gets here.
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("Can't convert back");
        }
    }


    /// <summary>   
    /// A type converter making a string all upper or lower case   
    /// </summary>
    /// <remarks>
    /// Set parameter to ToUpper or ToLower.
    /// </remarks>
    public class StringLettercaseConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            string v = value as string;
            if (v == null)
                return DependencyProperty.UnsetValue;

            bool isToLowerCase = (parameter as string).Equals("ToLower", StringComparison.OrdinalIgnoreCase);

            bool isToUpperCase = (parameter as string).Equals("ToUpper", StringComparison.OrdinalIgnoreCase);

            if (isToLowerCase)
                return v.ToLower();

            if (isToUpperCase)
                return v.ToUpper();

            // else no transform

            return v;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            throw new NotImplementedException("Can't convert back");
        }
    }


    /// <summary>
    /// Convert the int input value to N thousands.
    /// </summary>
    /// <remarks>
    /// For lower numbers, reports a single decimal digit, else whole numbers.
    /// Does not include commas since intended to be used in ring labels.
    /// Intended mostly for displaying product counts.
    /// </remarks>
    public class ThousandsConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            try
            {
                int number = (int)value;

                if (number < 9901)
                {
                    // result should include a single decimal place digit
                    return Math.Round(number / 1000M, 1).ToString();
                }
                else
                {
                    // big numbers get rounded whole numbers
                    return Math.Round(number / 1000M).ToString();
                }
            }
            catch
            {
                return DependencyProperty.UnsetValue;

            }
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            throw new NotImplementedException("Can't convert back");
        }
    }


    public class EnumDescriptionConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            try
            {

                return value.DescriptionAttr();
            }
            catch
            {
                return DependencyProperty.UnsetValue;

            }
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            throw new NotImplementedException("Can't convert back");
        }
    }


    public class ZeroPaddingConverter : IValueConverter
    {
        public object Convert(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                var number = (int)value;
                var paddingCount = int.Parse(parameter as string);

                var zeros = new StringBuilder();
                for (int i = 0; i < paddingCount; i++)
                    zeros.Append("0");

                var fmtString = "{0:" + zeros.ToString() + "}";
                return string.Format(fmtString, number);
            }
            catch
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("Can't convert back");
        }
    }


    public class MaxValueVisibilityConverter : IValueConverter
    {
        public object Convert(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                var number = (int)value;
                var maxNumber = int.Parse(parameter as string);

                return (number > maxNumber) ? Visibility.Collapsed : Visibility.Visible;
            }
            catch
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("Can't convert back");
        }
    }

    public class MaxValueInverseVisibilityConverter : IValueConverter
    {
        public object Convert(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                var number = (int)value;
                var maxNumber = int.Parse(parameter as string);

                return (number > maxNumber) ? Visibility.Visible : Visibility.Collapsed;
            }
            catch
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("Can't convert back");
        }
    }

    public class ScanLogEventColorConverter : IValueConverter
    {
        public object Convert(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                var eventType = (EventType)value;
                string rgb;

                switch (eventType)
                {
                    case EventType.Error:
                        rgb = "#FFC03B3B";
                        break;

                    case EventType.Warning:
                        rgb = "#FF3B80C0";
                        break;

                    case EventType.Success:
                        rgb = "#FF8ec441";
                        break;

                    default:
                        rgb = "#FF707070";
                        break;
                }

                return rgb.ToSolidBrush();
            }
            catch
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(
         object value, Type targetType,
         object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("Can't convert back");
        }
    }




// saved snippet 
//var blur = new BlurEffect(); 
//var current = this.Background; 
//blur.Radius = 5; 
//this.Background = new SolidColorBrush(Colors.DarkGray); 
//this.Effect = blur; 
//MessageBox.Show("hello"); 
//this.Effect = null; 
//this.Background = current;


}
