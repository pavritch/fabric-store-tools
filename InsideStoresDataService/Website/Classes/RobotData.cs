using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Website
{
    /// <summary>
    /// Class returned to callers requesting all known information about robots.
    /// </summary>
    public class RobotData
    {
        public List<string> GoodAddresses { get; set; }
        public List<string> BadAddresses { get; set; }
        public List<string> GoodAgents { get; set; }
        public List<string> BadAgents { get; set; }
        public List<string> TrapUrls { get; set; }

        /// <summary>
        /// White list is for non-bots (humans).
        /// </summary>
        public List<string> WhitelistAddresses { get; set; }

        public RobotData()
        {
            GoodAddresses = new List<string>();
            BadAddresses = new List<string>();
            GoodAgents = new List<string>();
            BadAgents = new List<string>();
            TrapUrls = new List<string>();
            WhitelistAddresses = new List<string>();
        }

    }
}