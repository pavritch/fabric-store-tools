using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using Gen4.Util.Misc;

namespace Website
{
    /// <summary>
    /// Relates progress for a long operation - to be displayed on web page, etc.
    /// </summary>
    public class LongOperationProgress
    {
        private  CancellationTokenSource cancelTokenSource { get; set; }

        /// <summary>
        /// Unique ID assigned to this operation.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Each kind of operation is assigned a key used for lookup tables, etc.
        /// </summary>
        /// <remarks>
        /// Generally, only one operation for a given key will be active at a time.
        /// </remarks>
        public string Key { get; private set; }

        public double PercentComplete { get; set; }

        public int CountTotal { get; set; }

        public int CountRemaining { get; set; }

        public DateTime TimeStarted { get; private set; }
        public DateTime? TimeFinished { get; private set; }

        public string StatusMessage { get; set; }
        public string Result { get; set; }
        public bool IsRunning { get; private set; }

        public List<string> Log { get; private set; }

        /// <summary>
        /// The human title for this operation.
        /// </summary>
        public string Title { get; set; }

        public LongOperationProgress(string key, string title = null)
        {
            this.Key = key;
            this.Title = title;
            Id = Guid.NewGuid();
            cancelTokenSource = new CancellationTokenSource();
            IsRunning = true;
            TimeStarted = DateTime.Now;
            PercentComplete = 0.0;
            Log = new List<string>();
            WriteLog(string.Format("Time started: {0:G}", TimeStarted));
        }

        public CancellationToken CancelToken
        {
            get {return cancelTokenSource.Token;}
        }

        public void Cancel()
        {
            cancelTokenSource.Cancel();
            WriteLog("Operation cancelled.");
            SetFinished("Cancelled");
        }

        public void Error(string msg)
        {
            WriteLog("Terminated due to error.");
            SetFinished("Error");
        }

        public void Update(int total, int completed)
        {
            CountTotal = total;
            CountRemaining = total - completed;
            PercentComplete = CountTotal == 0 ? 0 : (completed * 100.0) / (double)CountTotal;
        }

        public void WriteLog(string msg)
        {
            Log.Add(msg);
        }

        public void SetFinished(string result)
        {
            TimeFinished = DateTime.Now;
            this.Result = result;
            IsRunning = false;
            StatusMessage = string.Empty;
            WriteLog(string.Format("Time finished: {0:G}", TimeFinished));
            WriteLog(string.Format("Duration: {0:G}", TimeFinished - TimeStarted));

            // not setting percent complete since may not have actually finished.
        }

        public string ShortId
        {
            get
            {
                return Id.ToString().Replace("-", "").Right(8).ToUpper();
            }
        }
    }
}