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

namespace ControlPanel.Views
{
    public partial class PageViewsPage : UXViewPage
    {
        public PageViewsPage()
        {
            InitializeComponent();
            DataContext = new ControlPanel.ViewModels.PageViewsPageViewModel();
        }

    }
}
