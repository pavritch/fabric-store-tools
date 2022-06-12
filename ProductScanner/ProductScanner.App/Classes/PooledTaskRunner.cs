using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;
using Amib.Threading;


namespace ProductScanner.App
{
    /// <summary>
    /// Classes which use the PooledTaskRunner must pass in an input object which implements this interface.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public interface IPooledTaskRunnerInputs<TResult> where TResult: class
    {
        /// <summary>
        /// A string key created on the input information which allows comparison against other similar tasks.
        /// </summary>
        /// <remarks>
        /// Allows similar tasks to be consolidated so they feed from a single answer generation.
        /// </remarks>
        string Key { get; }

        /// <summary>
        /// A method on the input object which knows how to generate the answer.
        /// </summary>
        /// <remarks>
        /// This will be called on a new thread. Callers with the same question will all block on this answer generation.
        /// </remarks>
        /// <returns></returns>
        TResult GenerateAnswer();
    }

    /// <summary>
    /// Perform a long-running task using a queue to throttle CPU, but also gang up threads looking for an answer to the same question.
    /// Generally expected that each T of this class would be a singleton within the app.
    /// </summary>
    /// <remarks>
    /// Calling threads will block until the answer is ready.
    /// </remarks>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public class PooledTaskRunner<TInput, TResult> where TResult: class where TInput : class, IPooledTaskRunnerInputs<TResult>
    {
        private const int MaxGenerationTimeSeconds = 900;

        private class PendingRequestItem
        {
            public int WaitersCount { get; set; }
            public TResult Answer { get; set; }
            public bool IsComplete { get; set; }
            public TInput input { get; set; }

            public PendingRequestItem(TInput input)
            {
                this.input = input;
            }
        }

        private readonly object lockObj = new object();

        private readonly SmartThreadPool generatorThreadPool;
        private readonly Dictionary<string, PendingRequestItem> pendingRequests = new Dictionary<string, PendingRequestItem>();

        /// <summary>
        /// Pooled task running with tuning options.
        /// </summary>
        /// <param name="minWorkerThreads"></param>
        /// <param name="maxWorkerThreads"></param>
        /// <param name="idleTimeout"></param>
        public PooledTaskRunner(int minWorkerThreads, int maxWorkerThreads, int idleTimeout = 5000)
            : this(new STPStartInfo() { MinWorkerThreads = minWorkerThreads, MaxWorkerThreads = maxWorkerThreads, IdleTimeout = idleTimeout })
        {
            
        }


        /// <summary>
        /// Pooled task running with tuning options.
        /// </summary>
        /// <param name="poolInfo"></param>
        public PooledTaskRunner(STPStartInfo poolInfo = null)
	    {

            STPStartInfo generatorPoolInfo = poolInfo;

            if (generatorPoolInfo == null)
                generatorPoolInfo = new STPStartInfo()
                    {
                        MinWorkerThreads = 3,
                        MaxWorkerThreads = 10,
                        IdleTimeout = 5 * 1000,
                    };

                generatorThreadPool = new SmartThreadPool(generatorPoolInfo);            
	    }

        /// <summary>
        /// Return the requested instance, or null if not possible.
        /// </summary>
        /// <remarks>
        /// This call can block until the requested data is available. Callers
        /// are required to take this into account; for example, for ASP.NET
        /// must be called from an async module.
        /// </remarks>
        /// <param name="input">Class with input parameters for the work to perform.</param>
        /// <returns></returns>
        public TResult GenerateResult(TInput input)
        {

            PendingRequestItem reqItem;

            lock (lockObj)
            {
                if (!pendingRequests.TryGetValue(input.Key, out reqItem))
                {
                    // was not on the list, add

                    reqItem = new PendingRequestItem(input);

                    // kick off a task to come up with the answer

                    pendingRequests.Add(input.Key, reqItem);

                    generatorThreadPool.QueueWorkItem(new Amib.Threading.Func<PendingRequestItem, bool>(GeneratorWorker), reqItem);
                }
            }

            lock (reqItem)
            {
                // this is needed to account for the minor gap betwen getting a reference
                // and getting a lock on it.

                if (reqItem.IsComplete)
                    return reqItem.Answer;

                reqItem.WaitersCount++;

                if (!Monitor.Wait(reqItem, TimeSpan.FromSeconds(MaxGenerationTimeSeconds)))
                    return (TResult)null;

                // we got pulsed, so that means we have something in memory,
                // but on fail, logic still works the same

                reqItem.WaitersCount--;
            }

            // answer is either valid or null

            return reqItem.Answer;
        }


        private bool GeneratorWorker(PendingRequestItem reqItem)
        {
            TResult answer = (TResult)null;
            bool bResult = false;

            try
            {
                // this is where the answer is generated
                answer = reqItem.input.GenerateAnswer();

                bResult = true;

            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex);
                bResult = false;
            }

            // cannot leave thread without a clear disposition
            // for all the waiters - good or bad, else they would wait forever.

            lock (lockObj)
            {
                pendingRequests.Remove(reqItem.input.Key);
            }

            lock (reqItem)
            {
                reqItem.IsComplete = true;
                reqItem.Answer = answer;
                Monitor.PulseAll(reqItem);
            }

            return bResult;
        }


    }
}
