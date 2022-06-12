using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace ProductScanner.Data
{
    public static class ExtensionMethods
    {
        public static string HtmlEncode(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;

            return HttpUtility.HtmlEncode(s);
        }

        public static string HtmlDecode(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;

            return HttpUtility.HtmlDecode(s);
        }

        public static string ToCommaDelimitedList(this IEnumerable<string> values)
        {
            if (values == null)
                return string.Empty;

            var sb = new StringBuilder(500);
            int count = 0;
            foreach (var s in values)
            {
                if (string.IsNullOrWhiteSpace(s))
                    continue;

                if (count > 0)
                    sb.Append(", ");

                sb.Append(s);
                count++;
            }

            return sb.ToString();
        }

        public static string RomanNumeralCase(this string s)
        {
            const string RomanNumerals = @"^((?=[MDCLXVI])((M{0,3})((C[DM])|(D?" +
                @"C{0,3}))?((X[LC])|(L?XX{0,2})|L)?((I[VX])|(V?(II{0,2}))|V)?)),?$";

            if (string.IsNullOrWhiteSpace(s))
                return s;

            var sTrim = s.Trim();

            var tokens = sTrim.Split(' ');

            if (tokens.Length == 0)
                return string.Empty;

            StringBuilder sb = new StringBuilder(s.Length);
            foreach (string token in tokens)
            {
                var trimToken = token.Trim();
                if (string.IsNullOrWhiteSpace(trimToken))
                    continue;

                if (Regex.IsMatch(trimToken, RomanNumerals, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                {
                    sb.Append(trimToken.ToUpper());
                }
                else
                    sb.Append(trimToken);

                sb.Append(" ");
            }

            var finalText = sb.ToString().Trim();

            return finalText;
        }



        public static string TitleCase(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return s;

            var sTrim = s.Trim();

            var tokens = sTrim.Split(' ');

            if (tokens.Length == 0)
                return string.Empty;

            StringBuilder sb = new StringBuilder(s.Length);

            foreach (string token in tokens)
            {
                var trimToken = token.Trim();
                if (string.IsNullOrWhiteSpace(trimToken))
                    continue;

                sb.Append(trimToken[0].ToString().ToUpper());

                if (trimToken.Length > 1)
                {
                    var sb2 = new StringBuilder(100);
                    bool skipLower = false;
                    foreach (var c in trimToken.Substring(1))
                    {
                        if (skipLower)
                        {
                            sb2.Append(c.ToString());
                            skipLower = false;
                        }
                        else
                            sb2.Append(c.ToString().ToLower());

                        if (!Char.IsLetter(c) && c != '\'')
                            skipLower = true;
                    }


                    sb.Append(sb2.ToString());
                }

                sb.Append(" ");
            }

            return sb.ToString().Trim();
        }



    }
}