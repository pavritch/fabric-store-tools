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
    /// Presently used only for Rugs and Inside Avenue.
    /// </summary>
    public class DownloadedProductImage
    {
        #region Locals

        /// <summary>
        /// Set true to enable debug output.
        /// </summary>
        protected bool verboseOutput = false;

        /// <summary>
        /// data from Ext4
        /// </summary>
        protected ProductImage productImage;

        protected string SKU;

        /// <summary>
        /// Already downloaded image bytes, non-null.
        /// </summary>
        /// <remarks>
        /// Already cached (when caching enabled).
        /// </remarks>
        protected byte[] imageBytes;

        protected byte[] comparisonDescriptor = null;

        #endregion


        public DownloadedProductImage(string SKU, ProductImage productImage, byte[] imageBytes)
        {
            this.productImage = productImage;
            this.imageBytes = imageBytes;
            this.SKU = SKU;

            // caller expect to have exception thrown if error processing

            ProcessImage();
        }

        #region protected Methods

        /// <summary>
        /// Perform any needed processing, throw if error.
        /// </summary>
        protected virtual void ProcessImage()
        {
            var isJpeg = imageBytes.HasJpgImagePreamble();

            OriginalAsJpeg = isJpeg ? imageBytes : imageBytes.ToJpeg(98);

            int w;
            int h;
            CroppedImage = CropAndRotateImage(OriginalAsJpeg, out w, out h);
            CroppedWidth = w;
            CroppedHeight = h;

            // any additional processing which might be needed by subclass
            ProcessImageExtras();
        }

        protected virtual void ProcessImageExtras()
        {
            // nothing more in this base class. subclasses can override this.
        }

        protected virtual byte[] CropAndRotateImage(byte[] image, out int width, out int height)
        {
            // this base implementation does not rotate

            int originalWidth = 0;
            int originalHeight = 0;

            using (var bmp = image.FromImageByteArrayToBitmap())
            {
                // cropping needed?

                // remember dim so can tell if cropped
                originalWidth = bmp.Width;
                originalHeight = bmp.Height;

                var croppedBmp = bmp.CropWhiteSpace(0.998f, 0.97f);

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
                    width = croppedBmp.Width;
                    height = croppedBmp.Height;
                    return croppedBmp.ToJpeg(95);
                }
                else
                {
                    // not cropped, clean up attempted cropped image so don't leak
                    croppedBmp.Dispose();
                }
            }

            // otherwise, return the original image exactly as initially provided
            // to avoid quality reduction

            width = originalWidth;
            height = originalHeight;

            return image;
        }

        #endregion

        #region Public Properties

        
        /// <summary>
        /// The true uncropped original, but converted to JPG if not already in that format.
        /// </summary>
        /// <remarks>
        /// Because our rule is that only JPG files get saved in the [original] folder; although
        /// any image format can be saved in the [cache] folder.
        /// </remarks>
        public byte[] OriginalAsJpeg { get; protected set; }


        /// <summary>
        /// The largest possible size for a a cleaned-up/cropped image. This becomes the working image.
        /// </summary>
        /// <remarks>
        /// Cropped and rotated (to vertical orientation) as needed to remove white space.
        /// </remarks>
        public byte[] CroppedImage { get; protected set; }

        /// <summary>
        /// The width of CroppedImage after crop/rotate.
        /// </summary>
        public int CroppedWidth { get; protected set; }

        /// <summary>
        /// The heigth of CroppedImage after crop/rotate.
        /// </summary>
        public int CroppedHeight { get; protected set; }

        /// <summary>
        /// Used generally to tell how big an image is so that we can take the larger
        /// one when finding duplicates.
        /// </summary>
        public int PixelCount
        {
            get
            {
                return CroppedWidth * CroppedHeight;
            }
        }

        public double CroppedAspectRatio
        {
            get
            {
                return (double)CroppedWidth / (double)CroppedHeight;
            }
        }

        /// <summary>
        /// The CEDD descriptor to be used ONLY for file comparisons (looking for duplicates).
        /// </summary>
        /// <remarks>
        /// Likely from CroppedImage resized to something reasonable, then calc CEDD.
        /// </remarks>
        public byte[] ComparisonDescriptor
        {
            get
            {
                if (comparisonDescriptor == null)
                {
                    // make an image with a width of 400 having the same aspect ratio
                    double factor = 400.0 / (double)CroppedWidth;

                    using (var bmp = CroppedImage.FromImageByteArrayToBitmap())
                    {
                        using (var bmpResized = new Bitmap(bmp, new Size((int)(bmp.Width * factor), (int)(bmp.Height * factor))))
                        {
                            if (bmpResized == null)
                                throw new Exception(string.Format("Unable to reduce size for {0} at url {1}", productImage.Filename, productImage.SourceUrl));

                            var bmsrc = bmpResized.ToBitmapSource();
                            comparisonDescriptor = bmsrc.CalculateDescriptor();
                        }
                    }
                }

                return comparisonDescriptor;
            }
        }

        /// <summary>
        /// The filename to be used to save this image in the standard folder set.
        /// </summary>
        /// <remarks>
        /// These are the Original, Large, Medium, Small, Icon, Mini, Micro folders.
        /// </remarks>
        public string Filename
        {
            get
            {
                return productImage.Filename;
            }
        }

        public string ImageVariant
        {
            get
            {
                return productImage.ImageVariant;
            }
        }

        /// <summary>
        /// When not null, the filename of the keeper duplicate; this one not to be live.
        /// </summary>
        public string DuplicateOf { get; set; }

        /// <summary>
        /// True when this is a duplicate image which is to be eliminated (not going live).
        /// </summary>
        public bool IsDuplicate
        {
            get
            {
                return !string.IsNullOrEmpty(DuplicateOf);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a max size image suitable to be used as the "original" for the listing image in imagefilenameoverride.
        /// </summary>
        /// <returns></returns>
        public virtual byte[] GetListingImage()
        {
            // for this base implementation, we don't need to deal with things like round/oval/runner rugs.
            return CroppedImage;
        }


        /// <summary>
        /// Fit the full cropped image into the bounding box. 
        /// </summary>
        /// <param name="maxWidth"></param>
        /// <param name="maxHeight"></param>
        /// <param name="growToFit">If true, small images will be enlarged to fit the box.</param>
        /// <returns></returns>
        public virtual byte[] GetSizeConstrainedCroppedImage(int maxWidth, int maxHeight, bool growToFit=false, int jpgQuality=85)
        {

            if (!growToFit && CroppedWidth <= maxWidth && CroppedHeight <= maxHeight)
            {
                var jpg = CroppedImage.ToJpeg(jpgQuality);
                while (jpg.Length > 1024 * 500)
                {
                    if (verboseOutput)
                    {
                        Debug.WriteLine("-- jpg quality reduction due to size.");
                    }
                    jpgQuality -= 5;
                    jpg = CroppedImage.ToJpeg(jpgQuality);
                }

                if (verboseOutput)
                {
                    Debug.WriteLine(string.Format("GetSizeConstrainedCroppedImage({0}, {1}) yields:  ({2}, {3}) -> ({4}, {5}), size: {6:N0}KB", maxWidth, maxHeight, CroppedWidth, CroppedHeight, CroppedWidth, CroppedHeight, jpg.Length / 1024));
                }
                return jpg;
            }

            // getting to here, we know we want to touch the edge of the bounding box with the largest possible image
            // that will fit.

            // all we really need to do here is come up with the factor, and the rest is a simple resize operation

            var widthScale = maxWidth / (double)CroppedWidth;
            var heightScale = maxHeight / (double)CroppedHeight;
            var factor = Math.Min(widthScale, heightScale);

            using (var bmp = CroppedImage.FromImageByteArrayToBitmap())
            {
                using (var bmpResized = new Bitmap(bmp, new Size((int)(bmp.Width * factor), (int)(bmp.Height * factor))))
                {
                    var jpg = bmpResized.ToJpeg(jpgQuality);

                    while (jpg.Length > 1024 * 500)
                    {
                        if (verboseOutput)
                        {
                            Debug.WriteLine("-- jpg quality reduction due to size.");
                        }
                        jpgQuality -= 5;
                        jpg = bmpResized.ToJpeg(jpgQuality);
                    }

                    if (verboseOutput)
                    {
                        Debug.WriteLine(string.Format("GetSizeConstrainedCroppedImage({0}, {1}) yields:  ({2}, {3}) -> ({4}, {5}), size: {6:N0}KB", maxWidth, maxHeight, CroppedWidth, CroppedHeight, (int)(CroppedWidth * factor), (int)(CroppedHeight * factor), jpg.Length / 1024));
                    }

                    return jpg;

                }
            }
        }

        /// <summary>
        /// The full cropped image embedded on a white square background.
        /// </summary>
        /// <param name="dimension"></param>
        /// <returns></returns>
        public virtual byte[] GetSquareCroppedImage()
        {
            var sizeOfSide = 800;
            var widthScale = sizeOfSide / (double)CroppedWidth;
            var heightScale = sizeOfSide / (double)CroppedHeight;
            var factor = Math.Min(widthScale, heightScale);

            using (var bmp = CroppedImage.FromImageByteArrayToBitmap())
            {
                using (var bmpResized = new Bitmap(bmp, new Size((int)(bmp.Width * factor), (int)(bmp.Height * factor))))
                {
                    // bmpResized: cropped image scaled to fit into a square (800x800)

                    // test
                    //var filename = @"c:\temp\testimages\square-resized\" + this.Filename;
                    //File.Delete(filename);
                    //bmpResized.ToJpeg(80).WriteBinaryFile(filename);


                    using (var bmpSquare = new Bitmap(sizeOfSide, sizeOfSide))
                    {
                        // bmpSquare: is the final bitmap to be returned
                        using (Graphics g = Graphics.FromImage(bmpSquare))
                        {
                            // fill white
                            g.FillRectangle(new SolidBrush(Color.White), 0, 0, sizeOfSide, sizeOfSide);

                            var x = (sizeOfSide - bmpResized.Width) / 2;
                            var y = (sizeOfSide - bmpResized.Height) / 2;

                            g.DrawImage(bmpResized,
                              new RectangleF(x, y, bmpResized.Width, bmpResized.Height),
                              new RectangleF(0, 0, bmpResized.Width, bmpResized.Height),
                              GraphicsUnit.Pixel);
                        }

                        // this version of the image is then passed through our resizing filters...which scale and
                        // figure out compression factors to fit into a byte size...so here we return a very
                        // high quality image since won't be used directly


                        var jpg = bmpSquare.ToJpeg(95);

                        // test
                        //filename = @"c:\temp\testimages\square\" + this.Filename;
                        //File.Delete(filename);
                        //jpg.WriteBinaryFile(filename);

                        return jpg;
                    }
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
        public virtual byte[] GetAnalysisImage()
        {
            switch (productImage.ImageVariant)
            {
                case "Primary":
                case "Scene":

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
        public virtual LiveProductImage MakeLiveProductImage(bool isDefault = false, bool isDisplayedOnDetailPage = false)
        {
            var lpi = new LiveProductImage()
            {
                Filename = this.Filename,
                IsGenerated = false,
                IsDefault = isDefault,
                IsDisplayedOnDetailPage = isDisplayedOnDetailPage,
                ImageVariant = productImage.ImageVariant,
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