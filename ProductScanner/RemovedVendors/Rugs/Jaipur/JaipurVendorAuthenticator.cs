using System;
using System.Collections.Specialized;
using System.Text;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using ProductScanner.Core;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.WebClient;

namespace Jaipur
{
  public class JaipurVendorAuthenticator : IVendorAuthenticator<JaipurVendor>
  {
    public async Task<AuthenticationResult> LoginAsync()
    {
      var webClient = new WebClientExtended();
      var vendor = new JaipurVendor();
      var loginPage = await webClient.DownloadPageAsync(vendor.LoginUrl);

      var values = new NameValueCollection();
      values.Add("__LASTFOCUS", "");
      values.Add("__EVENTTARGET", "");
      values.Add("__EVENTARGUMENT", "");
      values.Add("__VIEWSTATEGENERATOR", "C2EE9ABB");
      values.Add("__VIEWSTATE", loginPage.QuerySelector("#__VIEWSTATE").Attributes["value"].Value);
      values.Add("__EVENTVALIDATION", loginPage.QuerySelector("#__EVENTVALIDATION").Attributes["value"].Value);
      //values.Add("__VIEWSTATE", "/wEPDwUKLTUxNDA5ODA3Mw9kFgJmD2QWAgIDD2QWBAIBD2QWBgIBD2QWAgIBD2QWAmYPD2QWAh4Lb25tb3VzZW92ZXIFEFNob3djYXJ0cG9wdXAoKTtkAgIPZBYEAgEPD2QWBB4Hb25DbGljawV9IHZhciBTcmh2YWx1ZT0gZG9jdW1lbnQuZ2V0RWxlbWVudEJ5SWQoJ3R4dFNlYXJjaCcpLnZhbHVlO2lmKFNyaHZhbHVlID09ICcnKXsgZG9jdW1lbnQuZ2V0RWxlbWVudEJ5SWQoJ3R4dFNlYXJjaCcpLnZhbHVlPScnO30eBm9uYmx1cgV8dmFyIFNyaHZhbHVlPSBkb2N1bWVudC5nZXRFbGVtZW50QnlJZCgndHh0U2VhcmNoJykudmFsdWU7aWYoU3JodmFsdWUgPT0gJycpeyBkb2N1bWVudC5nZXRFbGVtZW50QnlJZCgndHh0U2VhcmNoJykudmFsdWU9Jyc7fWQCAw8PFgIeCEltYWdlVXJsBUxodHRwczovL2QzOGM3MXdjcTd6NzhsLmNsb3VkZnJvbnQubmV0L21haW4vc2tpbi9qcmktdjQvaW1hZ2VzL25ldy1zZWFyY2guanBnFgIfAQUicmV0dXJuIHZhbGlkYXRlU2VhcmNoKCd0eHRTZWFyY2gnKWQCAw9kFgQCAQ8PZBYEHwEFjQEgdmFyIFNyaHZhbHVlPSBkb2N1bWVudC5nZXRFbGVtZW50QnlJZCgndGV4dHNlYXJjaF9tb2JpbGUnKS52YWx1ZTtpZihTcmh2YWx1ZSA9PSAnJyl7IGRvY3VtZW50LmdldEVsZW1lbnRCeUlkKCd0ZXh0c2VhcmNoX21vYmlsZScpLnZhbHVlPScnO30fAgWMAXZhciBTcmh2YWx1ZT0gZG9jdW1lbnQuZ2V0RWxlbWVudEJ5SWQoJ3RleHRzZWFyY2hfbW9iaWxlJykudmFsdWU7aWYoU3JodmFsdWUgPT0gJycpeyBkb2N1bWVudC5nZXRFbGVtZW50QnlJZCgndGV4dHNlYXJjaF9tb2JpbGUnKS52YWx1ZT0nJzt9ZAIDDw8WAh8DBUxodHRwczovL2QzOGM3MXdjcTd6NzhsLmNsb3VkZnJvbnQubmV0L21haW4vc2tpbi9qcmktdjQvaW1hZ2VzL25ldy1zZWFyY2guanBnZGQCAw9kFgICAQ9kFgJmD2QWBgIDDw9kFgIeCm9ua2V5cHJlc3MFJHJldHVybiBjbGlja0J1dHRvbihldmVudCwnYnRuTG9naW4nKWQCBQ8PZBYCHgdvbmNsaWNrBYABcmV0dXJuIGNoZWNrdmFsaWRhdGlvbignY3RsMDBfQ29udGVudFBsYWNlSG9sZGVyMV91Y19sb2dpbl90eHRMb2dpbkVtYWlsJywnY3RsMDBfQ29udGVudFBsYWNlSG9sZGVyMV91Y19sb2dpbl90eHRMb2dpblBhc3N3b3JkJylkAgYPDxYCHgtOYXZpZ2F0ZVVybAUVfi9mb3Jnb3RQYXNzd29yZC5hc3B4ZGQYAQUeX19Db250cm9sc1JlcXVpcmVQb3N0QmFja0tleV9fFgIFFGN0bDAwJGhlYWRlcjEkaW1nQnRuBRpjdGwwMCRoZWFkZXIxJGltZ0J0bm1vYmlsZSG62EOrHwuhRuHt0qmIhVAmQ3Bh0l80UTbemj9UYJ8I");
      //values.Add("__EVENTVALIDATION", "/wEdABAwIqCFtGTwa6VDySQGs4NN5I73YFpMUc7HwAhPdaozpZwLKnwTKuopq1O6PA7SKofvpL4+unL3CX0cjpCuuW+pVsxT4E1wk67zTD9/HSJfqyLBH5dt2ucc0AJTFM/KvvGuXzK5dLFTb1ZOSGSz0due2DfrgmrFp6CYtamljqXdzP2iistAHEHH07dj2kFcx8RP9pNhvV2ADhjkSoP2b5tWAACKEKM8rZX/V7aF7/Pm9TLD2NXsKzx5TQQI6C2LEacgm1XhQB/4vO3r9mfyWNsth704yKWGZCuiyrPDDoJ4GyEUZV6TLfMmV0J/e3uo8pXegIUO2JJiZ4RGP1itZuywAxMZ9K/3M4wLdCw0+68tCbcQBChga1e6FsKh8tzQLFM=");
      values.Add("ctl00$header1$txtSearch", "Search");
      values.Add("ctl00$header1$textsearch_mobile", "");
      values.Add("ctl00$header1$hdnpagename", "");
      values.Add("ctl00$header1$hdnusid", "");
      values.Add("ctl00$header1$hdnPageid", "");
      values.Add("ctl00$ContentPlaceHolder1$uc_login$txtLoginEmail", vendor.Username);
      values.Add("ctl00$ContentPlaceHolder1$uc_login$txtLoginPassword", vendor.Password);
      values.Add("ctl00$ContentPlaceHolder1$uc_login$btnLogin", "Login");
      values.Add("ctl00$ContentPlaceHolder1$uc_login$hdn_userid", "");
      values.Add("ctl00$ContentPlaceHolder1$uc_login$hdn_email", "");
      values.Add("ctl00$ContentPlaceHolder1$uc_login$hdn_password", "");
      values.Add("ctl00$ContentPlaceHolder1$uc_login$hdnPtypDatetime", "34##10/05/2017");
      values.Add("ctl00$ContentPlaceHolder1$uc_login$IsSession", "0");

      webClient.Headers["Accept"] = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
      webClient.Headers["Accept-Language"] = "en-US,en;q=0.8,ms;q=0.6";
      webClient.Headers["Accept-Encoding"] = "gzip, deflate, br";
      webClient.Headers["Referer"] = "https://www.jaipurliving.com/login.aspx";

      var login = await webClient.DownloadPageAsync(vendor.LoginUrl, values);
      var rugs = await webClient.DownloadPageAsync("https://www.jaipurliving.com/pillows-throws.aspx");

      var success = rugs.InnerText.Contains("mary joe");
      return new AuthenticationResult(true, webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl)));
    }

    private async Task<HtmlNode> GetGZippedDocument(WebClientExtended webClient, string url)
    {
      var firstPage = await webClient.DownloadDataTaskAsync(url);
      var str = Encoding.UTF8.GetString(firstPage.Decompress());
      var page = new HtmlDocument();
      page.LoadHtml(str);
      return page.DocumentNode.Clone();
    }
  }
}