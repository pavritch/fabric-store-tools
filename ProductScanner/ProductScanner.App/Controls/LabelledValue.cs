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

namespace ProductScanner.App.Controls
{
    /// <summary>
    /// A simple helper control to hold an int value with corresponding string label.
    /// </summary>
    class LabelledValue : Control
    {
        public LabelledValue()
        {
            StringValue = "0";
        }

        #region Label Property
        public const string LabelPropertyName = "Label";

        public string Label
        {
            get
            {
                return (string)GetValue(LabelProperty);
            }
            set
            {
                SetValue(LabelProperty, value);
            }
        }

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            LabelPropertyName,
            typeof(string),
            typeof(LabelledValue),
            new UIPropertyMetadata(null)); 
        #endregion


        #region StringValue
        public const string StringValuePropertyName = "StringValue";

        public string StringValue
        {
            get
            {
                return (string)GetValue(StringValueProperty);
            }
            set
            {
                SetValue(StringValueProperty, value);
            }
        }

        public static readonly DependencyProperty StringValueProperty = DependencyProperty.Register(
            StringValuePropertyName,
            typeof(string),
            typeof(LabelledValue),
            new UIPropertyMetadata(null));

        #endregion


        #region Value Property
        public const string ValuePropertyName = "Value";

        public int Value
        {
            get
            {
                return (int)GetValue(ValueProperty);
            }
            set
            {
                SetValue(ValueProperty, value);
            }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            ValuePropertyName,
            typeof(int),
            typeof(LabelledValue),
            new UIPropertyMetadata(0, new PropertyChangedCallback(UpdateStringValue)));

            protected static void UpdateStringValue(DependencyObject d, DependencyPropertyChangedEventArgs e)
            {
                var value = (int)e.NewValue;
                (d as LabelledValue).StringValue = string.Format("{0:N0}", value);
            }

        #endregion

    }
}
