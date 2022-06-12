using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyCommon
{
    /// <summary>
    /// Keeps track of API calls to ensure not going too fast.
    /// </summary>
    public class ShopifyApiSentry
    {
        private int bucketSize;
        private int leakRate; // milliseconds
        //private bool isShuttingDown = false;

        /// <summary>
        /// Initialize sentry.
        /// </summary>
        /// <param name="bucketSize">Max size of the bucket, generally 40</param>
        /// <param name="leakRate">Delay between leakage (requests) in milliseconds, generally 2 per second (500ms)</param>
        public ShopifyApiSentry(int bucketSize, int leakRate)
        {
            this.bucketSize = bucketSize;
            this.leakRate = leakRate;
            

        }

        public void ShutDown()
        {
            //isShuttingDown = true;
        }

        /// <summary>
        /// Entry point to be used prior to making any API call to ensure not going too fast.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> WaitMyTurn()
        {
            // dummy implementation
            await Task.Delay(leakRate);
            return true;
        }
    }
}
