using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace ProductScanner.App
{
        public class ImageAsyncHelper : FrameworkElement
        {
            public static string GetSourceUrl(DependencyObject obj) { return (string)obj.GetValue(SourceUrlProperty); }
            public static void SetSourceUrl(DependencyObject obj, Uri value) { obj.SetValue(SourceUrlProperty, value); }
    
            public static readonly DependencyProperty SourceUrlProperty =
                DependencyProperty.RegisterAttached("SourceUrl", 
                typeof(string), typeof(ImageAsyncHelper), new PropertyMetadata
                (null, async (d, e) =>
                {
                    try
                    {
                        Image imgControl = (Image)d;
                        string address = (string)e.NewValue;
                        if (string.IsNullOrWhiteSpace(address))
                            return;

                        var imgSrc = await ImageAsyncHelper.LoadImageSourceAsync(address);
                        imgControl.Source = imgSrc;
                    }
                    catch
                    {
                    }
                }));
        

            private static async Task<ImageSource> LoadImageSourceAsync(string address)
            {
                ImageSource imgSource = null;

                try
                {
                    MemoryStream ms = new MemoryStream(await new WebClient().DownloadDataTaskAsync(new Uri(address, UriKind.RelativeOrAbsolute)));
                    ImageSourceConverter imageSourceConverter = new ImageSourceConverter();
                    imgSource = (ImageSource)imageSourceConverter.ConvertFrom(ms);

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

                return imgSource;
            }
    }
}
