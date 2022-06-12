using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using ProductScanner.Core.Authentication;
using ProductScanner.Core.WebClient;
using Utilities.Extensions;

namespace GoHomeLTD
{
    public class GoHomeVendorAuthenticator : IVendorAuthenticator<GoHomeVendor>
    {
        public async Task<AuthenticationResult> LoginAsync()
        {
            var vendor = new GoHomeVendor();
            var webClient = new WebClientExtended();

            var nvCol = new NameValueCollection();
            nvCol.Add("__EVENTTARGET", "");
            nvCol.Add("__EVENTARGUMENT", "");
            nvCol.Add("__VIEWSTATE", "/wEPDwUKLTE4MDM3MzA3OA9kFgJmD2QWAgIDD2QWCgIDDxYCHglpbm5lcmh0bWwF7T08dWw+PGxpPjxhIGNsYXNzPSdDbGVhclNlc3Npb24nIGhyZWY9J1NlYXJjaC5hc3B4JyBjbGFzcz0nTWFpbkxpbmsnPkNvbGxlY3Rpb25zPC9hPjx1bD48bGk+PGEgY2xhc3M9J0NsZWFyU2Vzc2lvbicgaHJlZj0nU2VhcmNoLmFzcHg/Q29sbGVjdGlvbj0xJz5Db3VudHJ5IENoaWM8L2E+PC9saT48bGk+PGEgY2xhc3M9J0NsZWFyU2Vzc2lvbicgaHJlZj0nU2VhcmNoLmFzcHg/Q29sbGVjdGlvbj0yJz5WaW50YWdlIEZhcm1ob3VzZTwvYT48L2xpPjxsaT48YSBjbGFzcz0nQ2xlYXJTZXNzaW9uJyBocmVmPSdTZWFyY2guYXNweD9Db2xsZWN0aW9uPTMnPktlbnNpbmd0b248L2E+PC9saT48bGk+PGEgY2xhc3M9J0NsZWFyU2Vzc2lvbicgaHJlZj0nU2VhcmNoLmFzcHg/Q29sbGVjdGlvbj00Jz5WZW5ldGlhbjwvYT48L2xpPjxsaT48YSBjbGFzcz0nQ2xlYXJTZXNzaW9uJyBocmVmPSdTZWFyY2guYXNweD9Db2xsZWN0aW9uPTUnPkNvbnNlcnZhdG9yeSBHYXJkZW48L2E+PC9saT48bGk+PGEgY2xhc3M9J0NsZWFyU2Vzc2lvbicgaHJlZj0nU2VhcmNoLmFzcHg/Q29sbGVjdGlvbj02Jz5Ccml0aXNoIElzbGU8L2E+PC9saT48bGk+PGEgY2xhc3M9J0NsZWFyU2Vzc2lvbicgaHJlZj0nU2VhcmNoLmFzcHg/Q29sbGVjdGlvbj03Jz5UdXNjYW4gVmlsbGE8L2E+PC9saT48bGk+PGEgY2xhc3M9J0NsZWFyU2Vzc2lvbicgaHJlZj0nU2VhcmNoLmFzcHg/Q29sbGVjdGlvbj04Jz5IaXAgVmludGFnZTwvYT48L2xpPjxsaT48YSBjbGFzcz0nQ2xlYXJTZXNzaW9uJyBocmVmPSdTZWFyY2guYXNweD9Db2xsZWN0aW9uPTknPkRlc2lnbmVyIEJvb2tzPC9hPjwvbGk+PGxpPjxhIGNsYXNzPSdDbGVhclNlc3Npb24nIGhyZWY9J1NlYXJjaC5hc3B4P0NvbGxlY3Rpb249MTAnPkJlIFNlYXRlZDwvYT48L2xpPjxsaT48YSBjbGFzcz0nQ2xlYXJTZXNzaW9uJyBocmVmPSdTZWFyY2guYXNweCc+VmlldyBBbGw8L2E+PC9saT48L3VsPjwvbGk+PGxpPjxhIGNsYXNzPSdDbGVhclNlc3Npb24nIGhyZWY9J1NlYXJjaC5hc3B4JyBjbGFzcz0nTWFpbkxpbmsnPkNhdGVnb3JpZXM8L2E+PHVsPjxsaT48YSBjbGFzcz0nQ2xlYXJTZXNzaW9uJyBocmVmPSdTZWFyY2guYXNweD9DYXRlZ29yeT0xJz5BY2NlbnQgRnVybml0dXJlPC9hPjx1bD48bGk+PGEgY2xhc3M9J0NsZWFyU2Vzc2lvbicgaHJlZj0nU2VhcmNoLmFzcHg/Q2F0ZWdvcnk9MSZTdWJjYXRlZ29yeT0zMCc+U2lkZSBUYWJsZXM8L2E+PC9saT48bGk+PGEgY2xhc3M9J0NsZWFyU2Vzc2lvbicgaHJlZj0nU2VhcmNoLmFzcHg/Q2F0ZWdvcnk9MSZTdWJjYXRlZ29yeT0xNSc+RGluaW5nIFRhYmxlczwvYT48L2xpPjxsaT48YSBjbGFzcz0nQ2xlYXJTZXNzaW9uJyBocmVmPSdTZWFyY2guYXNweD9DYXRlZ29yeT0xJlN1YmNhdGVnb3J5PTEwJz5Db2ZmZWUgVGFibGVzPC9hPjwvbGk+PGxpPjxhIGNsYXNzPSdDbGVhclNlc3Npb24nIGhyZWY9J1NlYXJjaC5hc3B4P0NhdGVnb3J5PTEmU3ViY2F0ZWdvcnk9MTEnPkNvbnNvbGUgVGFibGVzPC9hPjwvbGk+PGxpPjxhIGNsYXNzPSdDbGVhclNlc3Npb24nIGhyZWY9J1NlYXJjaC5hc3B4P0NhdGVnb3J5PTEnPlZpZXcgQWxsPC9hPjwvbGk+PC91bD48L2xpPjxsaT48YSBjbGFzcz0nQ2xlYXJTZXNzaW9uJyBocmVmPSdTZWFyY2guYXNweD9DYXRlZ29yeT0zJz5EaW5pbmcgYW5kIEVudGVydGFpbmluZzwvYT48dWw+PGxpPjxhIGNsYXNzPSdDbGVhclNlc3Npb24nIGhyZWY9J1NlYXJjaC5hc3B4P0NhdGVnb3J5PTMmU3ViY2F0ZWdvcnk9MjgnPlNlcnZpbmc8L2E+PC9saT48bGk+PGEgY2xhc3M9J0NsZWFyU2Vzc2lvbicgaHJlZj0nU2VhcmNoLmFzcHg/Q2F0ZWdvcnk9MyZTdWJjYXRlZ29yeT0xJz5CYXIgRGVjb3I8L2E+PC9saT48bGk+PGEgY2xhc3M9J0NsZWFyU2Vzc2lvbicgaHJlZj0nU2VhcmNoLmFzcHg/Q2F0ZWdvcnk9MyZTdWJjYXRlZ29yeT0xOCc+R2xhc3N3YXJlPC9hPjwvbGk+PGxpPjxhIGNsYXNzPSdDbGVhclNlc3Npb24nIGhyZWY9J1NlYXJjaC5hc3B4P0NhdGVnb3J5PTMmU3ViY2F0ZWdvcnk9NSc+Q2FuZGxlc3RpY2tzPC9hPjwvbGk+PGxpPjxhIGNsYXNzPSdDbGVhclNlc3Npb24nIGhyZWY9J1NlYXJjaC5hc3B4P0NhdGVnb3J5PTMmU3ViY2F0ZWdvcnk9MjEnPlRyYXlzPC9hPjwvbGk+PGxpPjxhIGNsYXNzPSdDbGVhclNlc3Npb24nIGhyZWY9J1NlYXJjaC5hc3B4P0NhdGVnb3J5PTMmU3ViY2F0ZWdvcnk9MzEnPlNlcnZpbmcgU2V0czwvYT48L2xpPjxsaT48YSBjbGFzcz0nQ2xlYXJTZXNzaW9uJyBocmVmPSdTZWFyY2guYXNweD9DYXRlZ29yeT0zJz5WaWV3IEFsbDwvYT48L2xpPjwvdWw+PC9saT48bGk+PGEgY2xhc3M9J0NsZWFyU2Vzc2lvbicgaHJlZj0nU2VhcmNoLmFzcHg/Q2F0ZWdvcnk9NSc+SG9tZSBBY2NlbnRzPC9hPjx1bD48bGk+PGEgY2xhc3M9J0NsZWFyU2Vzc2lvbicgaHJlZj0nU2VhcmNoLmFzcHg/Q2F0ZWdvcnk9NSZTdWJjYXRlZ29yeT0xMic+RGVjb3JhdGl2ZSBBY2Nlc3NvcmllczwvYT48L2xpPjxsaT48YSBjbGFzcz0nQ2xlYXJTZXNzaW9uJyBocmVmPSdTZWFyY2guYXNweD9DYXRlZ29yeT01JlN1YmNhdGVnb3J5PTIwJz5IdXJyaWNhbmVzIGFuZCBKYXJzPC9hPjwvbGk+PGxpPjxhIGNsYXNzPSdDbGVhclNlc3Npb24nIGhyZWY9J1NlYXJjaC5hc3B4P0NhdGVnb3J5PTUmU3ViY2F0ZWdvcnk9MzknPlZhc2VzPC9hPjwvbGk+PGxpPjxhIGNsYXNzPSdDbGVhclNlc3Npb24nIGhyZWY9J1NlYXJjaC5hc3B4P0NhdGVnb3J5PTUmU3ViY2F0ZWdvcnk9Myc+Qm9va3M8L2E+PC9saT48bGk+PGEgY2xhc3M9J0NsZWFyU2Vzc2lvbicgaHJlZj0nU2VhcmNoLmFzcHg/Q2F0ZWdvcnk9NSZTdWJjYXRlZ29yeT0yNic+UGlsbG93czwvYT48L2xpPjxsaT48YSBjbGFzcz0nQ2xlYXJTZXNzaW9uJyBocmVmPSdTZWFyY2guYXNweD9DYXRlZ29yeT01JlN1YmNhdGVnb3J5PTM4Jz5UaHJvd3MgYW5kIFJ1Z3M8L2E+PC9saT48bGk+PGEgY2xhc3M9J0NsZWFyU2Vzc2lvbicgaHJlZj0nU2VhcmNoLmFzcHg/Q2F0ZWdvcnk9NSZTdWJjYXRlZ29yeT0xNyc+TGFudGVybnM8L2E+PC9saT48bGk+PGEgY2xhc3M9J0NsZWFyU2Vzc2lvbicgaHJlZj0nU2VhcmNoLmFzcHg/Q2F0ZWdvcnk9NSc+VmlldyBBbGw8L2E+PC9saT48L3VsPjwvbGk+PGxpPjxhIGNsYXNzPSdDbGVhclNlc3Npb24nIGhyZWY9J1NlYXJjaC5hc3B4P0NhdGVnb3J5PTcnPk1pcnJvcnMgYW5kIFdhbGwgRGVjb3I8L2E+PHVsPjxsaT48YSBjbGFzcz0nQ2xlYXJTZXNzaW9uJyBocmVmPSdTZWFyY2guYXNweD9DYXRlZ29yeT03JlN1YmNhdGVnb3J5PTI3Jz5SdXN0aWMsIFZpbnRhZ2UgYW5kIEFudGlxdWVkIE1pcnJvcnM8L2E+PC9saT48bGk+PGEgY2xhc3M9J0NsZWFyU2Vzc2lvbicgaHJlZj0nU2VhcmNoLmFzcHg/Q2F0ZWdvcnk9NyZTdWJjYXRlZ29yeT0yMic+TW9kZXJuIE1pcnJvcnM8L2E+PC9saT48bGk+PGEgY2xhc3M9J0NsZWFyU2Vzc2lvbicgaHJlZj0nU2VhcmNoLmFzcHg/Q2F0ZWdvcnk9NyZTdWJjYXRlZ29yeT00MCc+VmVuZXRpYW4gTWlycm9yczwvYT48L2xpPjxsaT48YSBjbGFzcz0nQ2xlYXJTZXNzaW9uJyBocmVmPSdTZWFyY2guYXNweD9DYXRlZ29yeT03JlN1YmNhdGVnb3J5PTEzJz5EZWNvcmF0aXZlIFdhbGwgQXJ0PC9hPjwvbGk+PGxpPjxhIGNsYXNzPSdDbGVhclNlc3Npb24nIGhyZWY9J1NlYXJjaC5hc3B4P0NhdGVnb3J5PTcnPlZpZXcgQWxsPC9hPjwvbGk+PC91bD48L2xpPjxsaT48YSBjbGFzcz0nQ2xlYXJTZXNzaW9uJyBocmVmPSdTZWFyY2guYXNweD9DYXRlZ29yeT05Jz5TZWF0aW5nPC9hPjx1bD48bGk+PGEgY2xhc3M9J0NsZWFyU2Vzc2lvbicgaHJlZj0nU2VhcmNoLmFzcHg/Q2F0ZWdvcnk9OSZTdWJjYXRlZ29yeT03Jz5DaGFpcnM8L2E+PC9saT48bGk+PGEgY2xhc3M9J0NsZWFyU2Vzc2lvbicgaHJlZj0nU2VhcmNoLmFzcHg/Q2F0ZWdvcnk9OSZTdWJjYXRlZ29yeT0zMyc+U3Rvb2xzPC9hPjwvbGk+PGxpPjxhIGNsYXNzPSdDbGVhclNlc3Npb24nIGhyZWY9J1NlYXJjaC5hc3B4P0NhdGVnb3J5PTkmU3ViY2F0ZWdvcnk9MjUnPk90dG9tYW5zIGFuZCBQb3VmczwvYT48L2xpPjxsaT48YSBjbGFzcz0nQ2xlYXJTZXNzaW9uJyBocmVmPSdTZWFyY2guYXNweD9DYXRlZ29yeT05JlN1YmNhdGVnb3J5PTMyJz5Tb2ZhczwvYT48L2xpPjxsaT48YSBjbGFzcz0nQ2xlYXJTZXNzaW9uJyBocmVmPSdTZWFyY2guYXNweD9DYXRlZ29yeT05Jz5WaWV3IEFsbDwvYT48L2xpPjwvdWw+PC9saT48bGk+PGEgY2xhc3M9J0NsZWFyU2Vzc2lvbicgaHJlZj0nU2VhcmNoLmFzcHg/Q2F0ZWdvcnk9Nic+TGlnaHRpbmc8L2E+PHVsPjxsaT48YSBjbGFzcz0nQ2xlYXJTZXNzaW9uJyBocmVmPSdTZWFyY2guYXNweD9DYXRlZ29yeT02JlN1YmNhdGVnb3J5PTM1Jz5UYWJsZTwvYT48L2xpPjxsaT48YSBjbGFzcz0nQ2xlYXJTZXNzaW9uJyBocmVmPSdTZWFyY2guYXNweD9DYXRlZ29yeT02JlN1YmNhdGVnb3J5PTUwJz5DZWlsaW5nIExpZ2h0aW5nPC9hPjwvbGk+PGxpPjxhIGNsYXNzPSdDbGVhclNlc3Npb24nIGhyZWY9J1NlYXJjaC5hc3B4P0NhdGVnb3J5PTYmU3ViY2F0ZWdvcnk9MTYnPkZsb29yIExpZ2h0aW5nPC9hPjwvbGk+PGxpPjxhIGNsYXNzPSdDbGVhclNlc3Npb24nIGhyZWY9J1NlYXJjaC5hc3B4P0NhdGVnb3J5PTYmU3ViY2F0ZWdvcnk9NDEnPldhbGw8L2E+PC9saT48bGk+PGEgY2xhc3M9J0NsZWFyU2Vzc2lvbicgaHJlZj0nU2VhcmNoLmFzcHg/Q2F0ZWdvcnk9Nic+VmlldyBBbGw8L2E+PC9saT48L3VsPjwvbGk+PGxpPjxhIGNsYXNzPSdDbGVhclNlc3Npb24nIGhyZWY9J1NlYXJjaC5hc3B4P0NhdGVnb3J5PTgnPk9mZmljZTwvYT48dWw+PGxpPjxhIGNsYXNzPSdDbGVhclNlc3Npb24nIGhyZWY9J1NlYXJjaC5hc3B4P0NhdGVnb3J5PTgmU3ViY2F0ZWdvcnk9MjMnPk9mZmljZSBBY2Nlc3NvcmllczwvYT48L2xpPjxsaT48YSBjbGFzcz0nQ2xlYXJTZXNzaW9uJyBocmVmPSdTZWFyY2guYXNweD9DYXRlZ29yeT04JlN1YmNhdGVnb3J5PTE0Jz5EZXNrczwvYT48L2xpPjxsaT48YSBjbGFzcz0nQ2xlYXJTZXNzaW9uJyBocmVmPSdTZWFyY2guYXNweD9DYXRlZ29yeT04JlN1YmNhdGVnb3J5PTUxJz5Ucm9waGllczwvYT48L2xpPjxsaT48YSBjbGFzcz0nQ2xlYXJTZXNzaW9uJyBocmVmPSdTZWFyY2guYXNweD9DYXRlZ29yeT04Jz5WaWV3IEFsbDwvYT48L2xpPjwvdWw+PC9saT48bGk+PGEgY2xhc3M9J0NsZWFyU2Vzc2lvbicgaHJlZj0nU2VhcmNoLmFzcHg/Q2F0ZWdvcnk9MTAnPlN0b3JhZ2U8L2E+PHVsPjxsaT48YSBjbGFzcz0nQ2xlYXJTZXNzaW9uJyBocmVmPSdTZWFyY2guYXNweD9DYXRlZ29yeT0xMCZTdWJjYXRlZ29yeT0yJz5CYXNrZXRzIGFuZCBCdWNrZXRzPC9hPjwvbGk+PGxpPjxhIGNsYXNzPSdDbGVhclNlc3Npb24nIGhyZWY9J1NlYXJjaC5hc3B4P0NhdGVnb3J5PTEwJlN1YmNhdGVnb3J5PTI5Jz5TaGVsdmluZzwvYT48L2xpPjxsaT48YSBjbGFzcz0nQ2xlYXJTZXNzaW9uJyBocmVmPSdTZWFyY2guYXNweD9DYXRlZ29yeT0xMCZTdWJjYXRlZ29yeT02Jz5DYXJ0czwvYT48L2xpPjxsaT48YSBjbGFzcz0nQ2xlYXJTZXNzaW9uJyBocmVmPSdTZWFyY2guYXNweD9DYXRlZ29yeT0xMCc+VmlldyBBbGw8L2E+PC9saT48L3VsPjwvbGk+PGxpPjxhIGNsYXNzPSdDbGVhclNlc3Npb24nIGhyZWY9J1NlYXJjaC5hc3B4P0NhdGVnb3J5PTInPkN1c3RvbSBVcGhvbHN0ZXJ5PC9hPjx1bD48bGk+PGEgY2xhc3M9J0NsZWFyU2Vzc2lvbicgaHJlZj0nU2VhcmNoLmFzcHg/Q2F0ZWdvcnk9MiZTdWJjYXRlZ29yeT00Myc+Q3VzdG9tIENoYWlyczwvYT48L2xpPjxsaT48YSBjbGFzcz0nQ2xlYXJTZXNzaW9uJyBocmVmPSdTZWFyY2guYXNweD9DYXRlZ29yeT0yJlN1YmNhdGVnb3J5PTQyJz5DdXN0b20gU29mYXM8L2E+PC9saT48bGk+PGEgY2xhc3M9J0NsZWFyU2Vzc2lvbicgaHJlZj0nU2VhcmNoLmFzcHg/Q2F0ZWdvcnk9MiZTdWJjYXRlZ29yeT00NSc+Q3VzdG9tIE90dG9tYW5zPC9hPjwvbGk+PGxpPjxhIGNsYXNzPSdDbGVhclNlc3Npb24nIGhyZWY9J1NlYXJjaC5hc3B4P0NhdGVnb3J5PTImU3ViY2F0ZWdvcnk9NDYnPkN1c3RvbSBCZW5jaGVzPC9hPjwvbGk+PGxpPjxhIGNsYXNzPSdDbGVhclNlc3Npb24nIGhyZWY9J1NlYXJjaC5hc3B4P0NhdGVnb3J5PTInPlZpZXcgQWxsPC9hPjwvbGk+PC91bD48L2xpPjxsaT48YSBjbGFzcz0nQ2xlYXJTZXNzaW9uJyBocmVmPSdTZWFyY2guYXNweD9DYXRlZ29yeT00Jz5Ib2xpZGF5PC9hPjx1bD48bGk+PGEgY2xhc3M9J0NsZWFyU2Vzc2lvbicgaHJlZj0nU2VhcmNoLmFzcHg/Q2F0ZWdvcnk9NCZTdWJjYXRlZ29yeT0xOSc+SG9saWRheSBEZWNvcjwvYT48L2xpPjxsaT48YSBjbGFzcz0nQ2xlYXJTZXNzaW9uJyBocmVmPSdTZWFyY2guYXNweD9DYXRlZ29yeT00JlN1YmNhdGVnb3J5PTI0Jz5Pcm5hbWVudHM8L2E+PC9saT48bGk+PGEgY2xhc3M9J0NsZWFyU2Vzc2lvbicgaHJlZj0nU2VhcmNoLmFzcHg/Q2F0ZWdvcnk9NCc+VmlldyBBbGw8L2E+PC9saT48L3VsPjwvbGk+PGxpPjxhIGNsYXNzPSdDbGVhclNlc3Npb24nIGhyZWY9J1NlYXJjaC5hc3B4Jz5WaWV3IEFsbDwvYT48L2xpPjwvdWw+PC9saT4NCiAgICAgICAgICAgIDxsaT48YSBocmVmPSdTaG93cm9vbXMuYXNweCcgY2xhc3M9J01haW5MaW5rJz5GaW5kIFVzPC9hPg0KICAgICAgICAgICAgICAgIDx1bD4NCiAgICAgICAgICAgICAgICAgICAgPGxpPjxhIGhyZWY9J0NhbGVuZGFyLmFzcHgnPk1hcmtldCBEYXRlczwvYT48L2xpPg0KICAgICAgICAgICAgICAgICAgICA8bGk+PGEgaHJlZj0nU2hvd3Jvb21zLmFzcHgnPlNob3dyb29tczwvYT48L2xpPg0KICAgICAgICAgICAgICAgICAgICA8bGk+PGEgaHJlZj0nU2hvd3Jvb21zLmFzcHgnPlNhbGVzIEFzc29jaWF0ZXM8L2E+PC9saT4NCiAgICAgICAgICAgICAgICAgICAgPGxpPjxhIGhyZWY9J1ByZXNzLmFzcHgnPkluIFRoZSBQcmVzczwvYT48L2xpPg0KICAgICAgICAgICAgICAgIDwvdWw+DQogICAgICAgICAgICA8L2xpPg0KICAgICAgICAgICAgPGxpPjxhIGhyZWY9J1NlYXJjaC5hc3B4P1NhbGU9MScgY2xhc3M9J01haW5MaW5rJz5EZWFsczwvYT4NCiAgICAgICAgICAgICAgICA8dWw+DQogICAgICAgICAgICAgICAgICAgIDxsaT48YSBocmVmPSdTZWFyY2guYXNweD9TYWxlPTEnPk9uIFNhbGUgTm93PC9hPjwvbGk+DQogICAgICAgICAgICAgICAgICAgIDxsaT48YSBocmVmPSdTZWFyY2guYXNweD9OZXc9MSc+TmV3IFN0dWZmPC9hPjwvbGk+DQogICAgICAgICAgICAgICAgICAgIDxsaT48YSBocmVmPSdMb3lhbHR5LmFzcHgnPkxveWFsdHkgUHJvZ3JhbTwvYT48L2xpPg0KICAgICAgICAgICAgICAgICAgICA8bGk+PGEgaHJlZj0nRnJlaWdodC5hc3B4Jz5GcmVpZ2h0IFByb2dyYW08L2E+PC9saT4NCiAgICAgICAgICAgICAgICA8L3VsPg0KICAgICAgICAgICAgPC9saT4NCiAgICAgICAgICAgIDxsaT48YSBocmVmPSdDb250YWN0LmFzcHgnIGNsYXNzPSdNYWluTGluayc+Q29udGFjdDwvYT4NCiAgICAgICAgICAgICAgICA8dWw+DQogICAgICAgICAgICAgICAgICAgIDxsaT48YSBocmVmPSdDb250YWN0LmFzcHgnPk91ciBUZWFtPC9hPjwvbGk+DQogICAgICAgICAgICAgICAgICAgIDxsaT48YSBocmVmPSdBYm91dC5hc3B4Jz5BYm91dCBVczwvYT48L2xpPg0KICAgICAgICAgICAgICAgICAgICA8bGk+PGEgaHJlZj0iSG9zcGl0YWxpdHkuYXNweCI+R08gSG9zcGl0YWxpdHk8L2E+PC9saT4NCiAgICAgICAgICAgICAgICA8L3VsPg0KICAgICAgICAgICAgPC9saT4NCiAgICAgICAgICAgIDxsaT48YSBocmVmPSdodHRwOi8vd3d3LmdvaG9tZWx0ZC5jb20vQmxvZycgY2xhc3M9J01haW5MaW5rJz5CbG9nPC9hPjwvbGk+DQogICAgICAgICAgICA8L3VsPg0KICAgICAgICBkAgUPDxYCHgdWaXNpYmxlZ2RkAgcPZBYCAgEPZBYCZg9kFgYCAQ8PFgIeBFRleHRlZGQCBQ8WAh8BaGQCBw9kFgICAw8PFgIfAmVkZAIJDxYCHwFoZAILD2QWCAIBDw8WAh4ISW1hZ2VVcmwFG34vSW1hZ2VzL1ZpZ25ldHRlcy9HT0g2LmpwZ2RkAgMPDxYCHwMFG34vSW1hZ2VzL1ZpZ25ldHRlcy9HT0g3LmpwZ2RkAgUPDxYCHwMFG34vSW1hZ2VzL1ZpZ25ldHRlcy9HT0g4LmpwZ2RkAgcPDxYCHwMFG34vSW1hZ2VzL1ZpZ25ldHRlcy9HT0g5LmpwZ2RkZCYVq1P1cjIgmZOYIlbouK1qcW8l5o5vVVT7jt6d2tjN");
            nvCol.Add("__VIEWSTATEGENERATOR", "C0093815");
            nvCol.Add("__EVENTVALIDATION", "/wEdAAW5u1D+g3uj9U3BvAe1WUV0iMG8PCotn5kvtSgTiaTI3sFlxQ/SVbz7xZQYGOSOrfcvz0gL8cmyBu9Bou5TEqNhSRZVUtO1aEx/UVNm/L2iDQUFU+UAHP2JSwa2yJow4ebaDNFmR6Oz6o9KzmwctYTg");
            nvCol.Add("ctl00$txtUsername", vendor.Username);
            nvCol.Add("ctl00$txtPassword", vendor.Password);
            nvCol.Add("ctl00$btnLogin", "Sign In");

            var postPage = await webClient.DownloadPageAsync(vendor.LoginUrl, nvCol);
            var result = await webClient.DownloadPageAsync("http://www.gohomeltd.com/Store/Home.aspx");
            if (result.InnerText.ContainsIgnoreCase("Tessa"))
                return new AuthenticationResult(true, webClient.Cookies.GetCookies(new Uri(vendor.LoginUrl)));
            return new AuthenticationResult(false);
        }
    }
}