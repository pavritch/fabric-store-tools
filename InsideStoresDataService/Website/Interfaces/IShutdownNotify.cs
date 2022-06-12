//------------------------------------------------------------------------------
// 
// Interface: IShutdownNotify 
//
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Website
{
    /// <summary>
    /// Interface used to signify that the class wishes to be notified when ASP.NET is shutting down.
    /// </summary>
    /// <remarks>
    /// Intended to give classes an opportunity to gracefully terminate - give up threads, etc.
    /// </remarks>
    public interface IShutdownNotify
    {
        void Stop();
    }
}