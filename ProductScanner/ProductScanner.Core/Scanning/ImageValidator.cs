using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using InsideFabric.Data;
using ProductScanner.Core.Scanning.Products.Vendor;
using ProductScanner.Core.Sessions;
using Utilities;

namespace ProductScanner.Core.Scanning
{
    public class NullImageChecker<T> : IImageChecker<T> where T : Vendor
    {
        public bool CheckImage(HttpWebResponse response)
        {
            return response.IsImage();
        }
    }

    public interface IImageChecker<T> where T : Vendor
    {
        bool CheckImage(HttpWebResponse response);
    }

    public interface IImageValidator<T>
    {
        void Validate(List<VendorProduct> vendorProducts);
    }

    public class ImageCheck
    {
        public long Length { get; set; }
        public string Url { get; set; }
        public int Code { get; set; }
    }

    public class ImageValidator<T> : IImageValidator<T> where T : Vendor
    {
        private readonly IVendorScanSessionManager<T> _sessionManager;
        private readonly IImageChecker<T> _imageChecker;

        public ImageValidator(IVendorScanSessionManager<T> sessionManager, IImageChecker<T> imageChecker)
        {
            _sessionManager = sessionManager;
            _imageChecker = imageChecker;
        }

        public virtual void Validate(List<VendorProduct> vendorProducts)
        {
            _sessionManager.ForEachNotify("Searching for images", vendorProducts, product =>
            {
                var validImages = new List<ScannedImage>();
                var prospectiveImages = product.ScannedImages;
                foreach (var prospectiveImage in prospectiveImages)
                {
                    // if this image is a Primary image, but we already have a valid primary image, skip it
                    if (validImages.Any(x => x.ImageVariantType == ImageVariantType.Primary) &&
                        prospectiveImage.ImageVariantType == ImageVariantType.Primary)
                        continue;

                    if (prospectiveImage.Url.StartsWith("ftp"))
                    {
                        if (IsImageAvailableFtp(prospectiveImage.Url, prospectiveImage.NetworkCredential)) validImages.Add(prospectiveImage);
                    }
                    else
                    {
                        var response = GetImage(prospectiveImage.Url);
                        if (response != null && _imageChecker.CheckImage(response)) validImages.Add(prospectiveImage);
                    }
                }
                product.ScannedImages = validImages;
            });
        }

        private HttpWebResponse GetImage(string url)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "HEAD";
                request.Timeout = 1000 * 10;
                request.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US; rv:1.9.2.15) Gecko/20110303 Firefox/3.6.15";

                _sessionManager.BumpVendorRequest();

                var response = (HttpWebResponse)request.GetResponse();
                response.Close();
                return response;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool IsImageAvailableFtp(string url, NetworkCredential credentials)
        {
            var request = WebRequest.Create(url);
            request.Method = WebRequestMethods.Ftp.GetFileSize;
            if (credentials != null) request.Credentials = credentials;
            try
            {
                _sessionManager.BumpVendorRequest();
                request.GetResponse();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}