using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Utilities.Extensions
{
    public static class ReflectionExtensions
    {
        // Using C# 3.0 For Reflection, The Wandering Glitch 2
        // http://aabs.wordpress.com/2006/11/09/using-c-30-for-reflection/

        /// <summary>
        /// Returns true if type has this attribute
        /// </summary>
        /// <param name="t"></param>
        /// <param name="attrType"></param>
        /// <returns></returns>
        public static bool HasAttribute(this Type t, Type attrType)
        {
            return t.GetCustomAttributes(attrType, true).Count() > 0;
        }

        public static bool HasAttribute<T>(this Type t, Func<T, bool> predicate)
        {
            foreach (T type in t.GetCustomAttributes(typeof(T), true))
            {
                if (predicate(type))
                    return true;
            }
            return false;
        }


        /// <summary>
        /// Returns true if the item has the specified attribute.
        /// </summary>
        /// <param name="mi"></param>
        /// <param name="attrType"></param>
        /// <returns></returns>
        public static bool HasAttribute(this MemberInfo mi, Type attrType)
        {
            return mi.GetCustomAttributes(attrType, true).Count() > 0;
        }

        /// <summary>
        /// Method with attribute and filter by predicate on value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mi"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static bool HasAttribute<T>(this MemberInfo mi, Func<T, bool> predicate)
        {
            foreach (T attrObject in mi.GetCustomAttributes(typeof(T), true))
            {
                if (predicate(attrObject))
                    return true;
            }
            return false;
        }



        /// <summary>
        /// Return enumeration of all members for a type.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static IEnumerable<MemberInfo> Members(this Type t)
        {
            foreach (FieldInfo fi in t.GetFields())
                yield return fi;
            foreach (PropertyInfo pi in t.GetProperties())
                yield return pi;
            foreach (MethodInfo mi in t.GetMethods())
                yield return mi;
            foreach (EventInfo ei in t.GetEvents())
                yield return ei;
        }


        /// <summary>
        /// Return enumeration of all members fields and properties.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static IEnumerable<MemberInfo> MemberFieldsAndProperties(this Type t)
        {
            foreach (FieldInfo fi in t.GetFields())
                yield return fi;
            foreach (PropertyInfo pi in t.GetProperties())
                yield return pi;
        }

        /// <summary>
        /// Return enumeration of all members fields.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static IEnumerable<MemberInfo> MemberFields(this Type t)
        {
            foreach (FieldInfo fi in t.GetFields())
                yield return fi;
        }

        /// <summary>
        /// Return enumeration of all members fields and properties.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static IEnumerable<MemberInfo> MemberProperties(this Type t)
        {
            foreach (PropertyInfo pi in t.GetProperties())
                yield return pi;
        }

        /// <summary>
        /// Given assy and type of attribute, returns a list of all types within having that attribute
        /// </summary>
        /// <param name="asm"></param>
        /// <param name="attrType"></param>
        /// <returns></returns>
        public static IEnumerable<Type> TypesWithAttributes(this Assembly asm, Type attrType)
        {
            return from t in asm.GetTypes()
                   where t.HasAttribute(attrType)
                   select t;
        }



        /// <summary>
        /// For the given MemberInfo, return enumeration of attributes matching the specified type,
        /// which accounts for the item having multiple similarly-named attributes.
        /// </summary>
        /// <typeparam name="AttrType"></typeparam>
        /// <param name="mi"></param>
        /// <returns></returns>
        public static IEnumerable<AttrType> Attributes<AttrType>(this MemberInfo mi)
               where AttrType : Attribute
        {
            foreach (AttrType ra in mi.GetCustomAttributes(typeof(AttrType), true)
                          .Where(t => t.GetType().Equals(typeof(AttrType))))
            {
                yield return ra as AttrType;
            }
        }

        /// <summary>
        /// Given a list (enumeration) of MemberInfo, return a list of all the matching attribute
        /// objects amongst the set of them. 
        /// </summary>
        /// <typeparam name="AttrType"></typeparam>
        /// <param name="emi"></param>
        /// <returns></returns>
        public static IEnumerable<AttrType> Attributes<AttrType>(this IEnumerable<MemberInfo> emi)
                where AttrType : Attribute
        {
            foreach (MemberInfo mi in emi)
            {
                foreach (AttrType ra in mi.GetCustomAttributes(typeof(AttrType), true)
                        .Where(t => t.GetType().Equals(typeof(AttrType))))
                {
                    yield return ra as AttrType;
                }
            }
        }

        /// <summary>
        /// Find and return all attributes of a certain type belonging to the given member.
        /// </summary>
        /// <typeparam name="AttrType"></typeparam>
        /// <param name="mi"></param>
        /// <returns></returns>
        private static IEnumerable<AttrType> GetAllAttributes<AttrType>(MemberInfo mi) where AttrType : Attribute
        {
            foreach (AttrType ra in mi.GetCustomAttributes(typeof(AttrType), true)
                                   .Where(t => t.GetType().Equals(typeof(AttrType))))
            {
                yield return ra as AttrType;
            }
        }

        /// <summary>
        /// Does this enum have the specified attribute.
        /// </summary>
        /// <param name="e"></param>
        public static bool HasAttribute(this Enum e, Type attrType )
        {

            FieldInfo fi = e.GetType().GetField(e.ToString());
            return fi.HasAttribute(attrType);

            //var memInfo = typeof(Enum).GetMember(e.ToString());
            //return memInfo[0].HasAttribute(attrType);
        }


        /// <summary>
        /// Returns Description attribute of an Enum
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string DescriptionAttr<T>(this T source)
        {
            FieldInfo fi = source.GetType().GetField(source.ToString());

            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0) return attributes[0].Description;
            else return source.ToString();
        }


        /// <summary>
        /// Checks if an object has a method. Used in ProductBase to detect if a 
        /// property formatter exists for a given property
        /// </summary>
        /// <param name="objectToCheck"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public static bool HasMethod(this object objectToCheck, string methodName)
        {
            var type = objectToCheck.GetType();
            return type.GetMethod(methodName) != null;
        }

        /// <summary>
        /// Checks if an object has a property.
        /// </summary>
        /// <param name="objectToCheck"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static bool HasProperty(this object objectToCheck, string propertyName)
        {
            var type = objectToCheck.GetType();
            return type.GetProperty(propertyName) != null;
        }

        /// <summary>
        /// Checks if an object has a method. Used in ProductBase to detect if a 
        /// property formatter exists for a given property
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public static object InvokeMethod(this object instance, string methodName, object[] parameters)
        {
            MethodInfo method = instance.GetType().GetMethod(methodName);
            if (method != null)
            {
                return method.Invoke(instance, parameters);
            }

            return null;
        }

        /// <summary>
        /// Returns the value of a property through reflection
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static object GetPropertyValue(this object instance, string propertyName)
        {
            return instance.GetType().GetProperty(propertyName).GetValue(instance, null);
        }

        public static List<Type> FindAllDerivedTypes<T>()
        {
            return FindAllDerivedTypes<T>(Assembly.GetAssembly(typeof(T)));
        }

        public static List<Type> FindAllDerivedTypes<T>(Assembly assembly)
        {
            var derivedType = typeof(T);
            return assembly
                .GetTypes()
                .Where(t =>
                    t != derivedType &&
                    derivedType.IsAssignableFrom(t)
                    ).ToList();

        }

        public static bool Implements<T>(this Type type)
        {
            var interfaceType = typeof (T);
            return interfaceType.IsAssignableFrom(type) && 
                !type.IsInterface && 
                !type.IsAbstract && 
                !type.IsGenericTypeDefinition;
        }

        //public static List<Type> GetImplementersOf<T>()
        //{
            //var type = typeof (T);
        //}
    }
}
