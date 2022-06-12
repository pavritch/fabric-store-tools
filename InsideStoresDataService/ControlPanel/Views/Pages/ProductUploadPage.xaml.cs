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
    public partial class ProductUploadPage : UXViewPage
    {
        public ProductUploadPage()
        {
            InitializeComponent();
            DataContext = new ControlPanel.ViewModels.ProductUploadPageViewModel();
        }


        private void RadUploadDropPanel1_DragEnter(object sender, DragEventArgs e)
        {
            Color backgroundColor = new Color();
            backgroundColor.R = 208;
            backgroundColor.G = 232;
            backgroundColor.B = 254;
            this.RadUploadDropPanel1.Background = new SolidColorBrush(backgroundColor);
        }

        private void RadUploadDropPanel_DragLeave(object sender, DragEventArgs e)
        {
            this.RadUploadDropPanel1.Background = new SolidColorBrush(Colors.White);
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ControlPanel.ViewModels.ProductUploadPageViewModel;
            if (vm == null)
                return;

            vm.SetRadUpload(RadUpload1);
        }

    }
}
