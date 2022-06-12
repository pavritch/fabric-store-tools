using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ProductScanner.App
{

    class RequestExportTextFile : IMessage
    {
        public List<string> TextLines { get; set; }
        public string SuggestedFilename { get; set; }

        public RequestExportTextFile(List<string> textLines, string suggestedFilename=null )
        {
            this.TextLines = textLines;
            this.SuggestedFilename = suggestedFilename;
        }
    }
}
