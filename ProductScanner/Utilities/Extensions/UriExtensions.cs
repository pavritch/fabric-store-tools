using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Utilities.Extensions
{
    public static class UriExtensions
    {
        private static readonly Regex _regex = new Regex(@"[?|&]([\w\.]+)=([^?|^&]+)");

        public static IReadOnlyDictionary<string, string> ParseQueryString(this Uri uri)
        {
            var match = _regex.Match(uri.PathAndQuery);
            var parameters = new Dictionary<string, string>();
            while (match.Success)
            {
                parameters.Add(match.Groups[1].Value, match.Groups[2].Value);
                match = match.NextMatch();
            }
            return parameters;
        }

        public static string GetQueryParameter(this Uri uri, string key)
        {
            var queryParams = uri.ParseQueryString();
            return queryParams[key];
        }

        public static string GetDocumentName(this Uri uri)
        {
            return System.IO.Path.GetFileName(uri.LocalPath);
        }
    }
}