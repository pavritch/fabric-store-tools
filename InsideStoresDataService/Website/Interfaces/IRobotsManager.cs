using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Website.Entities;
using System.Diagnostics;
using System.Globalization;
using System.ComponentModel;
using Gen4.Util.Misc;
using System.Reflection;
using System.Collections.Specialized;
using System.Text;
using System.Threading.Tasks;

namespace Website
{
    /// <summary>
    /// Interface for dealing with robots.
    /// </summary>
    public interface IRobotsManager
    {
        /// <summary>
        /// Adds a new bad robot to the system.
        /// </summary>
        /// <param name="ip">required ip</param>
        /// <param name="agent">optional agent</param>
        /// <returns></returns>
        Task<bool> AddRobot(string ip, string agent);

        /// <summary>
        /// Returns the current state of known robot data.
        /// </summary>
        /// <remarks>
        /// The RobotData class can be directly serialized and returned to callers.
        /// </remarks>
        /// <returns></returns>
        Task<RobotData> GetRobotData();
    }
}