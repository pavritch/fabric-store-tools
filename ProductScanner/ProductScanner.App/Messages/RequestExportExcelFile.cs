using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ProductScanner.App
{

    class RequestExportExcelFile<T> : IMessage
    {
        public IEnumerable<T> DataSet { get; set; }
        public string SuggestedFilename { get; set; }

        public RequestExportExcelFile(IEnumerable<T> dataSet, string suggestedFilename = null)
        {
            this.DataSet = dataSet;
            this.SuggestedFilename = suggestedFilename;
        }
    }
}
