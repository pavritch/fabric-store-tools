using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using InsideFabric.Data;
using Newtonsoft.Json;

namespace Website
{
    public class FacetSearchLogger
    {
        public class LogRecord
        {
            public DateTime Created { get; set; }
            public string VisitorID { get; set; }
            public FacetSearchCriteria Criteria { get; set; }
            public int PageNo { get; set; }

            public LogRecord()
            {

            }

            public LogRecord(string VisitorID, FacetSearchCriteria Criteria, int PageNo)
            {
                this.Created = DateTime.Now;
                this.VisitorID = VisitorID ?? string.Empty;
                this.Criteria = Criteria;
                this.PageNo = PageNo;
            }
        }

        private IWebStore store;
        private string storeFolder;

        private bool isEnabled;
        private object lockObj = new object();

        public FacetSearchLogger (IWebStore store, string rootPath)
	    {
            isEnabled = false; // default until proven good

            this.store = store;

            if (string.IsNullOrEmpty(rootPath) || !Directory.Exists(rootPath))
                return;

            storeFolder = Path.Combine(rootPath, store.StoreKey.ToString());

            if (!Directory.Exists(storeFolder))
            {
                Directory.CreateDirectory(storeFolder);
                if (!Directory.Exists(storeFolder))
                    return;
            }

            isEnabled = true;
	    }

        private static JsonSerializerSettings SerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None, Formatting = Formatting.None };

        /// <summary>
        /// Logs a single search record.
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="page"></param>
        /// <param name="visitorID">Can be null if not provided.</param>
        public void LogFacetSearch(FacetSearchCriteria criteria, int page, string visitorID)
        {
            if (!isEnabled)
                return;

            var filename = string.Format("{0}-{1:yyyy-MM-dd}.txt", store.StoreKey, DateTime.Now);
            var logFilepath = Path.Combine(storeFolder, filename);

            var record = new LogRecord(visitorID, criteria, page);
            var json = record.ToJSON(SerializerSettings);

            // need to ensure file access is synchronized with lock

            Task.Factory.StartNew(() =>
            {
                try
                {
                    lock(lockObj)
                    {
                        File.AppendAllText(logFilepath, string.Format("{0}\x0d\x0a", json));
                    }
                }
                catch (Exception Ex)
                {
                    Debug.WriteLine("Exception: " + Ex.Message);
                }

            });

        }
    }
}