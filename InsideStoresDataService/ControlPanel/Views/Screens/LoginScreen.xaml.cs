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
using ControlPanel.ViewModels;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace ControlPanel.Views
{
    public partial class LoginScreen : UXScreen
    {
        public LoginScreen()
        {
            InitializeComponent();
            DataContext = new ControlPanel.ViewModels.LoginScreenViewModel();
        }

        public override void OnNavigatingFrom()
        {
            Password.Text = string.Empty;
        }

    }
}
