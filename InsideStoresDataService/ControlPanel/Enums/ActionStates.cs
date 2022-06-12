using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ControlPanel
{
    /// <summary>
    /// Used to indicate the state of an action or operation through
    /// the RIA service. Repopulate cache, etc.
    /// </summary>
    public enum ActionStates
    {
        /// <summary>
        /// No action. None has been executed. Generally the starting state.
        /// </summary>
        Default,

        /// <summary>
        /// Action is presently executing.
        /// </summary>
        Executing,

        /// <summary>
        /// Action completed successfully.
        /// </summary>
        Success,

        /// <summary>
        /// Action completed in error or warning state.
        /// </summary>
        Warning
    }
}
