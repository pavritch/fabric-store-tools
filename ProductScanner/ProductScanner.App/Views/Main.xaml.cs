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
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;

namespace ProductScanner.App.Views
{
    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main : UserControl
    {
        public Main()
        {
            InitializeComponent();

            Loaded += Main_Loaded;
        }

        void Main_Loaded(object sender, RoutedEventArgs e)
        {
            if (!ViewModelBase.IsInDesignModeStatic)
            {
                // Announce we're up and running
                AppReady();
            }
        }

        private void Logo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            GoHome();
        }

        private void AppReady()
        {
            Messenger.Default.Send(new AnnouncementMessage(Announcement.ApplicationReady));
        }

        private void GoHome()
        {
            Messenger.Default.Send(new RequestNavigationToContentPageType(ContentPageTypes.Home));
        }

    }
}
