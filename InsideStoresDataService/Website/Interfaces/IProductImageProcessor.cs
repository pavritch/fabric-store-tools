using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Website.Emails;

namespace Website
{
    /// <summary>
    /// Process entries in SQL table ImageProcessingQueue in accordance with needs of the target website.
    /// </summary>
    public interface IProductImageProcessor
    {
        void ProcessQueue(CancellationToken cancelToken, IProgress<int> progressCallback, Action<string> reportStatusCallback = null);
    }
}