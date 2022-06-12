using System;
using System.Net;
using System.ServiceModel.DomainServices.Client;
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
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Intersoft.Client.UI.Controls;

namespace ControlPanel.Converters
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
            bool v = (bool)value;
            return v ? Visibility.Visible : Visibility.Collapsed;
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
            bool v = (bool)value;
            return v ? Visibility.Collapsed : Visibility.Visible;
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
    /// A type converter for UXCallout.   
    /// </summary>   
    /// <remarks>
    /// Used on website cart items. For cards on the left, the large thumb
    /// popup should be to the left, and visa versa.
    /// </remarks>
    public class PopupPositionConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            var index = (int)value;
            if ((index & 1) == 1)
                return PopupPosition.Left;
            return PopupPosition.Right;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    /// <summary>   
    /// A type converter for UXCallout.   
    /// </summary> 
    /// <remarks>
    /// The galleries have 4 items accross. The left 2 cols should popup
    /// the large thumb on the right, and visa versa.
    /// </remarks>
    public class PopupPositionThumbGalleryConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            // the value passed in is the 0-rel index of the item in the collection

            var index = (int)value;

            var column = index % 4;
            if (column < 2)
                return PopupPosition.Right;
            return PopupPosition.Left;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>   
    /// Convert dental market groups.   
    /// </summary>   
    public class DentalMarketGroupConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            string str = (string)value;

            if (string.Equals(str, "Specialist", StringComparison.OrdinalIgnoreCase))
                return "Board-Certified Specialist";

            return "General Practitioner";
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            throw new NotImplementedException("DentalMarketGroupConverter");
        }
    }


    /// <summary>   
    /// Convert an indent value into a width or margin.  
    /// </summary>   
    /// <remarks>
    /// Used on the dental marketing checklist page to indent on the 
    /// left and then shorten the width by same amount so the right
    /// margin of all text aligns.
    /// </remarks>
    public class DentalMarketIndentConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            bool bWidthMinusIndent = false;
            int width = 0;

            var indent = (int)value;

            if (parameter != null)
                bWidthMinusIndent = int.TryParse((string)parameter, out width);

            switch (indent)
            {
                case 1:
                    indent = 32;
                    break;

                default:
                    indent = 0;
                    break;
            }

            if (bWidthMinusIndent)
                return width - indent;

            return indent;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            throw new NotImplementedException("DentalMarketIndentConverter");
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
    /// IsAdvanced (search) color converter.
    /// </summary>   
    /// <remarks>
    /// Blue for advanced, green for simple search.
    /// </remarks>
    public class IsAdvancedSearchBrushConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            const string GREEN = "#FF8EC441";
            const string BLUE = "#FF1B9DDE";

            bool v = (bool)value;
            return v ? new SolidColorBrush(BLUE.ToColor()) : new SolidColorBrush(GREEN.ToColor());
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
