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
using Website.Services;

namespace ControlPanel.ViewModels
{
    public class LoginScreenViewModel : ValidatingViewModelBase
    {
        ControlPanelDomainContext ctx;

        public LoginScreenViewModel()
        {

            if (!ISControl.IsInDesignModeStatic)
            {
                LoginCommand = new DelegateCommand(ExecuteLoginCommand, CanExecuteLoginCommand);
                ClearMessages();
                AppService.Mediator.Register(this);
                ctx = new Website.Services.ControlPanelDomainContext();
            }
            else
            {
                SuccessMessage = "Please wait...";
                ErrorMessage = "Invalid password.";
            }
        }


        public DelegateCommand LoginCommand { get; private set; }

        private string password;

        public string Password
        {
            get { return password; }
            set
            {
                if (password != value)
                {
                    password = value;
                    RaisePropertyChanged(() => Password);
                }
            }
        }


        private string successMessage;

        public string SuccessMessage
        {
            get { return successMessage; }
            set
            {
                if (successMessage != value)
                {
                    successMessage = value;
                    RaisePropertyChanged(() => SuccessMessage);
                }
            }
        }
        
        private string errorMessage;

        public string ErrorMessage
        {
            get { return errorMessage; }
            set
            {
                if (errorMessage != value)
                {
                    errorMessage = value;
                    RaisePropertyChanged(() => ErrorMessage);
                }
            }
        }


        private void ClearMessages()
        {
            SuccessMessage = string.Empty;
            ErrorMessage = string.Empty;
        }


        private void IsValidPassword(string password)
        {
            ctx.IsAuthenticated(password,  (result) =>
                {
                    ClearMessages();

                    if (result.HasError)
                    {
                        result.MarkErrorAsHandled();
                        ErrorMessage = "Processing error.";
                        return;
                    }

                    // value is true for good password

                    if (result.Value)
                        AppService.Mediator.NotifyColleagues<object>(MediatorMessages.ApplicationLogin, null);
                    else
                        ErrorMessage = "Invalid password.";

                }, null);
        }

        #region Command Actions

        private bool CanExecuteLoginCommand(object parameter)
        {
            return true;
        }

        private void ExecuteLoginCommand(object parameter)
        {
            // password to match is "ControlPanelPassword" in web.config file

            ClearMessages();
            SuccessMessage = "Please wait...";
            AppSvc.Current.RunActionWithDelay(1500, () =>
                {
                    IsValidPassword(Password);
                });
        }


        #endregion

    }
}
