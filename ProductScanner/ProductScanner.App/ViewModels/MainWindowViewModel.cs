using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using ProductScanner.App.Views;
using GalaSoft.MvvmLight.Threading;
using System.Windows;

namespace ProductScanner.App.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private IAppModel appModel;

        /// <summary>
        /// Top level window for the app.
        /// </summary>
        /// <remarks>
        /// Only one VM of this for the life of the app.
        /// Starts off with splash screen showing while app model is initialized. After, transitions
        /// to show the "Main" screen - which is the normal full screen for the app.
        /// 
        /// The ActivityPanel is part of this as well, combined with a full screen mask to shield
        /// user input while the activity panel is up. Both activity panel and mask are tied
        /// to visual states with animation.
        /// </remarks>
        /// <param name="appModel"></param>
        public MainWindowViewModel(IAppModel appModel)
        {
            // the app model is used here primarily just to get it initialized while the splash
            // screen is up. What's passed in has been instantiated, but not yet initialized.

            this.appModel = appModel;

            Content = new ProductScanner.App.Views.SplashScreen();
            
            if (IsInDesignMode)
            {
                Initialize(appModel).Wait();
                return;
            }

            IsActivityMaskShowing = false;

            if (!ViewModelBase.IsInDesignModeStatic)
            {
                HookMessages();
            }

            // we keep the splash screen up while we initialize the app and determine 
            // what we've got for stores, vendors, persisted SQL state, etc.

            Task.Run(async () =>
            {
                // don't want the splash screen flashing too fast, so we wait
                // until the longer of the two tasks is completed

                var tasks = new List<Task>()
                {
                    Task.Delay(3000),
                    Initialize(appModel),
                };

                await Task.WhenAll(tasks);
                await DispatcherHelper.RunAsync(() =>
                { 
                    if (appModel.IsInitialized)
                    {
                        Content = new Main();
                    }
                    else
                    {
                        MessengerInstance.Send(new ForceAppTerminationMessage("Error initializing application model."));
                    }
                });
            });
        }

        private void HookMessages()
        {
            MessengerInstance.Register<TriggerActivityMask>(this, (msg) =>
            {
                IsActivityMaskShowing = msg.IsShowing;
                IsActivityPanelShowing = msg.IsShowing;
            });
        }


        private async Task Initialize(IAppModel appModel)
        {
            // if additional initialization is required, should add it here if it 
            // cannot run in parallel.
            await appModel.InitializeAsync();
            return;
        }

        private object _content = null;
        /// <summary>
        /// This content is what fills the full app window. RadTransitionControl with fade.
        /// </summary>
        /// <remarks>
        /// Initially, we display the splash screen while the appmodel populates; then change to the normal user interface.
        /// </remarks>
        public object Content
        {
            get
            {
                return _content;
            }
            set
            {
                Set(() => Content, ref _content, value);
            }
        }

        private bool _isActivityMaskShowing = false;
        public bool IsActivityMaskShowing
        {
            get
            {
                return _isActivityMaskShowing;
            }
            set
            {
                if (Set(() => IsActivityMaskShowing, ref _isActivityMaskShowing, value))
                {
                    if (value == true)
                    {
                        ActivityMaskVisibility = true;
                    }
                    else
                    {
                        Task.Run(async () =>
                        {
                            await Task.Delay(1000);
                            await DispatcherHelper.RunAsync(() =>
                            {
                                ActivityMaskVisibility = false;
                            });
                        });
                    }
                }
            }
        }

        private bool _activityMaskVisibility = false;

        public bool ActivityMaskVisibility
        {
            get
            {
                return _activityMaskVisibility;
            }
            set
            {
                Set(() => ActivityMaskVisibility, ref _activityMaskVisibility, value);
            }
        }


        private bool _isActivityPanelShowing = false;

        public bool IsActivityPanelShowing
        {
            get
            {
                return _isActivityPanelShowing;
            }
            set
            {
                Set(() => IsActivityPanelShowing, ref _isActivityPanelShowing, value);

                if (!IsInDesignMode)
                {
                    var msg = value ? Announcement.RequestIncrementApplicationCloseBlocked : Announcement.RequestDecrementApplicationCloseBlocked;
                    MessengerInstance.Send(new AnnouncementMessage(msg));
                }
                    
            }
        }

    }
}