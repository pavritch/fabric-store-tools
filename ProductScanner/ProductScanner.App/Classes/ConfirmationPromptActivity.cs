using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProductScanner.App
{
    public class ConfirmationPromptActivity : ActivityRequest
    {
        private Func<CancellationToken, Task<ActivityResult>> activityFunction;

        public string SuccessMessage { get; set; }
        public string FailureMessage { get; set; }

        public ConfirmationPromptActivity(string prompt, Func<CancellationToken, Task<ActivityResult>> activity=null, bool isCancellable=false) : base()
	    {
            this.activityFunction = activity;

            Title = prompt;
            IsIndeterminateProgress = true;
            PercentComplete = 0.0;
            IsCancellable = isCancellable;
            StatusMessage = string.Empty;
            IsAutoClose = false;
            IsAcceptanceDisabled = false;

            if (activityFunction== null)
            {
                AutoCloseDelay = 0;
                IsAutoClose = true;
            }

            OnAccept = async (a) =>
            {
                try
                {
                    if (activityFunction == null)
                    {
                        // no function supplied, meaning that this is only a prompt, return immediately
                        a.SetCompleted(ActivityResult.Success, string.Empty);
                    }
                    else
                    {
                        // there is an associated method to call with indeterminate progress, cancellation optionally supported.

                        var result = await activityFunction(a.CancelToken);

                        // if was cancelled by user, then we don't need to set cancel here

                        if (!a.CancelToken.IsCancellationRequested)
                        {
                            if (result == ActivityResult.Success)
                            {
                                string msg = SuccessMessage ?? "Action completed successfully.";
                                a.SetCompleted(ActivityResult.Success, msg);
                            }
                            else
                            {
                                string msg = FailureMessage ?? "Action completed with errors.";
                                a.SetCompleted(ActivityResult.Failed, msg);
                            }
                        }
                    }
                }
                catch (Exception Ex)
                {
                    a.SetCompleted(ActivityResult.Failed, Ex.Message);
                }
                finally
                {
                    // finalize the task, return from original await
                    a.FinishUp();
                }
            };


            OnCancel = (a) =>
            {
                // typically won't need to do anything here
                // FinishUp() called automatically upon return from this method.
            };

	    }

    }
}
