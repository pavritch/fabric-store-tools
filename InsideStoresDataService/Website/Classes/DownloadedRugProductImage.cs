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
    /// Presently used only for Rugs.
    /// </summary>
    public class DownloadedRugProductImage : DownloadedProductImage
    {
        #region Locals

        private static List<string> untrustedShapes = new List<string> { "Rectangular", "Square", "Oval", "Round", "Runner"};

          #endregion


        public DownloadedRugProductImage(string SKU, ProductImage productImage, byte[] imageBytes)
            : base(SKU, productImage, imageBytes)
        {

        }

        #region Private Methods


        protected override void ProcessImageExtras()
        {
            // deal with some issues with rugmarket

            if (productImage.SourceUrl.ContainsIgnoreCase("rugmarket") && CheckForRugMarketCopyrightNotice())
            {
                // some rugmarket images have a copyright notice on the left (after rotation)
                ReCropRugMarket();
            }

            SetShape();
        }

        protected override byte[] CropAndRotateImage(byte[] image, out int width, out int height)
        {
            int originalWidth = 0;
            int originalHeight = 0;

            using (var bmp = image.FromImageByteArrayToBitmap())
            {
                // cropping needed?

                // remember dim so can tell if cropped
                originalWidth = bmp.Width;
                originalHeight = bmp.Height;

                var croppedBmp = bmp.CropWhiteSpace(0.998f, 0.97f);

                Func<bool> isScene = () =>
                {
                    // we do not rotate scene images
                    return productImage.ImageVariant == "Scene";
                };

                Func<int, int, bool> hasGenerallySquareDimensions = (w, h) =>
                {
                    // if very close to square, do not rotate, assume as provided
                    // was best vantage for image

                    var ratio = (double)w / (double)h;
                    return Math.Abs(ratio - 1.0) < .05;
                };

                Func<bool> wasCropped = () =>
                {
                    if (croppedBmp.Width == originalWidth && croppedBmp.Height == originalHeight)
                        return false;

                    if (verboseOutput)
                    {
                        var msg = string.Format("    {4} Cropped from ({0} x {1}) to ({2} x {3})", originalWidth, originalHeight, croppedBmp.Width, croppedBmp.Height, productImage.Filename);
                        Debug.WriteLine(msg);
                    }
                    return true;
                };

                if (wasCropped())
                {
                    // rotation needed?

                    if (!isScene() && !hasGenerallySquareDimensions(croppedBmp.Width, croppedBmp.Height) && croppedBmp.Width > croppedBmp.Height)
                    {
                        // these vars only for debug output
                        var crWidth = croppedBmp.Width;
                        var crHeight = croppedBmp.Height;

                        croppedBmp.RotateFlip(RotateFlipType.Rotate90FlipNone);

                        if (verboseOutput)
                        {
                            var msg = string.Format("    {4} Rotated from ({0} x {1}) to ({2} x {3})", crWidth, crHeight, croppedBmp.Width, croppedBmp.Height, productImage.Filename);
                            Debug.WriteLine(msg);
                        }

                    }
                    width = croppedBmp.Width;
                    height = croppedBmp.Height;
                    return croppedBmp.ToJpeg(95);
                }
                else
                {
                    bool isChanged = false;

                    // not cropped, clean up attempted cropped image so don't leak
                    croppedBmp.Dispose();

                    // rotation needed?

                    if (!isScene() && !hasGenerallySquareDimensions(bmp.Width, bmp.Height) && bmp.Width > bmp.Height)
                    {
                        bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        isChanged = true;

                        if (verboseOutput)
                        {
                            var msg = string.Format("    {4} Rotated from ({0} x {1}) to ({2} x {3})", originalWidth, originalHeight, bmp.Width, bmp.Height, productImage.Filename);
                            Debug.WriteLine(msg);
                        }
                    }

                    if (isChanged)
                    {
                        width = bmp.Width;
                        height = bmp.Height;
                        return bmp.ToJpeg(95);
                    }
                }
            }

            // otherwise, return the original image exactly as initially provided
            // to avoid quality reduction

            width = originalWidth;
            height = originalHeight;

            return image;
        }

        private bool CheckForRugMarketCopyrightNotice()
        {
            using (var bmp = CroppedImage.FromImageByteArrayToBitmap())
            {
                var left = (int)(bmp.Width * .07);
                var top = (int)(bmp.Height * .02);

                var stripeWidth = (int)(bmp.Width * .05);
                var stripeHeight = (int)(bmp.Height * .95);

                var r = new Rectangle(left, top, stripeWidth, stripeHeight);

                return bmp.HasEmbeddedWhiteRectangle(r);
            }
        }

        /// <summary>
        /// Handle embedded copyright notices in RugMarket images.
        /// </summary>
        /// <remarks>
        /// Seems to be about 10% of the images. Not yet dealing with drop shadows (a bunch of them).
        /// </remarks>
        /// <returns></returns>
        private void ReCropRugMarket()
        {
            // trim some pixels off the left to skip past the text (give or take) and do the cropping over again from there
            // since now should have a clear shot at whatever white space remains.

            using (var bmp = CroppedImage.FromImageByteArrayToBitmap())
            {
                var trimLeftPixels = (int)(bmp.Width * .07);
                var croppedWidth = bmp.Width - trimLeftPixels;

                using (var trimmedBmp = new Bitmap(croppedWidth, bmp.Height))
                {
                    using (Graphics g = Graphics.FromImage(trimmedBmp))
                    {
                        g.DrawImage(bmp,
                            new RectangleF(0, 0, croppedWidth, bmp.Height),
                            new RectangleF(trimLeftPixels, 0, croppedWidth, bmp.Height),
                            GraphicsUnit.Pixel);
                    }

                    CroppedImage = trimmedBmp.ToJpeg(95);
                }
            }

            int w;
            int h;

            CroppedImage = CropAndRotateImage(CroppedImage, out w, out h);
            CroppedWidth = w;
            CroppedHeight = h;
        }

        private void SetShape()
        {
            Shape = productImage.ImageVariant;

            var statedShape = Shape;

            try
            {
                // many shapes are trusted to be as stated, so we don't make
                // any attempt to look any further

                if (!untrustedShapes.Contains(Shape))
                    return;

                // for any remaining shapes (Rectangular, Square, Oval, Round, Runner), perform
                // some tests to really try to get it right

                // the key here is in the borders

                Func<bool> hasGenerallySquareDimensions = () =>
                    {
                        var ratio = (double)CroppedWidth / (double)CroppedHeight;
                        return Math.Abs(ratio - 1.0) < .05;
                    };

                Func<bool> hasGenerallyRunnerDimensions = () =>
                {
                    var ratio = (double)CroppedWidth / (double)CroppedHeight;
                    return ratio <= .45;
                };

                Func<bool> hasGenerallyRectangularDimensions = () =>
                {
                    var ratio = (double)CroppedWidth / (double)CroppedHeight;
                    return ratio >= .5;
                };

                if (hasGenerallySquareDimensions())
                {
                    // could be square or round, determined by white corners

                    using (var bmp = CroppedImage.FromImageByteArrayToBitmap())
                    {
                        // the actual touching square corner would be closer to .14, but we'll trim that
                        // back to be extra safe.

                        int cornerSize = (int)(CroppedWidth * .10);
                        var isRound = ExtensionMethods.HasWhiteSpaceAroundImage(bmp, cornerSize, 0.998f, 0.90f, 3);

                        Shape = isRound ? "Round" : "Square";
                        return;
                    }
                }

                if (hasGenerallyRunnerDimensions())
                {
                    // likely a runner
                    // for sure, anything with a ratio of under .42 seems to always be a runner
                    // so for now, the test is assumed to be pretty accurate
                    Shape = "Runner";
                    return;
                }

                if (hasGenerallyRectangularDimensions())
                {
                    // oval or rectangular, determined by white corners

                    using (var bmp = CroppedImage.FromImageByteArrayToBitmap())
                    {
                        int cornerSize = (int)(CroppedWidth * .10);
                        var isOval = ExtensionMethods.HasWhiteSpaceAroundImage(bmp, cornerSize, 0.998f, 0.90f, 3);

                        Shape = isOval ? "Oval" : "Rectangular";
                        return;
                    }
                }

                // if all else fails, leave as it was initially stated by the scanner
            }
            finally
            {
                if (verboseOutput)
                {
                    if (statedShape != Shape)
                    {
                        var msg = string.Format("    {4} Shape changed from {0} to {1}.   ({2} x {3})", statedShape, Shape, CroppedWidth, CroppedHeight, productImage.Filename);
                        Debug.WriteLine(msg);
                    }
                    else
                    {
                        var msg = string.Format("    {1} Shape is {0}.", Shape, productImage.Filename);
                        Debug.WriteLine(msg);
                    }
                }
            }
        }

        #endregion

        #region Public Properties


        /// <summary>
        /// Our best guess at the shape of the product.
        /// </summary>
        /// <remarks>
        /// Rectangular, Oval, etc.
        /// </remarks>
        public string Shape { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a max size image suitable to be used as the "original" for the listing image in imagefilenameoverride.
        /// </summary>
        /// <remarks>
        /// Calling logic will pick just one of the available images to be the basis for default/listing image. 
        /// Shown only on listing pages and product feeds. On the detail pages, we instead use the Cropped version.
        /// </remarks>
        /// <returns></returns>
        public override byte[] GetListingImage()
        {
            Func<bool> hasRoundEdges = () =>
            {
                return Shape == "Round" || Shape == "Oval";
            };

            if (!hasRoundEdges())
            {
                // test
                //var filename = @"c:\temp\testimages\listing\" + this.Filename;
                //File.Delete(filename);
                //CroppedImage.WriteBinaryFile(filename);

                return CroppedImage;
            }
            // just Round and Oval here, zoom in to eliminate the white parts in the corners

            // x = R / sqrRoot(2); where x is 1/2 of a side.

            var centerPoint = CroppedWidth / 2.0;

            var radius = centerPoint * 0.95; // shrink in a tiny bit just for fudge factor

            var x = radius / Math.Sqrt(2.0);

            // so we want the square at (radius-x, radius-x), with sizes being 2x

            var sizeSide = (int)(x * 2);

            using (var bmp = CroppedImage.FromImageByteArrayToBitmap())
            {
                using (var bmpSquare = new Bitmap(sizeSide, sizeSide))
                {
                    using (Graphics g = Graphics.FromImage(bmpSquare))
                    {
                        g.DrawImage(bmp,
                          new RectangleF(0, 0, sizeSide, sizeSide),
                          new RectangleF((int)(centerPoint - x), (int)(centerPoint - x), sizeSide, sizeSide),
                          GraphicsUnit.Pixel);
                    }

                    // this version of the image is then passed through our resizing filters...which scale and
                    // figure out compression factors to fit into a byte size...so here we return a very
                    // high quality image since won't be used directly

                    // test
                    //var filename = @"c:\temp\testimages\listing\" + this.Filename;
                    //File.Delete(filename);
                    //bmpSquare.ToJpeg(80).WriteBinaryFile(filename);

                    return bmpSquare.ToJpeg(95);
                }
            }            
        }



        /// <summary>
        /// Return a 800x800 image suitable for CEDD analysis and related tasks.
        /// </summary>
        /// <remarks>
        /// Will only be called for the single image we've decided to use for the starting point
        /// for the analysis image.
        /// </remarks>
        /// <param name="dimension"></param>
        /// <returns></returns>
        public override byte[] GetAnalysisImage()
        {
            switch (Shape)
            {
                case "Rectangular":
                case "Square":
                case "Runner":

                    var centerPointSquare = CroppedWidth / 2.0;
                    var sizeSideSquare = (int)(CroppedWidth * 0.90); // take a slightly embedded square

                    double factorSquare = 800.0 / (double)sizeSideSquare;

                    using (var bmp = CroppedImage.FromImageByteArrayToBitmap())
                    {
                        using (var bmpSquare = new Bitmap(sizeSideSquare, sizeSideSquare))
                        {
                            using (Graphics g = Graphics.FromImage(bmpSquare))
                            {
                                g.DrawImage(bmp,
                                  new RectangleF(0, 0, sizeSideSquare, sizeSideSquare),
                                  new RectangleF((int)(centerPointSquare - (sizeSideSquare / 2.0)), (int)(centerPointSquare - (sizeSideSquare / 2.0)), sizeSideSquare, sizeSideSquare),
                                  GraphicsUnit.Pixel);
                            }

                            using (var bmpResized = new Bitmap(bmpSquare, new Size((int)(bmpSquare.Width * factorSquare), (int)(bmpSquare.Height * factorSquare))))
                            {

                                // test
                                //var filename = @"c:\temp\testimages\cedd\" + this.Filename;
                                //File.Delete(filename);
                                //bmpResized.ToJpeg(80).WriteBinaryFile(filename);

                                return bmpResized.ToJpeg(95);
                            }
                           
                        }
                    }          

                case "Round":
                case "Oval":

                    // get square that fits inside round part, then resize that square to our target 800px

                    var centerPointRound = CroppedWidth / 2.0;

                    var radius = centerPointRound * 0.90; // shrink in a tiny bit just for fudge factor

                    var x = radius / Math.Sqrt(2.0);

                    var sizeSideRound = (int)(x * 2);
                    double factorRound = 800.0 / (double)sizeSideRound;

                    using (var bmp = CroppedImage.FromImageByteArrayToBitmap())
                    {
                        using (var bmpSquare = new Bitmap(sizeSideRound, sizeSideRound))
                        {
                            using (Graphics g = Graphics.FromImage(bmpSquare))
                            {
                                g.DrawImage(bmp,
                                  new RectangleF(0, 0, sizeSideRound, sizeSideRound),
                                  new RectangleF((int)(centerPointRound - x), (int)(centerPointRound - x), sizeSideRound, sizeSideRound),
                                  GraphicsUnit.Pixel);
                            }

                            using (var bmpResized = new Bitmap(bmpSquare, new Size((int)(bmpSquare.Width * factorRound), (int)(bmpSquare.Height * factorRound))))
                            {

                                // test
                                //var filename = @"c:\temp\testimages\cedd\" + this.Filename;
                                //File.Delete(filename);
                                //bmpResized.ToJpeg(80).WriteBinaryFile(filename);

                                return bmpResized.ToJpeg(95);
                            }
                           
                        }
                    }            

                default:

                    // in this rare worse case scenario, note that the entire image is used.

                    double factorDefault = 800.0 / (double)CroppedWidth;

                    using (var bmp = CroppedImage.FromImageByteArrayToBitmap())
                    {
                        using (var bmpResized = new Bitmap(bmp, new Size((int)(bmp.Width * factorDefault), (int)(bmp.Height * factorDefault))))
                        {
                            // test
                            var filename = @"c:\temp\testimages\cedd\" + this.Filename;
                            File.Delete(filename);
                            bmpResized.ToJpeg(80).WriteBinaryFile(filename);


                            return bmpResized.ToJpeg(95);
                        }
                    }
            }

        }

        /// <summary>
        /// Returns the entity we keep in Ext4 to track what we've finally ended up with.
        /// </summary>
        /// <param name="isDefault"></param>
        /// <param name="isDisplayedOnDetailPage"></param>
        /// <returns></returns>
        public override LiveProductImage MakeLiveProductImage(bool isDefault = false, bool isDisplayedOnDetailPage = false)
        {
            var lpi = new LiveProductImage()
            {
                Filename = this.Filename,
                IsGenerated = false,
                IsDefault = isDefault,
                IsDisplayedOnDetailPage = isDisplayedOnDetailPage,
                ImageVariant = this.Shape,
                CreatedOn = DateTime.Now,
                CroppedWidth = this.CroppedWidth,
                CroppedHeight = this.CroppedHeight,
                SourceUrl = this.productImage.SourceUrl,
            };

            return lpi;
        }

        #endregion
    }
}