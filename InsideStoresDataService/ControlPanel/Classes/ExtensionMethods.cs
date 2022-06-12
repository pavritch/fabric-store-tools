namespace ControlPanel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Expression = System.Linq.Expressions.Expression;
    using System.Windows.Media;

    /// <summary>
    /// Generic extension methods used by the framework.
    /// </summary>
    public static class ExtensionMethods
    {
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


    }
}