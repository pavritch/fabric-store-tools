using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductScanner.App
{

    /// <summary>
    /// The current status/state for a vendor test.
    /// </summary>
    public enum TestStatus
    {
        Unknown,
        Running,
        Successful,
        Failed,
        Cancelled,
        Skipped,
    }
}
