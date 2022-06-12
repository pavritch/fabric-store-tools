using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Data.Linq;
using Website.Entities;
using System.Diagnostics;
using Gen4.Util.Misc;
using System.Text.RegularExpressions;

namespace Website
{
    public static class StringExtensions
    {

        public static List<int> ParseIntList(this string Parameter)
        {
            List<int> list = new List<int>();

            if (string.IsNullOrWhiteSpace(Parameter))
                return list;

            var s = Parameter.Trim();

            var ary = s.Replace(" ", "").Split(',');
            foreach (var x in ary)
            {
                int value;
                if (int.TryParse(x, out value))
                {
                    list.Add(value);
                }
            }

            return list;
        }

        public static string ReplacePrefix(this string input, string s1, string s2)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            if (!input.StartsWith(s1))
                return input;

            var s3 = s2 + input.Substring(s1.Length);

            return s3;
        }

        public static string ReplaceSuffix(this string input, string s1, string s2)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            if (!input.EndsWith(s1))
                return input;

            var s3 =  input.Left(input.LastIndexOf(s1));
            s3 += s2;

            return s3;
        }

        public static string ReplaceColorAbreviations(this string input)
        {
            var replacements = new Dictionary<string, string>()
                {
                    {"nat", "natural"},
                    {"gr", "green"},
                    {"wht", "white"},
                    {"go", "gold"},
                    {"pnk", "pink"},
                    {"br", "brown"},
                    {"blk", "black"},
                    {"or", "orange"},
                    {"yel", "yellow"},
                    {"pu", "purple"},
                    {"nu", "neutral"},
                    {"jonc", "jonquil"},
                    {"ic", "ice"},
                    {"bla", "black"},
                    {"cff", "coffee"},
                    {"cffee", "coffee"},
                    {"bge", "beige"},
                    {"bis", "biscotti"},
                    {"bisct", "biscotti"},
                    {"multi", "multicolor"},
                    {"brn", "brown"},
                    {"drk", "dark"},
                    {"gld", "gold"},
                    {"lght", "light"},
                    {"lt", "light"},
                    {"lt.", "light"},
                    {"mulberr", "mulberry"},
                    {"mul", "mulberry"},
                    {"natrl", "natural"},
                    {"mult", "multicolor"},
                    {"whi", "white"},
                    {"gol", "gold"},
                    {"ros", "rose"},
                    {"gre", "green"},
                    {"olive g", "olive green"},
                    {"ore", "oregano"},
                    {"ori", "oriental"},
                    {"pch", "peach"},
                    {"pur", "purple"},
                    {"rasber", "rasberry"},
                    {"truffl", "truffle"},
                    {"slv", "silver"},
                    {"silv", "silver"},
                    {"slvr", "silver"},
                    {"sla", "slate"},
                    {"pi", "pink"},
                    {"sunglo", "sunglow"},
                    {"smoky", "smokey"},
                    {"thym", "thyme"},
                    {"turq", "turquoise"},
                    {"turqois", "turquoise"},
                    {"turqoise", "turquoise"},
                    {"win", "wine"},
                    {"gry", "grey"},
                    {"gyspy", "gypsy"},
                    {"bleu", "blue"},
                    {"grn", "green"},
                    {"fiz", "fizz"},
                    {"dk", "dark"},
                    {"bl", "blue"},
                    {"crnbry", "cranberry"},
                    {"cornflo", "cornflower"},
                    {"fennelchocolate", "fennel chocolate"},
                    {"cognc", "cognac"},
                    {"chr", "chrome"},
                    {"charl", "charleston"},
                    {"carrott", "carrot"},
                    {"carib", "caribean"},
                    {"blu", "blue"},
                    {"blueber", "blueberry"},
                    {"brk", "bark"},
                    {"boysen", "boysenberry"},
                    {"burl", "burlap"},
                    {"fresa", "fresca"},
                };

            // if direct match on one of the phrases, it's a simple full replacement
            string replacement;
            if (replacements.TryGetValue(input, out replacement))
                return replacement;

            // see if starts with or ends with
            foreach (var item in replacements)
                input = input.ReplacePrefix(item.Key + " ", item.Value + " ").ReplaceSuffix(" " + item.Key, " " + item.Value).ReplaceSuffix(" on", string.Empty).ReplaceSuffix(" and", string.Empty);

            return input;
        }


        public static string MakeSafeSEName(this string s)
        {
            var s1 = s.Replace("   ", " ")
                .Replace("  ", " ")
                .Replace(" ", "-")
                .Replace("/", "-")
                .Replace("_", "-")
                .Replace(".", "-")
                .Replace("è", "e")
                .Replace("é", "e")
                .Replace("&", "and")
                .Replace("--", "-").Replace("--", "-").Replace("--", "-"); // three times

            var sb = new StringBuilder(200);
            foreach (var c in s1.ToLower())
                if (IsSafeSEFilenameChar(c))
                    sb.Append(c);

            return sb.ToString();
        }

        private static bool IsSafeSEFilenameChar(char c)
        {
            if (c >= 'a' && c <= 'z')
                return true;

            if (c >= 'A' && c <= 'Z')
                return true;

            if (c >= '0' && c <= '9')
                return true;

            if ("-".Contains(c))
                return true;

            return false;
        }

        public static string RemvoveCFA(this string input)
        {
            if (!input.Contains("cfa"))
                return input;

            // the order of this list is important, longer phrases should be in front
            var ignoreWords = new List<string> { "cfa b4 ship", "cfa bef ship", "cfa bef shippin", "cfa before ship", "cfa befre shipp", "for approva", "for approval", "cfa only", "cfa req'd", "cfa onl", "cfa", "before", "shipping", "only" };

            foreach (var ignoreWord in ignoreWords)
                input = input.Replace(ignoreWord, string.Empty);

            // do replace twice on space to be extra sure we got everything
            input = input.Replace("  ", " ").Replace("  ", " ").Trim();
            return input;
        }


        public static List<string> ParseSearchTokens(this string input)
        {
            var set = new HashSet<string>();

            var ary = input.ReplacePrefix("a ", string.Empty).ReplacePrefix("and ", string.Empty).Split(new char[] { ',', ';', '/', '\\', '-' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var item in ary)
            {
                var s1 = item.ToLower().Trim();
                if (string.IsNullOrWhiteSpace(s1))
                    continue;

                set.Add(s1);

                // now divide up into the individual words and output them as well, important for autosuggest phrases

                var s2 = s1.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                s2.ForEach(e => set.Add(e));
            }

            return set.ToList();
        }

        public static string ToInitialCaps(this string input)
        {
            char[] array = input.ToLower().ToCharArray();

            // Handle the first letter in the string.
            if (array.Length >= 1)
            {
                array[0] = char.ToUpper(array[0]);
            }
            // Scan through the letters, checking for spaces.
            // ... Uppercase the lowercase letters following spaces.
            for (int i = 1; i < array.Length; i++)
            {
                if (array[i - 1] == ' ')
                {
                    array[i] = char.ToUpper(array[i]);
                }
            }
            return new string(array);
        }
    }

    public static class ExtensionMethods123
    {
        public static string RemovePattern(this string input, string pattern)
        {
            var s = Regex.Replace(input, pattern, System.String.Empty, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            return s;
        }

        public static string ReplacePattern(this string input, string pattern, string fmtReplacement)
        {
            // relace div tags with p tags
            // @"\<div>(?<found>(CAPTURE-THIS)</div>"
            // @"<p>{0}</p>"

            // the captured pattern from the match expression will be formatted using the fmtReplacement
            // and put back into the original string.

            return Regex.Replace(input, pattern, (match) =>
                {
                    var capturedText = match.Groups[1].ToString();

                    var mergedValue = string.Format(fmtReplacement, capturedText);

                    return mergedValue ?? string.Empty;

                }, RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);


        }
    }
}