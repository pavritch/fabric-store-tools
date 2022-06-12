using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductScanner.App
{
    /// <summary>
    /// Status for the currently running or just-finished/terminated scanning operation.
    /// </summary>
    /// <remarks>
    /// Note the descriptions have repeats - this is on purpose. We keep track at a granular level,
    /// but report to text status with simple descriptions.
    /// This state is retained in memory until app ends or new operation starts, unless cleared by user.
    /// </remarks>
    public enum ScanningStatus
    {
        [Description("Ready")]
        Idle,

        [Description("Running")]
        Running,

        [Description("Running")]
        RunningWithErrors,

        [Description("Finished")]
        Finished,

        [Description("Finished")]
        FinishedWithErrors,

        [Description("Failed")]
        Failed,

        [Description("Suspended")]
        Suspended,

        [Description("Cancelled")]
        Cancelled,

        [Description("Disabled")]
        Disabled,
    }
}
