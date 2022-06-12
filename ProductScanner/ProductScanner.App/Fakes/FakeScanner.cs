using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Threading;
using ProductScanner.Core.PlatformEntities;
using ProductScanner.Core.Scanning;
using ProductScanner.Core.Scanning.EventLogs;
using ProductScanner.Core.Scanning.Pipeline;

namespace ProductScanner.App
{
    /// <summary>
    /// Hooks for fake scanner to make callbacks to host.
    /// </summary>
    public class FakeScannerContext
    {
        public Action<EventType, string> SendLogEvent;
        public Action<string> SendProgressStatusMessage;
        public Action<string> SendProgressSecondaryMessage;
        public Action<double> SendProgressPercentComplete;
        public Action SendWebRequest;
        public Action SendCreateCheckpoint;
        public Action<CommitBatchType> SendSubmitBatch;
        public Action<string> SendReportError;
        public Action SendOperationFailed;
        public Action SendOperationFinished;
    }

    /// <summary>
    /// A crude simulated scanner.
    /// </summary>
    /// <remarks>
    /// Extreme liberties taken just to make data flow....do not assume anything based on what's here.
    /// </remarks>
    public class FakeScanner
    {
        private FakeScannerContext ctx;
        private bool isRunning;
        private DateTime segmentStartTime;
        private TimeSpan totalDuration;
        private TimeSpan segmentDuration;
        private TimeSpan accumulatedDuration;
        private Random rnd;
        private CancellationTokenSource cancelToken;
        private bool isFailure = false;
        private int maxErrorCount = 0;
        private int minErrorSeconds;
        private int maxErrorSeconds;
        private bool shouldMakeErrors;
        private bool shouldHaveFatalError;
        private int currentErrorCount;
        private bool isSuspended = false;
        private bool isCancelled = false;

        public FakeScanner(FakeScannerContext ctx)
        {
            this.ctx = ctx;
            rnd = new Random();

            // how long will it take this operation to complete.
            var minutes = GetRandom(2, 10);
            //minutes = 1;
            totalDuration = TimeSpan.FromMinutes(minutes);
        }

        private int GetRandom(int min, int max)
        {
            lock(rnd)
            {
                return rnd.Next(min, max);
            }
        }

        #region Send Notifications
        private async Task SendLogEvent(EventType eventType, string text)
        {
            if (ctx.SendLogEvent == null)
                return;

            await DispatcherHelper.RunAsync(() =>
            {
                ctx.SendLogEvent(eventType, text);
            });
        }

        private async Task SendProgressStatusMessage(string text)
        {
            if (ctx.SendProgressStatusMessage == null)
                return;

            await DispatcherHelper.RunAsync(() =>
            {
                ctx.SendProgressStatusMessage(text);
            });
        }

        private async Task SendProgressSecondaryMessage(string text)
        {
            if (ctx.SendProgressSecondaryMessage == null)
                return;
            await DispatcherHelper.RunAsync(() =>
            {
                ctx.SendProgressSecondaryMessage(text);
            });
        }

        private async Task SendProgressPercentComplete(double pct)
        {
            if (ctx.SendProgressPercentComplete == null)
                return;

            await DispatcherHelper.RunAsync(() =>
            {
                ctx.SendProgressPercentComplete(pct);
            });
        }

        private async Task SendWebRequest()
        {
            await DispatcherHelper.RunAsync(() =>
            {
                if (ctx.SendWebRequest != null)
                    ctx.SendWebRequest();
            });
        }

        private async Task SendCreateCheckpoint()
        {
            await DispatcherHelper.RunAsync(() =>
            {
                if (ctx.SendCreateCheckpoint != null)
                    ctx.SendCreateCheckpoint();
            });
        }

        private async Task SendSubmitBatch(CommitBatchType batchType)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                if (ctx.SendSubmitBatch != null)
                    ctx.SendSubmitBatch(batchType);
            });
        }

        private async Task SendReportError(string text)
        {
            await DispatcherHelper.RunAsync(() =>
            {
                if (ctx.SendReportError != null)
                    ctx.SendReportError(text);
            });
        }

        private async Task SendOperationFailed()
        {
            await DispatcherHelper.RunAsync(() =>
            {

                if (ctx.SendOperationFailed != null)
                    ctx.SendOperationFailed();
            });
        }

        private async Task SendOperationFinished()
        {
            await DispatcherHelper.RunAsync(() =>
            {

                if (ctx.SendOperationFinished != null)
                    ctx.SendOperationFinished();
            });
        } 
        #endregion

        #region Main Entry Points

        /// <summary>
        /// Initiate a fresh scanning operation based on the provided options.
        /// </summary>
        /// <remarks>
        /// Although returns Task, intended to return rather quickly, with the result indicating if
        /// the action was successfully invoked -- not if was fully completed. 
        /// </remarks>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<ScanningActionResult> StartScanning(IEnumerable<ScanOptions> options, int maxErrorCount)
        {
            // note that in real life the requests/minute rate and error count are adjustable in real time, whereas
            // in this fake model, such is not supported for the sake of simplicity

            this.maxErrorCount = maxErrorCount;
            accumulatedDuration = TimeSpan.FromMilliseconds(0);
            currentErrorCount = 0;

            // decide what if any errors will happen during this scan
            shouldMakeErrors = GetRandom(1, 8) == 5;
            if (shouldMakeErrors)
            {
                minErrorSeconds = 15;
                maxErrorSeconds = 60;
            }

            shouldHaveFatalError = GetRandom(1, 8) == 5;

            //Debug.WriteLine(string.Format("Simulated time to complete: {0}", totalDuration.ToString(@"d\.hh\:mm\:ss")));
            //Debug.WriteLine(string.Format("Simulated errors: {0}", shouldMakeErrors.ToString()));

            await SendLogEvent(EventType.General, string.Format("Simulated time to complete: {0}", totalDuration.ToString(@"d\.hh\:mm\:ss")));
            await SendLogEvent(EventType.General, string.Format("Simulated errors: {0}", shouldMakeErrors.ToString()));

            await StartRunning();

            return ScanningActionResult.Success;
        }

        /// <summary>
        /// Cancel the running or suspended scanning operation.
        /// </summary>
        /// <remarks>
        /// Although returns Task, intended to return rather quickly, with the result indicating if
        /// the action was successfully invoked -- not if was fully completed. 
        /// </remarks>
        /// <returns></returns>
        public async Task<ScanningActionResult> CancelScanning()
        {
            isCancelled = true;
            await StopRunning();
            return ScanningActionResult.Success;
        }

        /// <summary>
        /// Suspend the now running scanning operation.
        /// </summary>
        /// <remarks>
        /// Although returns Task, intended to return rather quickly, with the result indicating if
        /// the action was successfully invoked -- not if was fully completed. 
        /// </remarks>
        /// <returns></returns>
        public async Task<ScanningActionResult> SuspendScanning()
        {
            isSuspended = true;
            await StopRunning();
            return ScanningActionResult.Success;
        }

        /// <summary>
        /// Resume the presently-suspended scanning operation.
        /// </summary>
        /// <remarks>
        /// Although returns Task, intended to return rather quickly, with the result indicating if
        /// the action was successfully invoked -- not if was fully completed. 
        /// </remarks>
        /// <returns></returns>
        public async Task<ScanningActionResult> ResumeScanning()
        {
            isSuspended = false;
            await StartRunning();
            return ScanningActionResult.Success;
        } 
        #endregion

        private void DoLogging()
        {
            // send random log events
            Task.Run(async () =>
            {
                int msgIndex = 1000;

                while (isRunning)
                {
                    var msg = string.Format("This is log entry number {0}", msgIndex++);
                    EventType evType = (GetRandom(1, 50) == 10) ? EventType.Warning : EventType.General;
                    await SendLogEvent(evType, msg);
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(GetRandom(5, 30)), cancelToken.Token);
                    }
                    catch { }

                }
            });
        }

        private void DoProgressStatusMessages()
        {
            Task.Run(async () =>
            {
                int msgIndex = 1000;

                while (isRunning)
                {
                    var msg = string.Format("My status message number {0}", msgIndex++);
                    await SendProgressStatusMessage(msg);

                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(GetRandom(5, 30)), cancelToken.Token);
                    }
                    catch { }
                }
            });
        }


        private void DoProgressSecondaryMessages()
        {
            Task.Run(async () =>
            {
                int msgIndex = 1000 * 1000;

                while (isRunning)
                {
                    var msg = string.Format("Remaining: {0:N0}", msgIndex--);
                    await SendProgressSecondaryMessage(msg);

                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(GetRandom(1, 4)), cancelToken.Token);
                    }
                    catch { }
                }
            });
        }

        private void DoProgressMeter()
        {
            Task.Run(async () =>
            {
                var isIndeterminate = false;
                while (isRunning)
                {
                    if (isIndeterminate)
                    {
                        await SendProgressPercentComplete(-1);

                        try
                        {
                            await Task.Delay(TimeSpan.FromSeconds(GetRandom(5, 20)), cancelToken.Token);
                        }
                        catch { }
                    }
                    else
                    {
                        for (int i = 0; i <= 100; i++)
                        {
                            if (!isRunning)
                                break;

                            await SendProgressPercentComplete(i);
                            try
                            {
                                await Task.Delay(TimeSpan.FromMilliseconds(GetRandom(500, 2000)), cancelToken.Token);
                            }
                            catch { }

                        }
                    }

                    isIndeterminate = !isIndeterminate;
                }
            });
        }


        private async Task StopRunning()
        {
            // introduce latency
            await Task.Delay(TimeSpan.FromMilliseconds(GetRandom(500, 2000)));
            isRunning = false;
            cancelToken.Cancel();
            accumulatedDuration += (DateTime.Now - segmentStartTime);
        }

        private async Task StartRunning()
        {
            segmentStartTime = DateTime.Now;
            // segmentDuration is how long we need to run the clock out on this faked scan
            segmentDuration = totalDuration - accumulatedDuration;

            cancelToken = new CancellationTokenSource();

            // introduce latency
            await Task.Delay(TimeSpan.FromMilliseconds(GetRandom(500, 2000)));

            isRunning = true;
            DoLogging();
            DoCheckpoints();
            DoWebHits();
            DoProgressStatusMessages();
            DoProgressSecondaryMessages();
            DoProgressMeter();
            if (shouldMakeErrors)
                DoErrors();

            Run();
        }

        private void Run()
        {
            Task.Run(async () =>
            {
                try
                {
                    //Debug.WriteLine(string.Format("Begin segment delay for: {0}", segmentDuration));
                    await Task.Delay(segmentDuration, cancelToken.Token);
                }
                catch {}

                if (isRunning)
                {
                    // if time elapsed, and still running, means we've finished the scan
                    // and should report either pass or fail.

                    await StopRunning();
                }
                await DispatcherHelper.RunAsync(async () =>
                {

                    if (isCancelled || isSuspended)
                        return;

                    if (!isFailure)
                        await OnFinished();
                    else
                        await OnFailed();
                });
            });
        }

        private void DoCheckpoints()
        {
            Task.Run(async () =>
            {
                while (isRunning)
                {
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(GetRandom(30, 300)), cancelToken.Token);
                    }
                    catch { }

                    if (isRunning)
                        await SendCreateCheckpoint();
                }
            });
        }

        private void DoWebHits()
        {
            Task.Run(async () =>
            {
                while (isRunning)
                {
                    try
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(GetRandom(500, 2000)), cancelToken.Token);
                    }
                    catch { }

                    if (isRunning)
                        await SendWebRequest();
                }
            });
        }

        private void DoErrors()
        {
            Task.Run(async () =>
            {
                int index = 5000;

                while (true)
                {
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(GetRandom(minErrorSeconds, maxErrorSeconds)), cancelToken.Token);
                    }
                    catch { }

                    if (!isRunning)
                        break;

                    currentErrorCount++;
                    await SendReportError(string.Format("This is a reported error {0}.", index++));

                    if (shouldHaveFatalError)
                    {
                        var isFatalNow = shouldMakeErrors = GetRandom(1, 8) == 5;
                        if (isFatalNow)
                        {
                            isFailure = true;
                            await StopRunning();
                            break;
                        }   
                    }

                    if (currentErrorCount >= maxErrorCount)
                    {
                        isFailure = true;
                        await StopRunning();
                        break;
                    }
                }
            });
        }


        private async Task OnFinished()
        {
            // could be several batches which get committed. 
            // call out once for each individual batch
            var batchCount = GetRandom(1, 7);
            for (int i = 0; i < batchCount; i++)
            {
                var batchType = (CommitBatchType)i;
                await SendSubmitBatch(batchType);
            }
            await SendOperationFinished();
        }

        private async Task OnFailed()
        {
            await SendOperationFailed();
        }
    }
}
