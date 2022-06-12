using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Media;
using AForge.Imaging.ColorReduction;
using InsideStores.Imaging;

namespace ImageTester
{
    public class ColorSet
    {
        public string Name { get; set; }
        public List<Color> Colors  { get; set; }
        public TimeSpan Duration { get; set; }
        public string Specifications { get; set; }
    }

    public class ImageData
    {
        public string Filepath { get; set; }
        public List<ColorSet> ColorSets;

        public ImageData()
        {
            ColorSets = new List<ColorSet>();
        }

        public string Filename
        {
            get
            {
                return Path.GetFileName(Filepath);
            }
        }
    } 

    public interface GenerateColorSet
    {
        ColorSet Generate();
    }

    public class DomColorsGenerator
    {
        #region Local Classes


        #endregion

        private const string ImageFolder = @"D:\IF-WebV8-Dec27-2012\WebV8\images\product\small";
        private const string HtmlTemplateTop = @"C:\Dev\InsideStores\Projects\InsideStoresDataService\ImageTester\Templates\DomColorsTop.html";
        private const string HtmlTemplateBottom = @"C:\Dev\InsideStores\Projects\InsideStoresDataService\ImageTester\Templates\DomColorsBottom.html";
        private const string OutputFile = @"C:\\temp\DomColors.html";


        /// <summary>
        /// Full filepaths to jpg files. Count is passed in maxCount.
        /// </summary>
        private List<string> RandomFiles;

        private List<ImageData> Images = new List<ImageData>();

        public void Run(int maxCount=100)
        {

            var allFiles = GetAllFilenames();

            RandomFiles = GetRandomFiles(allFiles, maxCount);

            Images = CreateImageData(RandomFiles);

            CreateHtmlPage(Images);

            Debug.WriteLine("break;");
        }

        private List<ImageData> CreateImageData(List<string> inputFiles)
        {
            var images = new List<ImageData>();
            var rnd = new Random();

            foreach(var imageFilepath in inputFiles)
            {
                Debug.WriteLine(string.Format("Processing: {0}", Path.GetFileName(imageFilepath)));
                var record = new ImageData();
                record.Filepath = imageFilepath;

#if true
                //record.ColorSets.Add(new ColorSetOneA(imageFilepath).Generate());
                //record.ColorSets.Add(new ColorSetOneB(imageFilepath).Generate());
                //record.ColorSets.Add(new ColorSetOneC(imageFilepath).Generate());
                //record.ColorSets.Add(new ColorSetOneD(imageFilepath).Generate());
                //record.ColorSets.Add(new ColorSetOneE(imageFilepath).Generate());
                //record.ColorSets.Add(new ColorSetOneF(imageFilepath).Generate());
                //record.ColorSets.Add(new ColorSetOneG(imageFilepath).Generate());
                //record.ColorSets.Add(new ColorSetOneE(imageFilepath).Generate());
                //record.ColorSets.Add(new ColorSetTwo(imageFilepath).Generate());
                //record.ColorSets.Add(new ColorSetThree(imageFilepath).Generate());
                //record.ColorSets.Add(new ColorSetOne(imageFilepath).Generate());

                record.ColorSets.Add(new ColorSetTwo(imageFilepath, 1).Generate());
                record.ColorSets.Add(new ColorSetTwo(imageFilepath, 2).Generate());
                record.ColorSets.Add(new ColorSetTwo(imageFilepath, 3).Generate());
                record.ColorSets.Add(new ColorSetTwo(imageFilepath, 4).Generate());
                record.ColorSets.Add(new ColorSetTwo(imageFilepath, 5).Generate());
                record.ColorSets.Add(new ColorSetTwo(imageFilepath, 6).Generate());
                record.ColorSets.Add(new ColorSetTwo(imageFilepath, 7).Generate());

#else
                // fake color sets
                for(var i=0; i <  7; i++)
                {
                    var cs = new ColorSet();
                    char letterName = (char)('A' + i);
                    cs.Name = letterName.ToString();
                    cs.Colors = new List<Color>();
                    for(int j=0; j < 8; j++)
                    {
                        var r = rnd.Next(0, 256);
                        var g = rnd.Next(0, 256);
                        var b = rnd.Next(0, 256);

                        var color = Color.FromArgb(r, g, b);
                        cs.Colors.Add(color);
                    }

                    record.ColorSets.Add(cs);
                }
#endif

                images.Add(record);
            }

            Debug.WriteLine("Done processing images.");
            return images;
        }

        private void CreateHtmlPage(List<ImageData> imageRecords)
        {
            File.Delete(OutputFile);
            var templateTop = File.ReadAllText(HtmlTemplateTop);
            var templateBottom = File.ReadAllText(HtmlTemplateBottom);

            var sbHtmlOutput = new StringBuilder(1000 * imageRecords.Count());
            sbHtmlOutput.Append(templateTop);

            // do the rows

            foreach(var record in imageRecords)
            {
                var sbRow = new StringBuilder(1000);

                // start row
                sbRow.AppendLine("<div class=\"imageRow\">");

                // image div
                sbRow.AppendFormat("<div class=\"imageColumn primaryImage\"><img src=\"{0}\"></div>\n", record.Filepath);

                // dom colors div
                sbRow.AppendLine("<div class=\"imageColumn domColors\">");

                    foreach(var cs in record.ColorSets)
                    {
                        // single color set
                        sbRow.AppendLine("<div class=\"domColorsRow\">");

                        sbRow.AppendFormat("<div class=\"domColorLabel\" title=\"{1}\">{0}</div>\n", cs.Name, HttpUtility.HtmlEncode(cs.Specifications));

                        foreach(var c in cs.Colors)
                        {
                            sbRow.AppendFormat("<div class=\"domColorBlock\" title=\"{0}\" style=\"background-color:{0};\"></div>\n", string.Format("#{0:X2}{1:X2}{2:X2}", c.R, c.G, c.B));
                        }

                        // end single color set
                        sbRow.AppendLine("</div>");
                    }
                // end dom colors div
                sbRow.AppendLine("</div>");

                // end row
                sbRow.AppendLine("</div>");

                sbHtmlOutput.Append(sbRow);
            }

            sbHtmlOutput.Append(templateBottom);

            File.WriteAllText(OutputFile, sbHtmlOutput.ToString());
        }

        private List<string> GetRandomFiles(List<string> inputFiles, int maxCount)
        {
            var taken = new HashSet<int>();
            var outputFiles = new List<string>();
            var rnd = new Random();

            while (outputFiles.Count < maxCount)
            {
                var pickIndex = rnd.Next(0, inputFiles.Count());
                if (taken.Contains(pickIndex))
                    continue;

                var filename = inputFiles[pickIndex];
                outputFiles.Add(filename);
            }

            return outputFiles;
        }



        private List<string> GetAllFilenames()
        {
            var files = Directory.GetFiles(ImageFolder, "*.jpg").ToList();
            return files;
        }



    }

    #region ColorSet One
    public class ColorSetOne : GenerateColorSet
    {
        string filepath;

        public ColorSetOne(string filepath)
        {
            this.filepath = filepath;
        }

        public ColorSet Generate()
        {
            var colorSet = new ColorSet
            {
                Name = "A"
            };

            colorSet.Colors = new List<Color>();
            var imageBytes = filepath.ReadBinaryFile();
            using (var bmp = imageBytes.FromImageByteArrayToBitmap())
            {
                var bmsrc = bmp.ToBitmapSource();
                var domColors = DominantColors.GetDominantColors(bmsrc, 0.02, 100, 8, 20);
                colorSet.Colors.AddRange(domColors);
            }
            return colorSet;
        }
    }

    public class ColorSetOneBase
    {

        public ColorSetOneBase()
        {
        }

        public ColorSet Generate(string filepath, string name, double samples, int interactions, int number, int similarity)
        {
            var dtStart = DateTime.Now;

            var colorSet = new ColorSet
            {
                Name = name
            };

            colorSet.Colors = new List<Color>();
            var imageBytes = filepath.ReadBinaryFile();
            using (var bmp = imageBytes.FromImageByteArrayToBitmap())
            {
                var bmsrc = bmp.ToBitmapSource();
                var domColors = DominantColors.GetDominantColors(bmsrc, samples, interactions, number, similarity);
                colorSet.Colors.AddRange(domColors);
            }

            var dtEnd =DateTime.Now;
            colorSet.Duration = dtEnd - dtStart;
            colorSet.Specifications = string.Format("{0}: samples: {1}, interactions: {2}, number: {3}, similarity: {4}, duration: {5}", name, samples, interactions, number, similarity, dtEnd - dtStart);
            return colorSet;
        }
    }

    public class ColorSetOneA : ColorSetOneBase, GenerateColorSet
    {
        string filepath;
        public ColorSetOneA(string filepath)
        {
            this.filepath = filepath;
        }

        public ColorSet Generate()
        {
            var cs = base.Generate(filepath, "A", .04, 50, 8, 10);
            return cs;
        }
    }

    public class ColorSetOneB : ColorSetOneBase, GenerateColorSet
    {
        string filepath;
        public ColorSetOneB(string filepath)
        {
            this.filepath = filepath;
        }

        public ColorSet Generate()
        {
            var cs = base.Generate(filepath, "B", .04, 100, 8, 10);
            return cs;
        }
    }

    public class ColorSetOneC : ColorSetOneBase, GenerateColorSet
    {
        string filepath;
        public ColorSetOneC(string filepath)
        {
            this.filepath = filepath;
        }

        public ColorSet Generate()
        {
            var cs = base.Generate(filepath, "C", .04, 200, 8, 10);
            return cs;
        }
    }

    public class ColorSetOneD : ColorSetOneBase, GenerateColorSet
    {
        string filepath;
        public ColorSetOneD(string filepath)
        {
            this.filepath = filepath;
        }

        public ColorSet Generate()
        {
            var cs = base.Generate(filepath, "D", .04, 300, 8, 10);
            return cs;
        }
    }

    public class ColorSetOneE : ColorSetOneBase, GenerateColorSet
    {
        string filepath;
        public ColorSetOneE(string filepath)
        {
            this.filepath = filepath;
        }

        public ColorSet Generate()
        {
            var cs = base.Generate(filepath, "E", .04, 400, 8, 10);
            return cs;
        }

    }

    public class ColorSetOneF : ColorSetOneBase, GenerateColorSet
    {
        string filepath;
        public ColorSetOneF(string filepath)
        {
            this.filepath = filepath;
        }

        public ColorSet Generate()
        {
            var cs = base.Generate(filepath, "F", .04, 500, 8, 10);
            return cs;
        }

    }

    public class ColorSetOneG : ColorSetOneBase, GenerateColorSet
    {
        string filepath;
        public ColorSetOneG(string filepath)
        {
            this.filepath = filepath;
        }

        public ColorSet Generate()
        {
            var cs = base.Generate(filepath, "G", .04, 1000, 8, 10);
            return cs;
        }
    }


    #endregion


    #region ColorSet Two
    public class ColorSetTwo : GenerateColorSet
    {
        string filepath;
        int palCount;

        public ColorSetTwo(string filepath, int palCount)
        {
            this.filepath = filepath;
            this.palCount = palCount;
        }

        public ColorSet Generate()
        {
            Func<System.Drawing.Color, System.Windows.Media.Color> convert = (dc) =>
                {
                    var mc = System.Windows.Media.Color.FromArgb((byte)255, (byte)dc.R, (byte)dc.G, (byte)dc.B);
                    return mc;
                };

            var colorSet = new ColorSet
            {
                Name = string.Format("{0}", palCount)
            };

            var dtStart = DateTime.Now;

            colorSet.Colors = new List<Color>();
            var imageBytes = filepath.ReadBinaryFile();
            using (var bmp = imageBytes.FromImageByteArrayToBitmap())
            {
                ColorImageQuantizer ciq = new ColorImageQuantizer(new MedianCutQuantizer());
                // get 16 color palette for a given image
                var colorTable = ciq.CalculatePalette(bmp, palCount);

                foreach (var c in colorTable)
                    colorSet.Colors.Add(convert(c));
            }

            var dtEnd = DateTime.Now;
            colorSet.Duration = dtEnd - dtStart;
            colorSet.Specifications = string.Format("{0}: palCount: {2},  duration: {1}", colorSet.Name, dtEnd - dtStart, palCount);


            return colorSet;
        }
    }
    #endregion


    #region ColorSet Three
    public class ColorSetThree : GenerateColorSet
    {
        string filepath;

        public ColorSetThree(string filepath)
        {
            this.filepath = filepath;
        }

        public ColorSet Generate()
        {
            var colorSet = new ColorSet
            {
                Name = "C"
            };

            var rnd = new Random(3545);
            // add the colors
            colorSet.Colors = new List<Color>();
            for (int j = 0; j < 8; j++)
            {
                var r = rnd.Next(0, 256);
                var g = rnd.Next(0, 256);
                var b = rnd.Next(0, 256);

                var color = System.Windows.Media.Color.FromArgb((byte)255, (byte)r, (byte)g, (byte)b);
                colorSet.Colors.Add(color);
            }
            return colorSet;
        }
    }
    #endregion

}
