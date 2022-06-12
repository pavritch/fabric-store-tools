using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Transactions;
using Ionic.Zip;
using Gen4.Util.Misc;

namespace Website
{
    /// <summary>
    /// Logic to perform a variety of image-related tasks.
    /// </summary>
    /// <remarks>
    /// Presently only called from CP domain service to deal with InsideAvenue uploads. Does not
    /// currently deal with Original.
    /// </remarks>
    public class ProductImageUnzipManager
    {
        private static readonly string[] skipFolders = new string[]
        {
            "Micro\\",
            "Micro/",
            "Mini\\",
            "Mini/",
            "Icon\\",
            "Icon/",
            "Small\\",
            "Small/",
            "Medium\\",
            "Medium/",
        };

        /// <summary>
        /// Take a zip file of large images and put correctly resized images into product image folders.
        /// </summary>
        /// <param name="Store"></param>
        /// <param name="pathZipFile"></param>
        /// <returns>"OK" for success, else error message to display.</returns>
        public string UnzipAndResize(IWebStore Store, string pathZipFile)
        {

            try
            {

                if (!ZipFile.IsZipFile(pathZipFile))
                    throw new Exception("File is not a valid ZIP.");

                var webRoot = Store.PathWebsiteRoot;

                if (string.IsNullOrWhiteSpace(webRoot) || !Directory.Exists(webRoot))
                    return "Invalid appSetting for WebsiteRootPath.";

                int imageCount = 0;

                using (ZipFile zip = ZipFile.Read(pathZipFile))
                {
                    foreach (ZipEntry zipEntry in zip)
                    {
                        try
                        {
                            if (zipEntry.IsDirectory)
                                continue;

                            // watch out for and old-style zip files. Allow using Large, but anything from Icon or Medium folders
                            // must be skipped.

                            if (skipFolders.Any(e => zipEntry.FileName.ContainsIgnoreCase(e)))
                                continue;

                            var filename = Path.GetFileName(zipEntry.FileName);

                            var ext = Path.GetExtension(filename);
                            if (string.IsNullOrWhiteSpace(ext) || !string.Equals(".jpg", ext, StringComparison.OrdinalIgnoreCase))
                                continue;

                            // to receive the bytes from the jpg in the zip
                            byte[] origImage;

                            using (var stm = new MemoryStream())
                            {
                                zipEntry.Extract(stm);

                                stm.Seek(0, SeekOrigin.Begin);
                                origImage = new byte[(int)zipEntry.UncompressedSize];
                                stm.Read(origImage, 0, origImage.Length);
                            }

                            // now have all the bytes for filename in origImage array

                            // figure out target folder locations

                            var productImageMicro = Path.Combine(new string[] { webRoot, "images", "product", "Micro", filename });
                            var productImageMini = Path.Combine(new string[] { webRoot, "images", "product", "Mini", filename });
                            var productImageIcon = Path.Combine(new string[] { webRoot, "images", "product", "Icon", filename });
                            var productImageSmall = Path.Combine(new string[] { webRoot, "images", "product", "Small", filename });
                            var productImageMedium = Path.Combine(new string[] { webRoot, "images", "product", "Medium", filename });
                            var productImageLarge = Path.Combine(new string[] { webRoot, "images", "product", "Large", filename });

                            // sanity check on image, bail if not looking right...will throw if not image, that's okay too

                            int? Width;
                            int? Height;

                            GetImageDimensions(origImage, out Width, out Height);

                            // skip anything that does not look right

                            if (Width.GetValueOrDefault() < 5 || Height.GetValueOrDefault() < 5)
                                continue;

                            byte[] imgMicro = ResizeImage(origImage, Store.ProductImageWidthMicro);
                            byte[] imgMini = ResizeImage(origImage, Store.ProductImageWidthMini);
                            byte[] imgIcon = ResizeImage(origImage, Store.ProductImageWidthIcon);
                            byte[] imgSmall = ResizeImage(origImage, Store.ProductImageWidthSmall);
                            byte[] imgMedium = ResizeImage(origImage, Store.ProductImageWidthMedium);
                            byte[] imgLarge = ResizeImage(origImage, Store.ProductImageWidthLarge);

#if !DEBUG
                            if (File.Exists(productImageMicro))
                                File.Delete(productImageMicro);

                            if (File.Exists(productImageMini))
                                File.Delete(productImageMini);
                           
                            if (File.Exists(productImageIcon))
                                File.Delete(productImageIcon);

                            if (File.Exists(productImageSmall))
                                File.Delete(productImageSmall);

                            if (File.Exists(productImageMedium))
                                File.Delete(productImageMedium);

                            if (File.Exists(productImageLarge))
                                File.Delete(productImageLarge);

                            WriteBinaryFile(productImageMicro, imgMicro);
                            WriteBinaryFile(productImageMini, imgMini);
                            WriteBinaryFile(productImageIcon, imgIcon);
                            WriteBinaryFile(productImageSmall, imgSmall);
                            WriteBinaryFile(productImageMedium, imgMedium);
                            WriteBinaryFile(productImageLarge, imgLarge);
#endif
                            imageCount++;
                        }
                        catch (Exception Ex)
                        {
                            Debug.WriteLine(Ex.Message);
                        }
                    }

                    return string.Format("OK:{0:N0}", imageCount);
                }
            }
            catch (Exception Ex)
            {
                return string.Format("Exception: {0}", Ex.Message);
            }
        }


        /// <summary>
        /// Updates product record to have an image.
        /// </summary>
        /// <remarks>
        /// Images provided should be the max size available. Will be resized as needed.
        /// </remarks>
        /// <param name="ProductID"></param>
        /// <param name="filename"></param>
        /// <param name="image"></param>
        private string UpdateProductImage(IWebStore Store, int ProductID, string filename, string url)
        {
            // this method never referenced

            try
            {
                if (!url.IsValidAbsoluteUrl())
                {
                    var msg = string.Format("Invalid image url for {0}: {1}", ProductID, url);
                    Debug.WriteLine(msg);
                    return msg;
                }

                if (!IsSafeFilename(filename))
                    filename = string.Format("{0}.jpg", ProductID);

                var webRoot = Store.PathWebsiteRoot;

                if (string.IsNullOrWhiteSpace(webRoot))
                    return "Invalid appSetting for WebsiteRootPath.";

                var productImageMicro = Path.Combine(new string[] { webRoot, "images", "product", "Micro", filename });
                var productImageMini = Path.Combine(new string[] { webRoot, "images", "product", "Mini", filename });
                var productImageIcon = Path.Combine(new string[]   {"images", "product", "Icon", filename});
                var productImageSmall = Path.Combine(new string[] { webRoot, "images", "product", "Small", filename });
                var productImageMedium = Path.Combine(new string[] {"images", "product", "Medium", filename});
                var productImageLarge = Path.Combine(new string[]  {"images", "product", "Large", filename});

                bool bFetchFromWeb = true;
#if true
                if (File.Exists(productImageMicro) && File.Exists(productImageMini) && File.Exists(productImageIcon) && File.Exists(productImageSmall) && File.Exists(productImageMedium) && File.Exists(productImageLarge))
                    bFetchFromWeb = false;
#endif

                if (bFetchFromWeb)
                {
                    var image = GetProductImageFromWeb(url);
                    if (image == null)
                    {
                        var msg = string.Format("No image for {0} at url {1}", ProductID, url);
                        Debug.WriteLine(msg);
                        return msg;
                    }

                    Debug.WriteLine(string.Format("Found image for {0}", ProductID));

                    // sanity check on image, bail if not looking right...will throw if not image, that's okay too

                    int? Width;
                    int? Height;

                    GetImageDimensions(image, out Width, out Height);

                    if (Width.GetValueOrDefault() < 5 || Height.GetValueOrDefault() < 5)
                        return "Image size less than 5x5 pixels.";

                    if (File.Exists(productImageMicro))
                        File.Delete(productImageMicro);

                    if (File.Exists(productImageMini))
                        File.Delete(productImageMini);

                    if (File.Exists(productImageIcon))
                        File.Delete(productImageIcon);

                    if (File.Exists(productImageSmall))
                        File.Delete(productImageSmall);

                    if (File.Exists(productImageMedium))
                        File.Delete(productImageMedium);

                    if (File.Exists(productImageLarge))
                        File.Delete(productImageLarge);

                    byte[] imgMicro = ResizeImage(image, Store.ProductImageWidthMicro);
                    byte[] imgMini = ResizeImage(image, Store.ProductImageWidthMini);
                    byte[] imgIcon = ResizeImage(image, Store.ProductImageWidthIcon);
                    byte[] imgSmall = ResizeImage(image, Store.ProductImageWidthSmall);
                    byte[] imgMedium = ResizeImage(image, Store.ProductImageWidthMedium);
                    byte[] imgLarge = ResizeImage(image, Store.ProductImageWidthLarge);

                    WriteBinaryFile(productImageMicro, imgMicro);
                    WriteBinaryFile(productImageMini, imgMini);
                    WriteBinaryFile(productImageIcon, imgIcon);
                    WriteBinaryFile(productImageSmall, imgSmall);
                    WriteBinaryFile(productImageMedium, imgMedium);
                    WriteBinaryFile(productImageLarge, imgLarge);
                }

                using (var dc = new AspStoreDataContext(Store.ConnectionString))
                {
                    using (var scope = new TransactionScope())
                    {
                        var product = dc.Products.Where(e => e.ProductID == ProductID).FirstOrDefault();
                        if (product == null)
                            return string.Format("Unable to locate SQL record for product {0}.", ProductID);

                        product.ImageFilenameOverride = filename;
                        dc.SubmitChanges();

                        scope.Complete();
                    }
                }

                return "OK";

            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.ToString());
                return String.Format("Exception: {0}", Ex.Message);
            }

        }

        private void GetImageDimensions(byte[] ContentData, out int? Width, out int? Height)
        {
            Width = null;
            Height = null;

            if (ContentData.Length > 0)
            {
                try
                {
                    using (var bmp = ContentData.FromImageByteArrayToBitmap())
                    {
                        Width = bmp.Width;
                        Height = bmp.Height;
                    }
                }
                catch
                {
                }
            }
        }



        private byte[] ResizeImage(byte[] originalContent, int newWidth)
        {

            int? Width;
            int? Height;

            GetImageDimensions(originalContent, out Width, out Height);

            // if original content already smaller than desired size, no need to resize smaller, keep what we have

            if (Width.HasValue && Width.Value <= newWidth)
            {
#if DEBUG
                //Debug.WriteLine(string.Format("Want {2}, Not resized: {0}x{1}", Width.Value, Height.Value, newWidth));
#endif
                return originalContent;
            }
            var imgElem = new Neodynamic.WebControls.ImageDraw.ImageElement { Source = Neodynamic.WebControls.ImageDraw.ImageSource.Binary, SourceBinary = originalContent, PreserveMetaData = false };

            var imgDraw = new Neodynamic.WebControls.ImageDraw.ImageDraw();
            imgDraw.Canvas.AutoSize = true;

            //Create an instance of Resize class
            var resize = new Neodynamic.WebControls.ImageDraw.Resize
            {
                Width = newWidth,
                //Height = imageSize,
                LockAspectRatio = Neodynamic.WebControls.ImageDraw.LockAspectRatio.WidthBased
            };

            //Apply the action on the ImageElement
            imgElem.Actions.Add(resize);

            imgDraw.Elements.Add(imgElem);
            imgDraw.ImageFormat = Neodynamic.WebControls.ImageDraw.ImageDrawFormat.Jpeg;
            imgDraw.JpegCompressionLevel = 80;

            var resizedImage = imgDraw.GetOutputImageBinary();
#if false
            int? WidthAfter;
            int? HeightAfter;

            GetImageDimensions(resizedImage, out WidthAfter, out HeightAfter);
            Debug.WriteLine(string.Format("Want {4}, Resized from {0}x{1} to {2}x{3}", Width.Value, Height.Value, WidthAfter.Value, HeightAfter.Value, newWidth));
#endif
            return resizedImage;
        }


        private Bitmap FromImageByteArrayToBitmap(byte[] ContentData)
        {
            try
            {
                if (ContentData.Length > 0)
                {
                    using (var stream = new MemoryStream(ContentData.Length))
                    {
                        stream.Write(ContentData, 0, ContentData.Length);
                        stream.Position = 0;

                        var bmp = new Bitmap(stream);
                        return bmp;
                    }
                }
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
            }

            return null;
        }


        private void WriteBinaryFile(string filepath, byte[] data)
        {
            using (var fs = new FileStream(filepath, FileMode.CreateNew, FileAccess.Write))
            {
                fs.Write(data, 0, data.Length);
            }
        }

        private byte[] GetProductImageFromWeb(string Url)
        {
            try
            {
                WebClient client = new WebClient();

                var image = client.DownloadData(Url);

                if (client.ResponseHeaders["Content-Type"].IndexOf("image") == -1)
                    return null;

                return image;
            }
            catch (Exception Ex)
            {
                Debug.WriteLine(Ex.Message);
            }

            return null;
        }


        private bool IsSafeFilename(string filename)
        {
            foreach (var c in filename)
            {
                if (c >= 'A' && c <= 'Z')
                    continue;

                if (c >= 'a' && c <= 'z')
                    continue;

                if (c >= '0' && c <= '9')
                    continue;

                if (".-_".Contains(c))
                    continue;

                return false;
            }

            return true;
        }
        
    }
}