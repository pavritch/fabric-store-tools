using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ProductScanner.App
{

    /// <summary>
    /// Sent by any module which wishes to have the system open a file or explorer folder.
    /// </summary>
    class RequestOpenFileOrFolder : IMessage
    {
        public string Path { get; private set; }

        public RequestOpenFileOrFolder(string Path)
        {
            this.Path = Path; 
        }
    }
}
