using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Intersoft.Client.UI.Navigation;

namespace ControlPanel.Views
{
    public class UXViewPage : UXPage
    {

        // Executes when the user navigates to this page.
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var vm = DataContext as IViewModelPageNavigation;
            if (vm != null)
                vm.OnNavigatedTo(this, e);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            var vm = DataContext as IViewModelPageNavigation;
            if (vm != null)
                vm.OnNavigatingFrom(e);
        }
    }
}
