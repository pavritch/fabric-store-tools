using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Website
{
    /// <summary>
    /// Keeps track of API calls to ensure not going too fast. Singleton.
    /// </summary>
    /// <remarks>
    /// All callers wishing to make API calls must call in here to wait in line.
    /// Their thread/task will be released/completed to run when it comes their turn.
    /// </remarks>
    public class ShopifyApiSentry
    {
        private object lockObj = new Object();
        private int bucketSize;
        private int leakRate; // milliseconds
        private int bucketLevel; // actual present level
        private Timer timer;
        private bool isShuttingDown = false;
        private Queue<TaskCompletionSource<bool>> requestQueue;

        /// <summary>
        /// Initialize sentry.
        /// </summary>
        /// <param name="bucketSize">Max size of the bucket, generally 40</param>
        /// <param name="leakRate">Delay between leakage (requests) in milliseconds, generally 2 per second (500ms)</param>
        public ShopifyApiSentry(int bucketSize, int leakRate)
        {
            this.bucketSize = bucketSize;
            this.leakRate = leakRate;
            SetBucketLevel(0);
            requestQueue = new Queue<TaskCompletionSource<bool>>();
            timer = new Timer(OnTimer, null, TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(leakRate));
        }

        /// <summary>
        /// Set current bucket level, but limit to max size no matter what.
        /// </summary>
        /// <param name="level"></param>
        public void SetBucketLevel(int level)
        {
            bucketLevel = Math.Min(level, bucketSize);
        }

        /// <summary>
        /// Return the current bucket level.
        /// </summary>
        public int BucketLevel
        {
            get
            {
                return bucketLevel;
            }
        }

        /// <summary>
        /// Fired at leakRate milliseconds. Leak the bucket by one.
        /// </summary>
        /// <param name="stateInfo"></param>
        private void OnTimer(object stateInfo)
        {
            lock(lockObj)
            {
                if (bucketLevel > 0)
                    bucketLevel--;
            }

            CheckQueue();
        }

        private void CheckQueue()
        {
            lock(lockObj)
            {
                if (isShuttingDown)
                    return;

                if (requestQueue.Count() == 0)
                    return;

                // if bucket already full, then cannot do anything now
                if (bucketLevel >= bucketSize)
                    return;

                var tcs = requestQueue.Dequeue();

                // skip if already cancelled out
                if (tcs.Task.IsCanceled)
                    return;

                tcs.TrySetResult(true);
                bucketLevel++;
            }
        }

        public void ShutDown()
        {
            lock(lockObj)
            {
                if (isShuttingDown)
                    return;

                isShuttingDown = true;
                timer.Dispose();

                while(true)
                {
                    if (requestQueue.Count() == 0)
                        break;

                    var tcs = requestQueue.Dequeue();
                    tcs.TrySetResult(false);
                }

            }
        }

        /// <summary>
        /// Number of requests presently in the queue.
        /// </summary>
        public int RequestQueueLength
        {
            get
            {
                lock (lockObj)
                {
                    return requestQueue.Count();
                }
            }
        }

        /// <summary>
        /// Entry point to be used prior to making any API call to ensure not going too fast.
        /// </summary>
        /// <returns></returns>
        public Task<bool> WaitMyTurn()
        {
           lock(lockObj)
           {
               if (isShuttingDown)
                   return Task.FromResult<bool>(false);

               var tcs = new TaskCompletionSource<bool>();

               requestQueue.Enqueue(tcs);

               Task.Factory.StartNew(() =>
               {
                   CheckQueue(); 
               });

               return tcs.Task;
           }
        }
    }
}
