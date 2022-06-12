using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductScanner.App
{

    /// <summary>
    /// The result of an activity.
    /// </summary>
    public enum ActivityResult
    {
        None,
        Success,
        Cancelled,
        Failed
    }
}
