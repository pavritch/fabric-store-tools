using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows;
using Telerik.Windows.Controls;
using System.Windows.Controls;

namespace ProductScanner.App.Controls
{
    /// <summary>
    /// Similar to a normal textblock, but fades between text.
    /// </summary>
    public class FaderTextBlock : RadTransitionControl
    {
        public FaderTextBlock()
        {
            Duration = TimeSpan.FromMilliseconds(100);
            Transition = new Telerik.Windows.Controls.TransitionEffects.FadeTransition();
            SetContent(null);
        }

        public const string TextPropertyName = "Text";
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

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            TextPropertyName,
            typeof(string),
            typeof(FaderTextBlock),
            new UIPropertyMetadata(null, new PropertyChangedCallback(TextChanged)));

        protected static void TextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (string)e.NewValue;
            var ctrl = (FaderTextBlock)d;
            ctrl.SetContent(value);
        }

        private void SetContent(string text)
        {
            var tb = new TextBlock()
            {
                Text = text ?? string.Empty,
            };

            this.Content = tb;
        }
            
    }
}
