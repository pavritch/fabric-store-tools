using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Website
{
    /// <summary>
    /// Singleton global manager for anything releated to robots.
    /// </summary>
    /// <remarks>
    /// The robots controller calls this class to do all the work.
    /// </remarks>
    public class RobotsManager : IRobotsManager
    {

        #region Constants
        private const string GOOD_IP_FILESPEC = "good-ip-*.txt";
        private const string BAD_IP_FILESPEC = "bad-ip-*.txt";
        private const string WHITELIST_IP_FILESPEC = "whitelist-ip-*.txt";

        private const string GOOD_AGENT_FILESPEC = "good-agent-*.txt";
        private const string BAD_AGENT_FILESPEC = "bad-agent-*.txt";

        private const string TRAP_URLS_FILENAME = "trap-urls.txt";
        private const string DEFAULT_BAD_ID_FILENAME = "bad-ip-default.txt";

        private const string REGEX_IPV4 = @"^([0-9]{1,3}\.){3}[0-9]{1,3}$";

        #endregion

        #region Locals
        /// <summary>
        /// We want to delay initialization until we come in on a real HTTP request so we have a 
        /// HTTP Context to perform MapPath.
        /// </summary>
        private bool isFolderPathInitialized = false;

        private string robotsDataRootFolder;
        private object lockObj = new object(); 
        #endregion

        public RobotsManager(string robotsDataRootFolder)
        {
            this.robotsDataRootFolder = robotsDataRootFolder;
        }

        /// <summary>
        /// Return the root path where we keep the robot file data.
        /// </summary>
        private string RobotsFolder
        {
            get
            {
                return robotsDataRootFolder;
            }
        }

        /// <summary>
        /// Given just the filename component, combine with robots folder root.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private string MakeFilepath(string filename)
        {
            return Path.Combine(RobotsFolder, filename);
        }

        private List<string> GetFilenames(string filespec)
        {
            return Directory.GetFiles(RobotsFolder, filespec, SearchOption.TopDirectoryOnly).ToList();
        }

        private void InitializeFolderPath()
        {
            lock (this)
            {
                if (isFolderPathInitialized)
                    return;

                // just in case we happen to use App_Data; which generally would be only for debugging

                if (HttpContext.Current != null && robotsDataRootFolder.StartsWith("~"))
                    robotsDataRootFolder = HttpContext.Current.Server.MapPath(robotsDataRootFolder);

                isFolderPathInitialized = true;
            }
        }

        /// <summary>
        /// Adds a new bad robot to the system.
        /// </summary>
        /// <param name="ip">required ip</param>
        /// <param name="agent">optional agent</param>
        /// <returns></returns>
        public Task<bool> AddRobot(string ip, string agent=null)
        {
            if (!isFolderPathInitialized)
                InitializeFolderPath();

            var tcs = new TaskCompletionSource<bool>();
            Task.Factory.StartNew(() =>
            {
                lock(lockObj)
                { 
                    var existingData = CreateRobotData();

                    // make sure not already on any existing list

                    bool result = false;
                    if (!existingData.BadAddresses.Contains(ip) && !existingData.GoodAddresses.Contains(ip) && !existingData.WhitelistAddresses.Contains(ip))
                    {
                        var line = (agent == null) ? ip : string.Format("{0} # {1}", ip, agent);

                        AppendFile(DEFAULT_BAD_ID_FILENAME, line);
                        result = true;
                        Debug.WriteLine(string.Format("Added bad bot to list: {0}    {1}", ip, agent ?? "(no agent)"));
                    }

                    tcs.SetResult(result);
                }
            });
            return tcs.Task;
         
        }

        /// <summary>
        /// Returns the current state of known robot data.
        /// </summary>
        /// <remarks>
        /// The RobotData class can be directly serialized and returned to callers.
        /// </remarks>
        /// <returns></returns>
        public Task<RobotData> GetRobotData()
        {
            if (!isFolderPathInitialized)
                InitializeFolderPath();

            var tcs = new TaskCompletionSource<RobotData>();
            Task.Factory.StartNew(() =>
            {
                lock (lockObj)
                {
                    var result = CreateRobotData();
                    tcs.SetResult(result);
                }
            });
            return tcs.Task;
        } 

        private RobotData CreateRobotData()
        {
            var data = new RobotData()
            {
                GoodAddresses = ReadFiles(GOOD_IP_FILESPEC, REGEX_IPV4),
                BadAddresses = ReadFiles(BAD_IP_FILESPEC, REGEX_IPV4),
                WhitelistAddresses = ReadFiles(WHITELIST_IP_FILESPEC, REGEX_IPV4),
                GoodAgents = ReadFiles(GOOD_AGENT_FILESPEC),
                BadAgents = ReadFiles(BAD_AGENT_FILESPEC),
                TrapUrls = ReadFile(MakeFilepath(TRAP_URLS_FILENAME))
            };

            // validate, remove from "bad" what is already in "good" (just in case)

            data.BadAddresses = data.BadAddresses.Where(e => !data.GoodAddresses.Contains(e) && !data.WhitelistAddresses.Contains(e)).ToList();
            data.BadAgents = data.BadAgents.Where(e => !data.GoodAgents.Contains(e)).ToList();

            return data;
        }

        #region Read and Write Files

        /// <summary>
        /// Read the named filepath and return a list of clean strings.
        /// </summary>
        /// <remarks>
        /// Comments are stripped out, strings are trimmed.
        /// </remarks>
        /// <param name="filename"></param>
        /// <returns></returns>
        private List<string> ReadFile(string filepath)
        {
            // hash guarantees no duplicates
            var data = new HashSet<string>();

            var lines = File.ReadAllLines(filepath);
            foreach (var line in lines)
            {
                // skip lines that begin with comment # or if blank
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                    continue;

                // could have spaces and # comments
                var ary = line.Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
                if (ary.Length == 0 || string.IsNullOrWhiteSpace(ary[0]))
                    continue;

                data.Add(ary[0].Trim());
            }

            return data.ToList();
        }

        /// <summary>
        /// Returns a de-duped clean list of strings from union of matched files in the robots folder.
        /// </summary>
        /// <param name="filespec"></param>
        /// <param name="validationRegex">optional validation check</param>
        /// <returns></returns>
        private List<string> ReadFiles(string filespec, string validationRegex = null)
        {
            // use a hash to guarantee no duplicates; order is not important

            var data = new HashSet<string>();

            foreach (var file in GetFilenames(filespec))
            {
                foreach (var item in ReadFile(file))
                {
                    var lowerItem = item.ToLower();
                    if (validationRegex != null && !Regex.IsMatch(lowerItem, validationRegex))
                        continue;

                    data.Add(lowerItem);
                }
            }

            return data.ToList();
        }

        /// <summary>
        /// Append the given string s to the file in the robots folder.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="s"></param>
        private void AppendFile(string filename, string s)
        {
            var filepath = MakeFilepath(filename);

            if (!File.Exists(filepath))
                return;

            File.AppendAllLines(filepath, new List<string>() { s });
        } 
        #endregion
    }
}