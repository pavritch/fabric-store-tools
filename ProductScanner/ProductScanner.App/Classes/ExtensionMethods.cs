using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Media.Effects;
using Expression = System.Linq.Expressions.Expression;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Animation;
using System.Dynamic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace ProductScanner.App
{

    /// <summary>
    /// Generic extension methods used by the framework.
    /// </summary>
    public static class ExtensionMethods
    {

        public static int ToSeed(this string text)
        {
            UnicodeEncoding UE = new UnicodeEncoding();
            byte[] hashValue;
            byte[] message = UE.GetBytes(text);

            var hashString = new SHA1Managed();
            int result = 0;

            hashValue = hashString.ComputeHash(message);
            foreach (byte x in hashValue)
                result += x;

            return result;
        }


        /// <summary>
        /// Gets all the attributes of a particular type.
        /// </summary>
        /// <typeparam name="T">The type of attributes to get.</typeparam>
        /// <param name="member">The member to inspect for attributes.</param>
        /// <param name="inherit">Whether or not to search for inherited attributes.</param>
        /// <returns>The list of attributes found.</returns>
        public static IEnumerable<T> GetAttributes<T>(this MemberInfo member, bool inherit) {
            return Attribute.GetCustomAttributes(member, inherit).OfType<T>();
        }

        /// <summary>
        /// Gets a property by name, ignoring case and searching all interfaces.
        /// </summary>
        /// <param name="type">The type to inspect.</param>
        /// <param name="propertyName">The property to search for.</param>
        /// <returns>The property or null if not found.</returns>
        public static PropertyInfo GetPropertyCaseInsensitive(this Type type, string propertyName) {
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
        public static void Apply<T>(this IEnumerable<T> enumerable, Action<T> action) {
            foreach(var item in enumerable)
                action(item);
        }

        /// <summary>
        /// Converts an expression into a <see cref="MemberInfo"/>.
        /// </summary>
        /// <param name="expression">The expression to convert.</param>
        /// <returns>The member info.</returns>
        public static MemberInfo GetMemberInfo(this Expression expression) {
            var lambda = (LambdaExpression)expression;

            MemberExpression memberExpression;
            if(lambda.Body is UnaryExpression) {
                var unaryExpression = (UnaryExpression)lambda.Body;
                memberExpression = (MemberExpression)unaryExpression.Operand;
            }
            else
                memberExpression = (MemberExpression)lambda.Body;

            return memberExpression.Member;
        }


        /// <summary>
        /// Convert a (A)RGB string to a Color object.
        /// </summary>
        /// <param name="s">An RGB or an ARGB string.</param>
        /// <returns>A Color object.</returns>
        public static Color ToColor(this string s)
        {
            s = s.Replace("#", string.Empty);

            byte a = System.Convert.ToByte("ff", 16);

            byte pos = 0;

            if (s.Length == 8)
            {
                a = System.Convert.ToByte(s.Substring(pos, 2), 16);
                pos = 2;
            }

            byte r = System.Convert.ToByte(s.Substring(pos, 2), 16);

            pos += 2;

            byte g = System.Convert.ToByte(s.Substring(pos, 2), 16);

            pos += 2;

            byte b = System.Convert.ToByte(s.Substring(pos, 2), 16);

            return Color.FromArgb(a, r, g, b);
        }

        public static SolidColorBrush ToSolidBrush(this string rgb)
        {
            return new SolidColorBrush(rgb.ToColor());
        }

        /// <summary>
        /// Create a dynamic list of name/value pairs from input.
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static List<dynamic> ToDynamicNameValueList(this Dictionary<string, string> dic)
        {
            var results = new List<dynamic>();
            foreach (var item in dic)
            {
                var nameValue = new ExpandoObject();
                var nv = nameValue as IDictionary<String, object>;
                nv["Name"] = item.Key;
                nv["Value"] = item.Value;
                results.Add(nameValue);
            }
            return results;
        }

        public static System.Windows.Controls.Image ToImageControl(this string input, bool useDefaultImagePath)
        {
            var imgsrc = input.ToImageSource(useDefaultImagePath);
            return new System.Windows.Controls.Image() { Source = imgsrc };
        }

        public static ImageSource ToImageSource(this string input, bool useDefaultImagePath)
        {
                var isc = new System.Windows.Media.ImageSourceConverter();
                string url;
                if (useDefaultImagePath)
                    url = string.Format("pack://application:,,,/ProductScanner.App;component/Assets/Images/{0}", input);
                else
                    url = input;

                var imgsrc = isc.ConvertFromString(url) as System.Windows.Media.ImageSource;
                return imgsrc;
        }

        public static bool TryOpenUrl(this string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            // http://stackoverflow.com/questions/502199/how-to-open-a-web-page-from-my-application

            // try use default browser [registry: HKEY_CURRENT_USER\Software\Classes\http\shell\open\command]

            //try
            //{
            //    string keyValue = Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\Classes\http\shell\open\command", "", null) as string;
            //    if (string.IsNullOrEmpty(keyValue) == false)
            //    {
            //        string browserPath = keyValue.Replace("%1", url);
            //        System.Diagnostics.Process.Start(browserPath);
            //        return true;
            //    }
            //}
            //catch { }

            // try open browser as default command
            try
            {
                System.Diagnostics.Process.Start(url); //browserPath, argUrl);
                return true;
            }
            catch { }

            // return false, all failed
            return false;
        }


        public static string DescriptionAttr<T>(this T source)
        {
            FieldInfo fi = source.GetType().GetField(source.ToString());

            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0) return attributes[0].Description;
            else return source.ToString();
        }

        /// <summary>
        /// Given the input string, return split up into lines.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static List<string> ConvertToListOfLines(this string input)
        {
            var list = new List<string>();

            if (string.IsNullOrWhiteSpace(input))
                return list;

            StringReader strReader = new StringReader(input);
            while (true)
            {
                var line = strReader.ReadLine();
                if (line == null)
                    break;

                if (!string.IsNullOrWhiteSpace(line))
                    list.Add(line);
            }

            return list;
        }

        public static string ToFileSize(this long size)
        {
            if (size < 1024)
            {
                return (size).ToString("F0") + " bytes";
            }
            else if (size < Math.Pow(1024, 2))
            {
                return (size / 1024).ToString("F0") + " KB";
            }
            else if (size < Math.Pow(1024, 3))
            {
                return (size / Math.Pow(1024, 2)).ToString("F0") + " MB";
            }
            else if (size < Math.Pow(1024, 4))
            {
                return (size / Math.Pow(1024, 3)).ToString("F0") + " GB";
            }
            else if (size < Math.Pow(1024, 5))
            {
                return (size / Math.Pow(1024, 4)).ToString("F0") + " TB";
            }
            else if (size < Math.Pow(1024, 6))
            {
                return (size / Math.Pow(1024, 5)).ToString("F0") + " PB";
            }
            else
            {
                return (size / Math.Pow(1024, 6)).ToString("F0") + " EB";
            }
        }

    }
}