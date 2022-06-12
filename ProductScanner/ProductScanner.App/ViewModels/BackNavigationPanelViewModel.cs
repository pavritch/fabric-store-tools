using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace ProductScanner.App.ViewModels
{
    public class BackNavigationPanelViewModel : ViewModelBase
    {
        public BackNavigationPanelViewModel()
        {
#if DEBUG
            if (IsInDesignMode)
            {
                IsEnabled = true;
            }
#endif
            if (!IsInDesignMode)
            {
                HookMessages();
            }
            
        }

        private void HookMessages()
        {
            MessengerInstance.Register<AnnouncementMessage>(this, (msg) =>
            {
                switch(msg.Kind)
                {
                    case Announcement.DisableBackNavigation:
                        IsEnabled = false;
                        break;

                    case Announcement.EnableBackNavigation:
                        IsEnabled = true;
                        break;

                    default:
                        break;
                }
            });
        }



        private bool _isEnabled = false;

        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                Set(() => IsEnabled, ref _isEnabled, value);
                BackNavCommand.RaiseCanExecuteChanged();
            }
        }

        private RelayCommand _backNavCommand;


        public RelayCommand BackNavCommand
        {
            get
            {
                return _backNavCommand
                    ?? (_backNavCommand = new RelayCommand(
                    () =>
                    {
                        if (!BackNavCommand.CanExecute(null))
                        {
                            return;
                        }

                        MessengerInstance.Send(new AnnouncementMessage(Announcement.RequestBackNavigation));
                    },
                    () => IsEnabled));
            }
        }
    }
}