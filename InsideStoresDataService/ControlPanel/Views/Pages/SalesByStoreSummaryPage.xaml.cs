using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Intersoft.Client.UI.Navigation;
using Website.Services;
using System.ServiceModel.DomainServices.Client;

namespace ControlPanel.Views
{
    public partial class SalesByStoreSummaryPage : UXViewPage
    {
        public SalesByStoreSummaryPage()
        {
            InitializeComponent();
            DataContext = new ControlPanel.ViewModels.SalesByStoreSummaryPageViewModel();
        }

    }
}
