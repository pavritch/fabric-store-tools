using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using ProductScanner.Core;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace Scalamandre.Discovery
{
    public class ScalamandreVendorAuthenticator : IVendorAuthenticator<ScalamandreVendor>
    {
        public async Task<AuthenticationResult> LoginAsync()
        {
            try
            {
                var vendor = new ScalamandreVendor();
                var host = new Uri(vendor.LoginUrl, UriKind.Absolute).Host;
                var webClient = new WebClientExtended();
                var htmlLoginPage = await webClient.DownloadStringTaskAsync(vendor.LoginUrl);

                //if (!htmlLoginPage.HasAllRequiredElements(requiredElements))
                    //throw new Exception("Login failed. Html on login page has changed.");

                // need to send a request which looks like this:
                // GET /wwizv2.asp?wwizmstr=OE.LOGIN2&SUBMIT=VALIDATE&ACCTNO=49329&ZIP=94965&EMAIL=&DATETIME=1386562582740 HTTP/1.1

                var url = string.Format("http://{0}/wwiz.asp?wwizmstr=OE.LOGIN2&SUBMIT=VALIDATE&ACCTNO={1}&ZIP={2}&DATETIME={3}", host, vendor.Username,
                    vendor.Password, DateTime.Now.ConvertDatetimeToUnixTimeStamp());

                var htmlResponse = await webClient.DownloadPageAsync(url);
                var authResult = htmlResponse.InnerHtml.CaptureWithinMatchedPattern(@"<HTML>(?<capture>(.+))</HTML>");

                // which returns a token:
                // <HTML>OE*16779*82964</HTML>

                // the innerHtml contains the session cookie token. If failed, then innerHtml will be: BAD

                // upon success, their browser script sets the following cookies, with tvar being the innerHtml value from the 
                // authentication request

                // expireDate = new Date;
                // expireDate.setDate(expireDate.getDate()+7);
                // document.cookie = "IDCUST="+tvar+";path=/;expires="+ expireDate.toGMTString();
                // document.cookie = "USERTYPE=PRIVATE;path=/;expires="+ expireDate.toGMTString();

                if (authResult == "BAD")
                    throw new Exception("Authentication failed.");

                webClient.Cookies.Add(new Cookie("IDCUST", authResult, "/", host));
                webClient.Cookies.Add(new Cookie("USERTYPE", "PRIVATE", "/", host));

                return new AuthenticationResult(true, webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl)));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }
    }
}