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
using ControlPanel.Views;

namespace ControlPanel
{
    public interface IViewModelPageNavigation
    {
        void OnNavigatedTo(UXViewPage page, NavigationEventArgs e);
        void OnNavigatingFrom(NavigatingCancelEventArgs e);
    }
}
