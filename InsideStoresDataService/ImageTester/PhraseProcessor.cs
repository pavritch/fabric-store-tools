using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageTester
{
    public class PhraseProcessor
    {

        public void Run(string inputFile, string outputFile)
        {
            Debug.WriteLine(string.Format("Reading file: {0}", Path.GetFileName(inputFile)));

            var distinctSet = new HashSet<string>();

            var lines = File.ReadAllLines(inputFile);
            foreach(var line in lines.Where(e => !string.IsNullOrWhiteSpace(e)))
                distinctSet.Add(line.ToLower());

            Debug.WriteLine(string.Format("Writing file: {0}", Path.GetFileName(outputFile)));

            File.Delete(outputFile);
            File.WriteAllLines(outputFile, distinctSet.OrderBy(e => e).ToList());
        }
    }
}
