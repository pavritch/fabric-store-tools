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
    /// Interaction logic for RingLabel.xaml
    /// </summary>
    public partial class RingLabel : UserControl
    {
        public RingLabel()
        {
            InitializeComponent();
        }

        #region RingColor Property
        /// <summary>
        /// The <see cref="RingColor" /> dependency property's name.
        /// </summary>
        public const string RingColorPropertyName = "RingColor";

        public Brush RingColor
        {
            get
            {
                return (Brush)GetValue(RingColorProperty);
            }
            set
            {
                SetValue(RingColorProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="RingColor" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty RingColorProperty = DependencyProperty.Register(
            RingColorPropertyName,
            typeof(Brush),
            typeof(RingLabel),
            new UIPropertyMetadata(Brushes.Blue));
        
        #endregion

        #region Text Property
		        /// <summary>
        /// The <see cref="Text" /> dependency property's name.
        /// </summary>
        public const string TextPropertyName = "Text";

        /// <summary>
        /// Gets or sets the value of the <see cref="Text" />
        /// property. This is a dependency property.
        /// </summary>
        public string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }
            set
            {
                SetValue(TextProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="Text" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            TextPropertyName,
            typeof(string),
            typeof(RingLabel),
            new UIPropertyMetadata("??"));
 
	    #endregion 

        #region RingThickness Property
        /// <summary>
        /// The <see cref="RingThickness" /> dependency property's name.
        /// </summary>
        public const string RingThicknessPropertyName = "RingThickness";

        /// <summary>
        /// Gets or sets the value of the <see cref="RingThickness" />
        /// property. This is a dependency property.
        /// </summary>
        public int RingThickness
        {
            get
            {
                return (int)GetValue(RingThicknessProperty);
            }
            set
            {
                SetValue(RingThicknessProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="RingThickness" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty RingThicknessProperty = DependencyProperty.Register(
            RingThicknessPropertyName,
            typeof(int),
            typeof(RingLabel),
            new UIPropertyMetadata(15)); 
        #endregion

        #region XOffset Property
        /// <summary>
        /// The <see cref="XOffset" /> dependency property's name.
        /// </summary>
        public const string XOffsetPropertyName = "XOffset";

        /// <summary>
        /// Gets or sets the value of the <see cref="XOffset" />
        /// property. This is a dependency property.
        /// </summary>
        public int XOffset
        {
            get
            {
                return (int)GetValue(XOffsetProperty);
            }
            set
            {
                SetValue(XOffsetProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="XOffset" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty XOffsetProperty = DependencyProperty.Register(
            XOffsetPropertyName,
            typeof(int),
            typeof(RingLabel),
            new UIPropertyMetadata(0));
        
        #endregion

        #region YOffset Property
        /// <summary>
        /// The <see cref="YOffset" /> dependency property's name.
        /// </summary>
        public const string YOffsetPropertyName = "YOffset";

        /// <summary>
        /// Gets or sets the value of the <see cref="YOffset" />
        /// property. This is a dependency property.
        /// </summary>
        public int YOffset
        {
            get
            {
                return (int)GetValue(YOffsetProperty);
            }
            set
            {
                SetValue(YOffsetProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="YOffset" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty YOffsetProperty = DependencyProperty.Register(
            YOffsetPropertyName,
            typeof(int),
            typeof(RingLabel),
            new UIPropertyMetadata(0));
        
        #endregion
    }
}
