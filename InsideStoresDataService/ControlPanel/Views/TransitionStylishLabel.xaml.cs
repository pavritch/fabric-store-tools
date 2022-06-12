using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Intersoft.Client.UI.Controls;
using System.Windows.Data;
using Intersoft.Client.Framework;

namespace ControlPanel.Views
{
    public partial class TransitionStylishLabel : UserControl
    {
        private double savedWidth;
        //private double savedHeight;

        public TransitionStylishLabel()
        {
            InitializeComponent();
            savedWidth = 650;
        }

#if false
        #region FontFamily (DependencyProperty)

        /// <summary>
        /// FontFamily
        /// </summary>
        public FontFamily FontFamily
        {
            get { return (FontFamily)GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }
        public static readonly DependencyProperty FontFamilyProperty =
            DependencyProperty.Register("FontFamily", typeof(FontFamily), typeof(TransitionStylishLabel),
              new PropertyMetadata(null));

        #endregion


        #region FontSize (DependencyProperty)

        /// <summary>
        /// Font size.
        /// </summary>
        public Double FontSize
        {
            get { return (Double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }
        public static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register("FontSize", typeof(Double), typeof(TransitionStylishLabel),
              new PropertyMetadata(0));

        #endregion



        #region Foreground (DependencyProperty)

        /// <summary>
        /// Foreground
        /// </summary>
        public Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }
        public static readonly DependencyProperty ForegroundProperty =
            DependencyProperty.Register("Foreground", typeof(Brush), typeof(TransitionStylishLabel),
              new PropertyMetadata(null));
        #endregion




        


        #region FontWeight (DependencyProperty)

        /// <summary>
        /// FontWeight
        /// </summary>
        public FontWeight FontWeight
        {
            get { return (FontWeight)GetValue(FontWeightProperty); }
            set { SetValue(FontWeightProperty, value); }
        }
        public static readonly DependencyProperty FontWeightProperty =
            DependencyProperty.Register("FontWeight", typeof(FontWeight), typeof(TransitionStylishLabel),
              new PropertyMetadata(null));

        #endregion



        #region FontStyle (DependencyProperty)

        /// <summary>
        /// FontStyle
        /// </summary>
        public FontStyle FontStyle
        {
            get { return (FontStyle)GetValue(FontStyleProperty); }
            set { SetValue(FontStyleProperty, value); }
        }
        public static readonly DependencyProperty FontStyleProperty =
            DependencyProperty.Register("FontStyle", typeof(FontStyle), typeof(TransitionStylishLabel),
              new PropertyMetadata(null));

        #endregion

#endif

        #region Text (DependencyProperty)

        /// <summary>
        /// Text
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TransitionStylishLabel),
            new PropertyMetadata(null, new PropertyChangedCallback(OnTextChanged)));

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TransitionStylishLabel)d).OnTextChanged(e);
        }

        protected virtual void OnTextChanged(DependencyPropertyChangedEventArgs e)
        {
            UpdateContent();
        }

        #endregion


        #region TextTrimming (DependencyProperty)

        /// <summary>
        /// TextTrimming
        /// </summary>
        public TextTrimming TextTrimming
        {
            get { return (TextTrimming)GetValue(TextTrimmingProperty); }
            set { SetValue(TextTrimmingProperty, value); }
        }
        public static readonly DependencyProperty TextTrimmingProperty =
            DependencyProperty.Register("TextTrimming", typeof(TextTrimming), typeof(TransitionStylishLabel),
              new PropertyMetadata(TextTrimming.None));

        #endregion



        #region TextWrapping (DependencyProperty)

        /// <summary>
        /// TextWrapping
        /// </summary>
        public TextWrapping TextWrapping
        {
            get { return (TextWrapping)GetValue(TextWrappingProperty); }
            set { SetValue(TextWrappingProperty, value); }
        }
        public static readonly DependencyProperty TextWrappingProperty =
            DependencyProperty.Register("TextWrapping", typeof(TextWrapping), typeof(TransitionStylishLabel),
              new PropertyMetadata(TextWrapping.NoWrap));

        #endregion


        private void UpdateContent()
        {
            var label = new TextBlock();

            // FontFamily
            var bindFontFamily = new Binding("FontFamily")
            {
                Source = this,
                Mode = BindingMode.OneWay,
            };
            label.SetBinding(TextBlock.FontFamilyProperty, bindFontFamily);

            // FontSize
            var bindFontSize = new Binding("FontSize")
            {
                Source = this,
                Mode = BindingMode.OneWay,
            };
            label.SetBinding(TextBlock.FontSizeProperty, bindFontSize);

            // Foreground
            var bindForeground = new Binding("Foreground")
            {
                Source = this,
                Mode = BindingMode.OneWay,
            };
            label.SetBinding(TextBlock.ForegroundProperty, bindForeground);

            // FontStyle
            var bindFontStyle = new Binding("FontStyle")
            {
                Source = this,
                Mode = BindingMode.OneWay,
            };
            label.SetBinding(TextBlock.FontStyleProperty, bindFontStyle);

            // Text
            var bindText = new Binding("Text")
            {
                Source = this,
                Mode = BindingMode.OneWay,
            };
            label.SetBinding(TextBlock.TextProperty, bindText);


            // FontWeight
            var bindFontWeight = new Binding("FontWeight")
            {
                Source = this,
                Mode = BindingMode.OneWay,
            };
            label.SetBinding(TextBlock.FontWeightProperty, bindFontWeight);

            // TextWrapping
            var bindTextWrapping = new Binding("TextWrapping")
            {
                Source = this,
                Mode = BindingMode.OneWay,
            };
            label.SetBinding(TextBlock.TextWrappingProperty, bindTextWrapping);

            // TextTrimming
            var bindTextTrimming = new Binding("TextTrimming")
            {
                Source = this,
                Mode = BindingMode.OneWay,
            };
            label.SetBinding(TextBlock.TextTrimmingProperty, bindTextTrimming);

            label.Width = savedWidth;
            //label.Height = savedHeight;
            TransitionCtl.Content = label;

        }

    }
}
