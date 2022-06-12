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

using MEFedMVVM.ViewModelLocator;
using Intersoft.Client.Framework;
using Intersoft.Client.Framework.Input;
using Intersoft.Client.UI.Aqua;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations;
using MEFedMVVM.Services.CommonServices;
using System.ServiceModel.DomainServices.Client;
using Intersoft.Client.UI.Navigation;
using System.ComponentModel.Composition;

namespace ControlPanel.ViewModels
{
    public class ShellViewModel : ValidatingViewModelBase
    {
        public ShellViewModel()
        {
            if (!ISControl.IsInDesignModeStatic)
            {
                AppService.Mediator.Register(this);
            }
            
            // always start with the login screen
            ScreenContent = LoginScreen;
        }

        private UserControl loginScreen;
        private UserControl LoginScreen
        {
            get
            {
                if (loginScreen == null)
                    loginScreen = new ControlPanel.Views.LoginScreen();

                return loginScreen;
            }
        }

        private UserControl homeScreen;
        private UserControl HomeScreen
        {
            get
            {
                if (homeScreen == null)
                    homeScreen = new ControlPanel.Views.HomeScreen();

                return homeScreen;
            }
        }


        private UserControl screenContent;

        public UserControl ScreenContent
        {
            get { return screenContent; }
            set
            {
                if (screenContent != value)
                {
                    screenContent = value;
                    RaisePropertyChanged(() => ScreenContent);
                }
            }
        }

        private void SignalNavigateAway()
        {
            var screen = ScreenContent as IUXScreen;
            if (screen == null)
                return;

            screen.OnNavigatingFrom();
        }

        private void ChangeScreens(UserControl ctl)
        {
            SignalNavigateAway();
            AppService.Mediator.NotifyColleagues<object>(MediatorMessages.BeginNavigateToScreen, null);
            ScreenContent = ctl;
            AppService.Mediator.NotifyColleagues<object>(MediatorMessages.EndNavigateToScreen, null);                
        }

        [MediatorMessageSink(MediatorMessages.ApplicationLogin, ParameterType = typeof(object))]
        public void OnApplicationLogin(object o)
        {
            ChangeScreens(HomeScreen);
            AppService.Mediator.NotifyColleagues<object>(MediatorMessages.DisplayHomeScreen, null);
        }

        [MediatorMessageSink(MediatorMessages.ApplicationLogout, ParameterType = typeof(object))]
        public void OnApplicationLogout(object o)
        {
            ChangeScreens(LoginScreen);
            AppService.Mediator.NotifyColleagues<object>(MediatorMessages.DisplayLoginScreen, null);
        }
    }
}
