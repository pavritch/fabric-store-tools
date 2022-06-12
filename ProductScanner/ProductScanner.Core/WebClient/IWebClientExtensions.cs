using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Utilities;
using Utilities.Extensions;

namespace ProductScanner.Core.WebClient
{
    public static class IWebClientExtensions
    {
        private static int _timeout = 600000;
        public static async Task<HtmlNode> PostDataAsync(this IWebClientEx webClient, string url, string uploadData)
        {
            var task = webClient.PostDataAsync(url, uploadData.ToByteArray());
            if (await Task.WhenAny(task, Task.Delay(_timeout)) == task)
            {
                var page = new HtmlDocument();
                page.LoadHtml(task.Result);
                return page.DocumentNode.Clone();
            }
            throw new TimeoutException();
        }

        public static async Task<HtmlNode> DownloadPageAsync(this IWebClientEx webClient, string url, NameValueCollection postValues = null)
        {
            try
            {
                //webClient.TimeoutSeconds = 5000;
                var downloadedPage = await Retry.DoAsync(async () =>
                {
                    Task<string> task;
                    task = postValues != null ? webClient.PostValuesTaskAsync(url, postValues) :
                        webClient.DownloadStringTaskAsync(url);

                    if (await Task.WhenAny(task, Task.Delay(_timeout)) == task)
                    {
                        var page = new HtmlDocument();
                        page.LoadHtml(task.Result);
                        return page.DocumentNode.Clone();
                    }
                    throw new TimeoutException();
                }, TimeSpan.FromSeconds(1), 3);
                return downloadedPage;
            }
            // instead of letting an AggregateException flow up the stack, we'll throw something more descriptive
            catch (AggregateException e)
            {
                var firstEx = e.InnerExceptions.First() as AggregateException;
                if (firstEx != null)
                {
                    var webEx = firstEx.InnerException as WebException;
                    if (webEx != null)
                    {
                        var response = (HttpWebResponse)webEx.Response;
                        if (response == null) throw webEx;
                        var readStream = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                        throw new PageDownloadException(((HttpWebResponse)webEx.Response).StatusCode, url, readStream.ReadToEnd());
                    }
                }
                else
                {
                    var timeoutEx = e.InnerExceptions.First() as TimeoutException;
                    if (timeoutEx != null)
                    {
                        throw timeoutEx;
                    }
                }
                throw new Exception();
            }
        }
    }
}