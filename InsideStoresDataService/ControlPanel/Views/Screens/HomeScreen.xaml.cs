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
using System.Diagnostics;
using Intersoft.Client.UI.Controls;

namespace ControlPanel.Views
{
    public partial class HomeScreen : UXScreen
    {
        private bool isLoaded = false;

        public HomeScreen()
        {
            InitializeComponent();
            AppSvc.Current.PageContentFrame = ContentFrame;
            DataContext = new ControlPanel.ViewModels.HomeScreenViewModel();

            ContentFrame.Loaded += (s, e) =>
                {
                    if (isLoaded)
                        return;

                    ContentFrame.SetContent(new ControlPanel.Views.BlankPage(), false);
                    isLoaded = true;
                };
        }

        private void ContentFrame_ResolveNavigationDirection(object sender, NavigationDirectionChangedEventArgs e)
        {
            string uri = e.Uri.ToString().ToLower();

            // if blank, then logging out

            if (uri == "/blank")
            {
                e.ResolvedTransitionDirection = TransitionContentDirection.Back;
                return;
            }

            e.ResolvedTransitionDirection = TransitionContentDirection.Default;
        }

   }
}
