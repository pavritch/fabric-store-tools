using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Website
{
    /// <summary>
    /// Used by stock checker to weed out bots which are causing calls into stock check web service.
    /// </summary>
    public class IpAddressTracker
    {
        private object lockObj = new object();

        private Dictionary<string, int> dicAddresses; // ip:count
        private DateTime lastReset = DateTime.MinValue; // so will trigger first time

        public IpAddressTracker()
        {
            dicAddresses = new Dictionary<string, int>();
        }

        /// <summary>
        /// Check an IP address, log it, return true if okay to do a stock check, else, false.
        /// </summary>
        /// <remarks>
        /// Funky or missing addresses are allowed to pass through.
        /// </remarks>
        /// <param name="ip"></param>
        /// <returns></returns>
        public bool IsGoodAddress(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip) || ip=="0.0.0.0")
                return true;

            lock(lockObj)
            {
                // clear the ip collection every hour
                if (DateTime.Now - lastReset > TimeSpan.FromHours(1))
                {
                    dicAddresses.Clear();
                    lastReset = DateTime.Now;
                }

                int count;
                if (dicAddresses.TryGetValue(ip, out count))
                {
                    count++;
                }
                else
                {
                    count = 1;    
                }

                // good IP if no more than N hits in the last hour

                dicAddresses[ip] = count;

                return count <= 75;
            }
        }

    }

}