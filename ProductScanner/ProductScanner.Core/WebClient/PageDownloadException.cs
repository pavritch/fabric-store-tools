using System;
using System.Net;
using HtmlAgilityPack;

namespace ProductScanner.Core.WebClient
{
    public class PageDownloadException : Exception
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Url { get; set; }
        public HtmlNode ReceivedPage { get; set; }

        public PageDownloadException(HttpStatusCode statusCode, string url, string response)
        {
            StatusCode = statusCode;
            Url = url;

            var page = new HtmlDocument();
            page.LoadHtml(response);
            ReceivedPage = page.DocumentNode.Clone();
        }
    }

}