using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace Website
{
    public interface IProductCollectionUpdater
    {
        string RunUpdate(CancellationToken cancelToken, Action<string> reportStatusCallback = null);
        string RunRebuildImages(CancellationToken cancelToken, Action<string> reportStatusCallback = null);
    }
}