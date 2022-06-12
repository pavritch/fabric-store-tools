using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductScanner.App
{
    /// <summary>
    /// A vendor will always be in one of these states with regard to scanning.
    /// </summary>
    /// <remarks>
    /// These states are exclusive.
    /// </remarks>
    public enum ScannerState
    {
        /// <summary>
        /// Not doing anything. No checkpoints, nothing to commit.
        /// </summary>
        Idle,

        /// <summary>
        /// Presently scanning.
        /// </summary>
        Scanning,

        /// <summary>
        /// Not scanning, but has a valid checkpoint which would allow scanning to be resumed.
        /// </summary>
        Suspended,

        /// <summary>
        /// Not scanning or suspended, but has at least one batch ready to commit.
        /// </summary>
        Committable,

        /// <summary>
        /// Specifically disabled so cannot participate in any scanning. Likey means vendor is broken.
        /// </summary>
        Disabled

    }
}
