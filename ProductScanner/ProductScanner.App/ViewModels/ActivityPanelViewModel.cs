using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Threading;

namespace ProductScanner.App.ViewModels
{
    public class ActivityPanelViewModel : ViewModelBase
    {

        public ActivityPanelViewModel()
        {
#if DEBUG
            if (IsInDesignMode)
            {
                var designActivity = new ActivityRequest()
                {
                    Caption = "Please Confirm",
                    Title = "This Is My Title",
                    IsIndeterminateProgress = false,
                    PercentComplete = 67,
                    IsCancellable = false,
                    StatusMessage = "This is my status message.",
                    CompletedResult = ActivityResult.Success,
                };

                ActivityRequest = designActivity;
                IsShowing = true;
                ShowProgressBar = true;
                return;
            }
#endif

            if (!ViewModelBase.IsInDesignModeStatic)
            {
                HookMessages();
            }

        }


        private void HookMessages()
        {
            MessengerInstance.Register<PerformActivity>(this, (msg) =>
            {
                // only one activity can be running at a time
                Debug.Assert(!IsShowing);

                if (IsShowing)
                    throw new Exception("Only one activity may run at a time.");

                ActivityRequest = msg.Activity;

                ShowProgressBar = false;
                ShowCloseButton = false;

                // this triggers the dialog to animate on screen
                IsShowing = true;
                InvalidateCommands();
            });
        }

        private void InvalidateCommands()
        {
            if (IsInDesignMode)
                return;

            AcceptCommand.RaiseCanExecuteChanged();
            CancelCommand.RaiseCanExecuteChanged();
            CloseCommand.RaiseCanExecuteChanged();
        }

        private ActivityRequest _activityRequest = null;

        public ActivityRequest ActivityRequest
        {
            get
            {
                return _activityRequest;
            }
            set
            {
                var old = _activityRequest;

                if (Set(() => ActivityRequest, ref _activityRequest, value))
                {
                    if (old != null)
                        old.PropertyChanged -= ActivityRequest_PropertyChanged;

                    if (value != null)
                        ActivityRequest.PropertyChanged += ActivityRequest_PropertyChanged;

                    InvalidateCommands();
                }
            }
        }

        void ActivityRequest_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsCompleted")
            {
                if (ActivityRequest.CompletedResult != ActivityResult.None && IsShowing == true)
                {
                    bool autoClose = false;

                    var delay = 750;

                    if (ActivityRequest.CompletedResult == ActivityResult.Cancelled)
                    {
                        // if cancelled, can hide with little delay
                        if (ActivityRequest.HasEverRun)
                        {
                            ShowCloseButton = true;
                            delay = 3000;
                        }
                        else
                        {
                            delay = 1;
                        }
                        autoClose = true;
                    }
                    else 
                    {
                        autoClose = ActivityRequest.CompletedResult == ActivityResult.Failed ? false : ActivityRequest.IsAutoClose;
                        ShowCloseButton = true;
                        delay = ActivityRequest.AutoCloseDelay;
                    }

                    if (autoClose)
                    {
                        Task.Run(async () =>
                        {
                            // wait a tiny bit so user can see how things ended up

                            await Task.Delay(delay);
                            await DispatcherHelper.RunAsync(() =>
                            {
                                IsShowing = false;
                            });
                        });
                    }
                }

                if (ActivityRequest.IsIndeterminateProgress)
                    ShowProgressBar = false;

                InvalidateCommands();
                
            }

            if (e.PropertyName == "IsRunning")
            {
                InvalidateCommands();
            }

            if (e.PropertyName == "IsAcceptanceDisabled")
            {
                InvalidateCommands();
            }
        }


        private bool _showCloseButton = false;

        public bool ShowCloseButton
        {
            get
            {
                return _showCloseButton;
            }

            set
            {
                Set(() => ShowCloseButton, ref _showCloseButton, value);
                InvalidateCommands();
            }
        }


        private bool _showProgressBar = false;

        public bool ShowProgressBar
        {
            get
            {
                return _showProgressBar;
            }
            set
            {
                Set(() => ShowProgressBar, ref _showProgressBar, value);
            }
        }

        private bool _isShowing = false;

        public bool IsShowing
        {
            get
            {
                return _isShowing;
            }
            set
            {
                if (Set(() => IsShowing, ref _isShowing, value))
                {
                    if (IsInDesignMode)
                        return;

                    InvalidateCommands();

                    // put up the protective mask in the background

                     MessengerInstance.Send(new TriggerActivityMask(value));

                     if (value == false)
                     {
                         // release the object if no longer needed, but wait until
                         // no longer showing by using a delay.

                         Task.Run(async () =>
                         {
                             // wait until now showing so controls in panel won't go blank
                             // while still animating off screen

                             await Task.Delay(2000);
                             await DispatcherHelper.RunAsync(() =>
                             {
                                 if (!IsShowing)
                                     ActivityRequest = null;
                             });
                         });
                     }
                }
            }
        }


        private RelayCommand _acceptCommand;
        public RelayCommand AcceptCommand
        {
            get
            {
                return _acceptCommand
                    ?? (_acceptCommand = new RelayCommand(
                    () =>
                    {
                        if (!AcceptCommand.CanExecute(null))
                        {
                            return;
                        }

                        ShowProgressBar = true;
                        ActivityRequest.AcceptAction();
                    },
                    () => 
                        {
                            if (ActivityRequest == null || ActivityRequest.IsAcceptanceDisabled)
                                return false;

                            if (ActivityRequest.IsCompleted || ActivityRequest.IsRunning)
                                return false;

                            return true;
                        }
                        ));
            }
        }

        private RelayCommand _cancelCommand;
        public RelayCommand CancelCommand
        {
            get
            {
                return _cancelCommand
                    ?? (_cancelCommand = new RelayCommand(
                    () =>
                    {
                        if (!CancelCommand.CanExecute(null))
                        {
                            return;
                        }

                        if (ActivityRequest == null)
                            return;

                        ActivityRequest.CancelAction();

                    },
                    () =>
                    {
                        if (ActivityRequest == null)
                            return false;

                        if (ActivityRequest.IsCompleted)
                            return false;
                        
                        if (ActivityRequest.IsRunning)
                            return ActivityRequest.IsCancellable;

                        return true;
                    }));
            }
        }

        private RelayCommand _closeCommand;

        public RelayCommand CloseCommand
        {
            get
            {
                return _closeCommand
                    ?? (_closeCommand = new RelayCommand(
                    () =>
                    {
                        if (!CloseCommand.CanExecute(null))
                        {
                            return;
                        }

                        IsShowing = false;
                    },
                    () =>
                    {
                        if (ActivityRequest == null)
                            return false;

                        return true;
                    }));
            }
        }

    }
}