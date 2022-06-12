using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StoreCurator
{
    public static class ExtensionMethods
    {

        private const StringComparison STRING_COMPARISON_RULE = StringComparison.InvariantCultureIgnoreCase;


        /// <summary>
        /// Extension method to determine if whether the string contains the specified value in a case-insensitive manner
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool ContainsIgnoreCase(this string str, string value)
        {
            return str.IndexOf(value, STRING_COMPARISON_RULE) != -1;
        }

        public static int ToInteger(this bool value)
        {
            return value == true ? 1 : 0;
        }
    }
}