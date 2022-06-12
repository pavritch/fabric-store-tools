using System;
using System.Data;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace Website
{
    /// <summary>
    /// Simple worker task with start stop pattern.
    /// </summary>
    /// <remarks>
    /// This class is a base for simple, yet long-running tasks. Frequently,
    /// these tasks would be started in global.asax.cs, but could also be
    /// invoked from any class that needs long-running background support.
    /// </remarks>
    public abstract class SimpleWorkerServiceBase : IDisposable
    {
        private ManualResetEvent evtStopRequested;
        private Thread thWorker;
        private int intervalSeconds;
        private string threadName;
        private bool disposed;
        private int initialDelaySeconds;
        
        public SimpleWorkerServiceBase(int IntervalSeconds)
            :
            this(IntervalSeconds, "thSimpleWorker")
        {
            this.intervalSeconds = IntervalSeconds;
        }

        public SimpleWorkerServiceBase(int IntervalSeconds, string threadName)
            : this(IntervalSeconds, threadName, 0)
        {
        }
        
        public SimpleWorkerServiceBase(int IntervalSeconds, string threadName, int initialDelaySeconds)
        {
            this.intervalSeconds = IntervalSeconds;
            this.threadName = threadName;
            this.initialDelaySeconds = initialDelaySeconds;
        }

        // the destructor
        ~SimpleWorkerServiceBase()
        {
            // call Dispose with false.  Since we're in the
            // destructor call, the managed resources will be
            // disposed of anyways.
            Dispose(false);
        }

        public virtual bool Start()
        {
            // if presently running, stop, then fire up from scratch
            if (evtStopRequested != null)
                Stop();
                
            evtStopRequested = new ManualResetEvent(false);
            // go tet the first set of data;
            // create a thread to refresh the website meta data every so often

            thWorker = new Thread(new ThreadStart(Run));
            thWorker.Name = threadName;
            thWorker.Start();

            return true;
        }


        public virtual bool Stop()
        {
            if (evtStopRequested != null)
            {
                // do not return when called until all threads die and
                // associated resources released, etc. Assume process may die
                // immediately upon return.

                // signal to watchers that we want to shut things down.
                evtStopRequested.Set();

                try
                {
                    thWorker.Join(60 * 1000);
                    if (thWorker.IsAlive)
                    {
                        thWorker.Abort(); // kill it
                    }
                }
                catch
                {
                }
                
                thWorker = null;
                evtStopRequested.Close();
                evtStopRequested = null;
            }
            return true;
        }

        protected bool SleepInterval(int milliseconds)
        {
            return evtStopRequested.WaitOne(milliseconds, false);
        }
        
        protected bool Sleep(int seconds)
        {
            // goes to sleep for N seconds, but wakes immediately if stop requested
            // return true if stop requested

            return evtStopRequested.WaitOne(1000 * seconds, false);
        }
        
        protected bool InitialDelay()
        {
            return Sleep(initialDelaySeconds);
        }

        protected bool SleepInterval()
        {
            // goes to sleep for N seconds, but wakes immediately if stop requested
            // return true if stop requested
            return Sleep(intervalSeconds);
        }

        protected bool IsStopRequested()
        {
            // always returns immediately
            // return true if stop requested
            return (evtStopRequested.WaitOne(0, false));
        }

        /// <summary>
        /// Run one pass of the task, called on timer interval by Run(). Derrived
        /// classes should generally override this method.
        /// </summary>
        protected virtual void RunSingleTask()
        {
            // do some task...if the task is long running, it is recommended
            // that it check IsStopRequested() now and then.
        }
        

        /// <summary>
        /// Main method, either this or RunSingleTask() should be overridden by 
        /// derrived class. 
        /// </summary>
        protected virtual void Run()
        {
            try
            {
                // an initial delay can be specified which prevents first
                // run of the task from running for a little bit.
                
                InitialDelay();
                    RunSingleTask();

                // SleepInterval() returns true when should exit now
                while (!SleepInterval())
                {
                    RunSingleTask();
                }
            }
            catch (Exception Ex)
            {
                // any exception that reaches here will stop the service
                Stop();
                var ev = new WebsiteRequestErrorEvent("Unhandled Exception. Simple service stopped.", this, WebsiteEventCode.UnhandledException, Ex);
                ev.Raise();
            }
        }
        public void Dispose()
        {
            // dispose of the managed and unmanaged resources
            Dispose(true);

            // tell the GC that the Finalize process no longer needs
            // to be run for this object.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposeManagedResources)
        {
            // process only if mananged and unmanaged resources have
            // not been disposed of.
            if (!disposed)
            {
                if (disposeManagedResources)
                {
                    Trace.WriteLine("ClassBeingTested: Disposing managed resources");
                    // dispose managed resources
                    Stop();
                }

                // dispose unmanaged resources here, if any

                disposed = true;
            }
        }
    }
}
