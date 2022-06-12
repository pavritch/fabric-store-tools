using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using System.Windows;

namespace ProductScanner.App
{
    public class ActivityRequest : ObservableObject
    {
        private bool isFinishedUp = false;
        private string _caption = "Please Confirm";
        private string _title = null;
        private bool _isCancellable = true;
        private bool _hasEverRun = false;
        private bool _isAutoClose = false;
        private int _autoCloseDelay = 4000;
        private bool _isIndeterminateProgress = false;
        private double _percentComplete = 0.0;
        private string _statusMessage = null;
        private bool _isRunning = false;
        private bool _isCompleted = false;
        private string _acceptButtonText = "Accept";
        private ActivityResult _completedResult = ActivityResult.None;
        private TaskCompletionSource<ActivityResult> tcs;

        private CancellationTokenSource cancelTokenSource;

        public Action<ActivityRequest> OnAccept { get; set; }
        public Action<ActivityRequest> OnCancel { get; set; }

        public ActivityRequest()
        {
            cancelTokenSource = new CancellationTokenSource();
            tcs = new TaskCompletionSource<ActivityResult>();
        }

        /// <summary>
        /// Cancellation token.
        /// </summary>
        public CancellationToken CancelToken
        {
            get
            {
                return cancelTokenSource.Token;
            }
        }

        /// <summary>
        /// General caption for the UX - something like "Please Confirm".
        /// </summary>
        /// <remarks>
        /// Think of this as being similar to a window/dialog caption.
        /// </remarks>
        public string Caption
        {
            get
            {
                return _caption;
            }
            set
            {
                Set(() => Caption, ref _caption, value);
            }
        }


        /// <summary>
        /// General caption for the UX - something like "Please Confirm".
        /// </summary>
        /// <remarks>
        /// Think of this as being similar to a window/dialog caption.
        /// </remarks>
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                Set(() => Title, ref _title, value);
            }
        }

        /// <summary>
        /// Indicates if the operation can be cancelled once started. Default true.
        /// </summary>
        public bool IsCancellable
        {
            get
            {
                return _isCancellable;
            }
            set
            {
                Set(() => IsCancellable, ref _isCancellable, value);
            }
        }

        /// <summary>
        /// Optional element/control which (when provided) will be 
        /// added to the right of the activity dialog.
        /// </summary>
        private UIElement _customElement = null;
        public UIElement CustomElement
        {
            get
            {
                return _customElement;
            }
            set
            {
                Set(() => CustomElement, ref _customElement, value);
            }
        }

        /// <summary>
        /// Sets and gets the HasEverRun property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool HasEverRun
        {
            get
            {
                return _hasEverRun;
            }
            set
            {
                Set(() => HasEverRun, ref _hasEverRun, value);
            }
        }
        /// <summary>
        /// Determines the kind of progress bar presented in the UX.
        /// </summary>
        /// <remarks>
        /// Default is false for a normal progress bar 0-100. When true,
        /// changes the meter bar to be of the indeterminate type.
        /// </remarks>
        public bool IsIndeterminateProgress
        {
            get
            {
                return _isIndeterminateProgress;
            }
            set
            {
                Set(() => IsIndeterminateProgress, ref _isIndeterminateProgress, value);
            }
        }


        public double PercentComplete
        {
            get
            {
                return _percentComplete;
            }
            set
            {
                Set(() => PercentComplete, ref _percentComplete, value);
            }
        }


        public string StatusMessage
        {
            get
            {
                return _statusMessage;
            }
            set
            {
                Set(() => StatusMessage, ref _statusMessage, value);
            }
        }

        public bool IsRunning
        {
            get
            {
                return _isRunning;
            }
            set
            {
                Set(() => IsRunning, ref _isRunning, value);
                if (value == true)
                    HasEverRun = true;
            }
        }



        /// <summary>
        /// Sets and gets the IsAutoClose property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsAutoClose
        {
            get
            {
                return _isAutoClose;
            }
            set
            {
                Set(() => IsAutoClose, ref _isAutoClose, value);
            }
        }



        public int AutoCloseDelay
        {
            get
            {
                return _autoCloseDelay;
            }
            set
            {
                Set(() => AutoCloseDelay, ref _autoCloseDelay, value);
            }
        }

        public string AcceptButtonText
        {
            get
            {
                return _acceptButtonText;
            }
            set
            {
                Set(() => AcceptButtonText, ref _acceptButtonText, value);
            }
        }

        /// <summary>
        /// The action has been completed in one form or another.
        /// </summary>
        /// <remarks>
        /// Generally, do not set this directly. Let it be set when the completed result is set.
        /// </remarks>
        public bool IsCompleted
        {
            get
            {
                return _isCompleted;
            }
            set
            {
                Set(() => IsCompleted, ref _isCompleted, value);
            }
        }


        // provides opportunity for external logic to manipulate state of accept button
        // when input state becomes invalid, etc.
        private bool _isAcceptanceDisabled = false;
        public bool IsAcceptanceDisabled
        {
            get
            {
                return _isAcceptanceDisabled;
            }
            set
            {
                Set(() => IsAcceptanceDisabled, ref _isAcceptanceDisabled, value);
            }
        }

        public ActivityResult CompletedResult
        {
            get
            {
                return _completedResult;
            }
            set
            {
                Set(() => CompletedResult, ref _completedResult, value);
                IsCompleted = value != ActivityResult.None;
            }
        }

        public void AcceptAction()
        {
            IsRunning = true;

            if (OnAccept != null)
                OnAccept(this);
        }

        public void CancelAction()
        {
            SetCompleted(ActivityResult.Cancelled, IsRunning ? "Activity cancelled." : string.Empty);
            cancelTokenSource.Cancel();
           
            if (OnCancel != null)
                OnCancel(this);

            FinishUp();
        }

        /// <summary>
        /// Indicates the running operation has completed - either successfully or not.
        /// </summary>
        /// <remarks>
        /// The result and message are simply for display in the UX.
        /// </remarks>
        /// <param name="result"></param>
        /// <param name="message"></param>
        public void SetCompleted(ActivityResult result, string message = null)
        {
            IsRunning = false;
            CompletedResult = result;
            StatusMessage = message;
        }

        /// <summary>
        /// Instruction the activity service to present the UX.
        /// </summary>
        /// <param name="onAccept">Invoked when user clicks Accept button.</param>
        /// <param name="onCancel">Invoked when user clicks Cancel button</param>
        /// <remarks>
        /// The service will invoke these callbacks when the user clicks on the
        /// respective buttons.
        /// </remarks>
        public Task<ActivityResult> Show(ActivityRequest activityRequest)
        {
            Messenger.Default.Send(new PerformActivity(activityRequest));
            return tcs.Task;
        }

        public void FinishUp()
        {
            if (isFinishedUp)
                return;

            tcs.SetResult(CompletedResult);
            isFinishedUp = true;
        }
    }
}
