using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Expression = System.Linq.Expressions.Expression;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Web;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace DataFeedsWebsite
{

    /// <summary>
    /// Generic extension methods used by the framework.
    /// </summary>
    public static class ExtensionMethods
    {



        public static string UrlEncode(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;

            return HttpUtility.UrlEncode(s);
        }



        public static string HtmlEncode(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;

            return HttpUtility.HtmlEncode(s);
        }

        /// <summary>
        /// Compresses a byte array using gzip compression.
        /// </summary>
        /// <param name="bytes">Byte array to compress.</param>
        /// <returns>A compressed byte array.</returns>
        public static byte[] Compress(this byte[] bytes)
        {
            MemoryStream ms = new MemoryStream();
            GZipStream zip = new GZipStream(ms, CompressionMode.Compress, true);
            zip.Write(bytes, 0, bytes.Length);
            zip.Close();
            ms.Position = 0;

            MemoryStream outStream = new MemoryStream();

            byte[] compressed = new byte[ms.Length];
            ms.Read(compressed, 0, compressed.Length);

            byte[] gzipBuffer = new byte[compressed.Length + 4];
            Buffer.BlockCopy(compressed, 0, gzipBuffer, 4, compressed.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(bytes.Length), 0, gzipBuffer, 0, 4);

            return gzipBuffer;
        }

        /// <summary>
        /// Decompresses a byte array using gzip compression.
        /// </summary>
        /// <param name="bytes">Byte array to decompress.</param>
        /// <returns>A decompressed byte array.</returns>
        public static byte[] Decompress(this byte[] bytes)
        {
            MemoryStream ms = new MemoryStream();
            int msgLength = BitConverter.ToInt32(bytes, 0);
            ms.Write(bytes, 4, bytes.Length - 4);

            byte[] buffer = new byte[msgLength];

            ms.Position = 0;
            GZipStream zip = new GZipStream(ms, CompressionMode.Decompress);
            zip.Read(buffer, 0, buffer.Length);

            return buffer;
        }


        public static byte[] ToByteArray(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return new byte[0];

            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            return encoding.GetBytes(str);
        }

        /// <summary>
        /// Gets the parameter token from the passed url fragment.
        /// </summary>
        /// <remarks>
        /// Assumes entry string starts with the parameter. Take up until "&" or end of string.
        /// </remarks>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ExtractNextUrlParameter(this string s)
        {
            var sb = new StringBuilder(100);

            foreach(var ch in s)
            {
                if (ch == '&')
                    break;

                sb.Append(ch);
            }

            return sb.ToString();
        }

        public static bool ContainsIgnoreCase(this string s1, string s2 )
        {
            if (string.IsNullOrEmpty(s1))
                return false;

            return (s1.IndexOf(s2, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        public static string TakeOnlyNumericPart(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;

            var sb = new StringBuilder();

            foreach (var ch in s)
            {
                if (char.IsDigit(ch) || ch == '.')
                    sb.Append(ch);
                else
                    break;
            }
            return sb.ToString();
        }

        /// <summary>
        /// Gets all the attributes of a particular type.
        /// </summary>
        /// <typeparam name="T">The type of attributes to get.</typeparam>
        /// <param name="member">The member to inspect for attributes.</param>
        /// <param name="inherit">Whether or not to search for inherited attributes.</param>
        /// <returns>The list of attributes found.</returns>
        public static IEnumerable<T> GetAttributes<T>(this MemberInfo member, bool inherit)
        {
            return Attribute.GetCustomAttributes(member, inherit).OfType<T>();
        }

        /// <summary>
        /// Gets a property by name, ignoring case and searching all interfaces.
        /// </summary>
        /// <param name="type">The type to inspect.</param>
        /// <param name="propertyName">The property to search for.</param>
        /// <returns>The property or null if not found.</returns>
        public static PropertyInfo GetPropertyCaseInsensitive(this Type type, string propertyName)
        {
            var typeList = new List<Type> { type };

            if (type.IsInterface)
                typeList.AddRange(type.GetInterfaces());

            var flags = BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance;

            return typeList
                .Select(interfaceType => interfaceType.GetProperty(propertyName, flags))
                .FirstOrDefault(property => property != null);
        }

        /// <summary>
        /// Applies the action to each element in the list.
        /// </summary>
        /// <typeparam name="T">The enumerable item's type.</typeparam>
        /// <param name="enumerable">The elements to enumerate.</param>
        /// <param name="action">The action to apply to each item in the list.</param>
        public static void Apply<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
                action(item);
        }

        /// <summary>
        /// Converts an expression into a <see cref="MemberInfo"/>.
        /// </summary>
        /// <param name="expression">The expression to convert.</param>
        /// <returns>The member info.</returns>
        public static MemberInfo GetMemberInfo(this Expression expression)
        {
            var lambda = (LambdaExpression)expression;

            MemberExpression memberExpression;
            if (lambda.Body is UnaryExpression)
            {
                var unaryExpression = (UnaryExpression)lambda.Body;
                memberExpression = (MemberExpression)unaryExpression.Operand;
            }
            else
                memberExpression = (MemberExpression)lambda.Body;

            return memberExpression.Member;
        }


        public static string RemoveInitialDash(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return s;

            if (s.StartsWith("-"))
                s = s.Remove(0, 1);

            return s;
        }


    }
}