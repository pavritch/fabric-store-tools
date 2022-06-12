using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageDownloader
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Begin downloading images.");

            var processor = new Processor();
            //processor.CreatMissingFileList("JP");
            //processor.ProcessFiles();
            processor.CreatMissingFileList();

            Console.WriteLine("Done. Press any key.");
            Console.ReadKey();
        }
    }
}
