using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Utilities
{
    public interface IFtpClient
    {
        bool IsAvailable(NetworkCredential credentials, string url);
        Task<List<string>> DirectoryListingAsync(NetworkCredential credentials, string url);
    }

    public class FtpClient : IFtpClient
    {
        public bool IsAvailable(NetworkCredential credentials, string url)
        {
            var request = (FtpWebRequest)WebRequest.Create(url);
            request.Method = WebRequestMethods.Ftp.ListDirectory;
            request.Credentials = credentials;
            request.UseBinary = false;
            request.Timeout = 200 * 1000;

            using (var resp = (FtpWebResponse)request.GetResponse())
            {
                return resp.StatusCode == FtpStatusCode.CommandOK ||
                       resp.StatusCode == FtpStatusCode.DataAlreadyOpen;
            }
        }

        public async Task<List<string>> DirectoryListingAsync(NetworkCredential credentials, string url)
        {
            var listing = await Retry.DoAsync(async () =>
            {
                var request = (FtpWebRequest) WebRequest.Create(url);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = credentials;
                request.UseBinary = false;
                request.Timeout = 200*1000;
                request.KeepAlive = true;

                var list = new List<string>();
                using (var resp = (FtpWebResponse) await request.GetResponseAsync())
                {
                    var responseStream = resp.GetResponseStream();
                    using (var reader = new StreamReader(responseStream))
                    {
                        var line = reader.ReadLine();
                        while (!string.IsNullOrEmpty(line))
                        {
                            list.Add(line);
                            line = reader.ReadLine();
                        }
                    }
                }
                return list.Where(x => x != "." && x != ".." && x != "Thumbs.db").ToList();
            }, TimeSpan.FromSeconds(3));
            return listing;
        }
    }
}