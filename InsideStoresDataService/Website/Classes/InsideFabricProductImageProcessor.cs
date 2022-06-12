using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Windows.Media.Imaging;
using BitMiracle.LibJpeg;
using ExifLibrary;
using InsideFabric.Data;
using InsideStores.Imaging;
using InsideStores.Imaging.Descriptors;
using Website.Entities;
using AForge.Imaging.ColorReduction;

namespace Website
{
    /// <summary>
    /// Main class which knows how to process images from the image queue.
    /// </summary>
    /// <remarks>
    /// For when new or updated images are found by the product scanner. The scanner puts the productID
    /// in the SQL table queue.
    /// </remarks>
    public class InsideFabricProductImageProcessor : ProductImageProcessor, IProductImageProcessor
    {
        public InsideFabricProductImageProcessor(IWebStore store) : base(store)
        {

        }
    }

}
