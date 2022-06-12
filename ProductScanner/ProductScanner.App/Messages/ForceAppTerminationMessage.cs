using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ProductScanner.App
{

    class ForceAppTerminationMessage : IMessage
    {

        public string Message { get; private set; }

        public ForceAppTerminationMessage(string message)
        {
            this.Message = message;
        }
    }
}
