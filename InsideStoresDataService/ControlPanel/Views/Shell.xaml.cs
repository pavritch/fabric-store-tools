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
using Intersoft.Client.UI.Navigation;
using Intersoft.Client.UI.Controls;
using System.Diagnostics;
using ControlPanel.ViewModels;

namespace ControlPanel.Views
{
    public partial class Shell : UserControl
    {
        public Shell()
        {
            InitializeComponent();
            DataContext = new ControlPanel.ViewModels.ShellViewModel();
        }

        private void sizeChanged(object sender, SizeChangedEventArgs e)
        {
            Debug.WriteLine("Size changed.");
            if (e.NewSize.Height < 200)
                return;

            if (e.NewSize.Width < 400)
                return;

            //if ((mainTree != null) && (e.NewSize.Height != 0) && (mainTree.Height != e.NewSize.Height - 145))
            //{
            //    mainTree.Height = e.NewSize.Height - 193;
            //    if (!double.IsNaN(PageContentContainer.ActualWidth))
            //    {
            //        PageContentContainer.Clip = new GeometryGroup()
            //        {
            //            FillRule = FillRule.EvenOdd,
            //            Children = new GeometryCollection() { 
            //            new RectangleGeometry() { Rect = new Rect( -300, -10, this.ActualWidth + 500, this.ActualHeight ) },
            //            new RectangleGeometry() { Rect = new Rect( -300, -10, TreeBorder.ActualWidth + 52, this.ActualHeight ) }
            //        }
            //        };
            //    }
            //}
        }


    }
}
