using System;
using System.Threading;
using Newtonsoft.Json;

namespace ReportingEngine.Classes
{
    public class ReportBase
    {
        protected int lastReportedProgress = -1;
        protected CancellationToken CancelToken { get; private set; }
        protected IProgress<int> ProgressIndicator { get; set; }


        public ReportBase(CancellationToken CancelToken = default(CancellationToken))
        {
            this.CancelToken = CancelToken;
        }


        protected bool IsCancelled
        {
            get
            {
                if (CancelToken == null)
                    return false;

                return CancelToken.IsCancellationRequested;
            }
        }

        protected void ThrowOnCancel()
        {
            if (CancelToken != null)
                CancelToken.ThrowIfCancellationRequested();
        }

        protected void ReportProgressPercent(int pct)
        {
            if (ProgressIndicator != null && pct != lastReportedProgress)
            {
                ProgressIndicator.Report(pct);
                lastReportedProgress = pct;
            }
        }

        protected void ReportProgressPercent(int countCompleted, int countTotal)
        {
            var pct = countTotal == 0 ? 0 : (countCompleted * 100) / countTotal;

            ReportProgressPercent(pct);
        }

        /// <summary>
        /// Common settings used for serialization/deserialization.
        /// </summary>
        protected  JsonSerializerSettings SerializerSettings
        {
            get
            {
                var jsonSettings = new JsonSerializerSettings
                {
                    Formatting = Newtonsoft.Json.Formatting.None,
                    TypeNameHandling = TypeNameHandling.None,
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    NullValueHandling = NullValueHandling.Include,
                };

                return jsonSettings;
            }
        }
    }
}
