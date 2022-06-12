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
    /// Icon logic for vendor test graphical representation.
    /// </summary>
    public partial class TestStatusIcon : RadTransitionControl
    {
        public TestStatusIcon()
        {
            Duration = TimeSpan.FromMilliseconds(150);
            Transition = new Telerik.Windows.Controls.TransitionEffects.FadeTransition();
            SetContent(Status);
        }

        #region Status Property

        public const string StatusPropertyName = "Status";

        public TestStatus Status
        {
            get
            {
                return (TestStatus)GetValue(StatusProperty);
            }
            set
            {
                SetValue(StatusProperty, value);
            }
        }

        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(
            StatusPropertyName,
            typeof(TestStatus),
            typeof(TestStatusIcon),
            new UIPropertyMetadata(TestStatus.Unknown, new PropertyChangedCallback(StatusChanged)));

        protected static void StatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (TestStatus)e.NewValue;
            var ctrl = (TestStatusIcon)d;
            ctrl.SetContent(value);
        }

        #endregion

        #region Local Methods

        private void SetContent(TestStatus status)
        {
            FrameworkElement content = null;

            if (status == TestStatus.Running)
            {
                content = new Spinner16();
            }
            else
            {
                content = new System.Windows.Controls.Image()
                {
                    Width = 16,
                    Height = 16,
                    Source = MakeImageSource(status)
                };
            }

            Content = content;
        }


        private static System.Windows.Media.ImageSource MakeImageSource(TestStatus status)
        {
            var isc = new ImageSourceConverter();
            var url = string.Format("pack://application:,,,/ProductScanner.App;component/Assets/Images/TestStatus/{0}{1}.png", status, 16);
            var imgSrc = isc.ConvertFromString(url) as System.Windows.Media.ImageSource;
            return imgSrc;
        }

        #endregion
    }
}
