namespace Loloi.Previous
{
    /*public class LoloiVendorAuthenticator : IVendorAuthenticator<LoloiVendor>
    {
        public async Task<AuthenticationResult> LoginAsync()
        {
            var webClient = new WebClientExtended();
            var vendor = new LoloiVendor();

            var nvCol = new NameValueCollection();
            nvCol.Add("Username", vendor.Username);
            nvCol.Add("Password", vendor.Password);

            var strResponse = await webClient.PostValuesTaskAsync(vendor.LoginUrl, nvCol);
            if (strResponse.Contains("\"success\":true"))
                return new AuthenticationResult(true, webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl)));
            return new AuthenticationResult(false);
        }
    }*/
}