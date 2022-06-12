using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.Server;

namespace SqlClrExtensions
{
    // https://www.codeproject.com/Articles/42764/Regular-Expressions-in-MS-SQL-Server

    // NOTE: .Net 3.5 ONLY!

    public static class MyFunctions
    {
        [SqlFunction]
        public static string RegExReplace(string input, string pattern, string replacement)
        {
            Regex rgx = new Regex(pattern, RegexOptions.Singleline);
            string result = rgx.Replace(input, replacement);
            return result;
        }

    }
}
