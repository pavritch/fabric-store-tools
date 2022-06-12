// Enum Utilities
// http://www.codeproject.com/KB/cs/EnumTools.aspx

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace Utilities
{
    /**
    <summary>
        Contains methods for working with enum values.
    </summary>
    */

    #region LibEnum
    public static class LibEnum
    {
        private static readonly Dictionary<
            PropertyInfo
            ,
            Dictionary<
                System.Enum
                ,
                string
                >
            > alternatetexts;

        private static readonly PropertyInfo description;

        private static readonly Dictionary<
            System.Enum
            ,
            string
            > descriptions;

        private static readonly string[] separators = new[] { ", " };
        private static readonly StringBuilder tempstring = new StringBuilder();

        static LibEnum
            (
            )
        {
            alternatetexts = new Dictionary<
                PropertyInfo
                ,
                Dictionary<
                    System.Enum
                    ,
                    string
                    >
                >();

            alternatetexts.Add
                (
                description = typeof(DescriptionAttribute).GetProperty("Description")
                ,
                descriptions = new Dictionary<
                                   System.Enum
                                   ,
                                   string
                                   >()
                );

            return;
        }

        /**************************************************************************************************************/

        /**
        <summary>
            Retrieves the type of the member values.
        </summary>
        <returns>
            The type of the values of the enumeration.
        </returns>
        */

        // System.Type t = LibEnum.GetUnderlyingType<MyEnum>() ;

        public static Type
            GetUnderlyingType<T>
            (
            )
        {
            return (System.Enum.GetUnderlyingType(typeof(T)));
        }

        /**************************************************************************************************************/

        /**
        <summary>
            Retrieves the names of the members.
        </summary>
        <returns>
            An array containing the member names.
        </returns>
        */

        // string[] names = LibEnum.GetNames<MyEnum>() ;

        public static string[]
            GetNames<T>
            (
            )
        {
            return (System.Enum.GetNames(typeof(T)));
        }

        /**************************************************************************************************************/

        /**
        <summary>
            Retrieves the name of the value.
        </summary>
        <param name="Value">
            The value whose name is desired.
        </param>
        <returns>
            The name of the value.
        </returns>
        */

        public static string
            GetName
            (
            System.Enum Value
            )
        {
            return (System.Enum.GetName(Value.GetType(), Value));
        }

        /**************************************************************************************************************/

        /**
        <summary>
            Formats the value.
        </summary>
        <param name="Value">
            The value whose formatted value is desired.
        </param>
        <param name="Format">
            The format to use.
        </param>
        <returns>
            A string containing the value after formatting.
        </returns>
        */

        // string s = LibEnum.Format ( MyEnum.Value1 , "00" ) ;

        public static string
            Format
            (
            System.Enum Value
            ,
            string Format
            )
        {
            return (System.Enum.Format(Value.GetType(), Value, Format));
        }

        /**************************************************************************************************************/

        /**
        <summary>
            Retrieves the values of the members.
        </summary>
        <returns>
            An array containing the member values.
        </returns>
        */

        // MyEnum[] values = LibEnum.GetValues<MyEnum>() ;

        public static T[]
            GetValues<T>
            (
            )
        {
            return ((T[])System.Enum.GetValues(typeof(T)));
        }

        /**************************************************************************************************************/

        /**
        <summary>
            Determines whether or not the value exists in the enum.
        </summary>
        <param name="Value">
            The value to check.
        </param>
        <returns>
            Whether or not the value was found.
        </returns>
        */

        // bool b ;
        //
        // b = LibEnum.IsDefined<MyEnum> ( "Value1" ) ;         // Case-sensitive
        //
        // b = LibEnum.IsDefined<MyEnum> ( "Value1" , false ) ; // Case-sensitive
        // b = LibEnum.IsDefined<MyEnum> ( "Value1" , true  ) ; // Case-insensitive

        public static bool
            IsDefined<T>
            (
            object Value
            )
        {
            return (System.Enum.IsDefined(typeof(T), Value));
        }

        /**************************************************************************************************************/

        /**
        <summary>
            Determines whether or not the name exists in the enum.
        </summary>
        <param name="Name">
            The string to check.
        </param>
        <param name="IgnoreCase">
            Whether or not to perform a case-insensitive search.
        </param>
        <returns>
            Whether or not the name was found.
        </returns>
        */

        public static bool
            IsDefined<T>
            (
            string Name
            ,
            bool IgnoreCase
            )
        {
            bool result = false;

            if (!IgnoreCase)
            {
                result = IsDefined<T>(Name);
            }
            else
            {
                string[] names = System.Enum.GetNames(typeof(T));

                for (int runner = 0; !result && (runner < names.Length); runner++)
                {
                    result = StringComparer.CurrentCultureIgnoreCase.Compare
                                 (
                                 Name
                                 ,
                                 names[runner]
                                 ) == 0;
                }
            }

            return (result);
        }

        /**************************************************************************************************************/

        /**
        <summary>
            Retrieves the value associated with the provided name.
        </summary>
        <param name="Name">
            The string to parse.
        </param>
        <returns>
            The requested value if it exists.
        </returns>
        */

        // MyEnum x = LibEnum.Parse<MyEnum>("Value1");         // Case-sensitive
        // MyEnum x = LibEnum.Parse<MyEnum>("Value1", false); // Case-sensitive
        // MyEnum x = LibEnum.Parse<MyEnum>("Value1", true);  // Case-insensitive


        public static T
            Parse<T>
            (
            string Name
            )
        {
            return
                (
                    (T)System.Enum.Parse
                            (
                            typeof(T)
                            ,
                            Name
                            )
                );
        }

        /**
        <summary>
            Retrieves the value associated with the provided name.
        </summary>
        <param name="Name">
            The string to parse.
        </param>
        <param name="IgnoreCase">
            Whether or not to perform a case-insensitive parse.
        </param>
        <returns>
            The requested value if it exists.
        </returns>
        */

        public static T
            Parse<T>
            (
            string Name
            ,
            bool IgnoreCase
            )
        {
            return
                (
                    (T)System.Enum.Parse
                            (
                            typeof(T)
                            ,
                            Name
                            ,
                            IgnoreCase
                            )
                );
        }

        /**************************************************************************************************************/

        /**
        <summary>
            Retrieves the value associated with the provided name.
        </summary>
        <param name="Name">
            The string to parse.
        </param>
        <param name="Result">
            Parameter to hold the requested value (or the default if the name wasn't found).
        </param>
        <returns>
            Whether or not the name was found.
        </returns>
        */

        // MyEnum x ;
        // 
        // LibEnum.TryParse<MyEnum> ( "Value1" , out x ) ;         // Case-sensitive
        // 
        // LibEnum.TryParse<MyEnum> ( "Value1" , false , out x ) ; // Case-sensitive
        // LibEnum.TryParse<MyEnum> ( "Value1" , true  , out x ) ; // Case-insensitive


        public static bool
            TryParse<T>
            (
            string Name
            ,
            out T Result
            )
        {
            bool result = false;

            try
            {
                Result = (T)System.Enum.Parse
                                 (
                                 typeof(T)
                                 ,
                                 Name
                                 );

                result = true;
            }
            catch (ArgumentNullException)
            {
                Result = Default<T>();
            }
            catch (ArgumentException)
            {
                Result = Default<T>();
            }

            return (result);
        }

        /**
        <summary>
            Retrieves the value associated with the provided name.
        </summary>
        <param name="Name">
            The string to parse.
        </param>
        <param name="IgnoreCase">
            Whether or not to perform a case-insensitive parse.
        </param>
        <param name="Result">
            Parameter to hold the requested value (or the default if the name wasn't found).
        </param>
        <returns>
            Whether or not the name was found.
        </returns>
        */

        public static bool
            TryParse<T>
            (
            string Name
            ,
            bool IgnoreCase
            ,
            out T Result
            )
        {
            bool result = false;

            try
            {
                Result = (T)System.Enum.Parse
                                 (
                                 typeof(T)
                                 ,
                                 Name
                                 ,
                                 IgnoreCase
                                 );

                result = true;
            }
            catch (ArgumentNullException)
            {
                Result = Default<T>();
            }
            catch (ArgumentException)
            {
                Result = Default<T>();
            }

            return (result);
        }

        /**************************************************************************************************************/

        private static string
            GetPropertyValue
            (
            PropertyInfo Property
            ,
            System.Enum Value
            )
        {
            lock (tempstring)
            {
                tempstring.Remove(0, tempstring.Length);

                foreach
                    (
                    string value
                        in
                        Value.ToString().Split
                            (
                            separators
                            ,
                            StringSplitOptions.RemoveEmptyEntries
                            )
                    )
                {
                    foreach
                        (
                        Attribute att
                            in
                            (Attribute[])
                            Value.GetType().GetField(value).GetCustomAttributes
                                (
                                Property.DeclaringType
                                ,
                                false
                                )
                        )
                    {
                        if (tempstring.Length > 0)
                        {
                            tempstring.Append(separators[0]);
                        }

                        tempstring.Append(Property.GetValue(att, null).ToString());
                    }
                }

                return (tempstring.ToString());
            }
        }

        /**************************************************************************************************************/

        /**
        <summary>
            Retrieve the value of a System.ComponentModel.DescriptionAttribute on an enum value, if any.
        </summary>
        <remarks>
            When Value reflects multiple values of an Enum with the FlagsAttribute
            all the descriptions are returned as a comma-separated list.
        </remarks>
        <param name="Value">
            The value whose Description is desired.
        </param>
        <returns>
            The value of the System.ComponentModel.DescriptionAttribute or an empty string.
        </returns>
        */

        // enum MyEnum
        // {
        //    [System.ComponentModel.DescriptionAttribute("The first value")]
        //    Value1 = 1,
        //    [System.ComponentModel.DescriptionAttribute("The second value")]
        //    Value2 = 2
        // }
        // string s = LibEnum.GetDescription(MyEnum.Value1);


        public static string
            GetDescription
            (
            System.Enum Value
            )
        {
            lock (alternatetexts)
            {
                if (!descriptions.ContainsKey(Value))
                {
                    descriptions[Value] = GetPropertyValue
                        (
                        description
                        ,
                        Value
                        );
                }
            }

            return (descriptions[Value]);
        }

        /**************************************************************************************************************/

        /**
        <summary>
            Retrieve the value of an attribute's property on an enum value, if any.
        </summary>
        <remarks>
            When Value reflects multiple values of an Enum with the FlagsAttribute
            all the alternate texts are returned as a comma-separated list.
        </remarks>
        <param name="Value">
            The value whose alternate text is desired.
        </param>
        <param name="Property">
            The property of an attribute to use as the source for the alternate text.
        </param>
        <returns>
            The value of the Property or an empty string.
        </returns>
        <exception cref="System.ArgumentException">
            The Property is not from an Attribute
        </exception>
        */

        public static string
            GetAlternateText
            (
            System.Enum Value
            ,
            PropertyInfo Property
            )
        {
            if (Property == null)
            {
                throw (new ArgumentNullException
                    (
                    "Property"
                    ,
                    "The Property must not be null"
                    ));
            }

            if (!Property.ReflectedType.IsSubclassOf(typeof(Attribute)))
            {
                throw (new ArgumentException
                    (
                    "The Property must be from an Attribute"
                    ));
            }

            lock (alternatetexts)
            {
                if (!alternatetexts.ContainsKey(Property))
                {
                    alternatetexts[Property] = new Dictionary<
                        System.Enum
                        ,
                        string
                        >();
                }

                if (!alternatetexts[Property].ContainsKey(Value))
                {
                    alternatetexts[Property][Value] = GetPropertyValue
                        (
                        Property
                        ,
                        Value
                        );
                }
            }

            return (alternatetexts[Property][Value]);
        }

        /**************************************************************************************************************/

        /**
        <summary>
            Retrieves the default value of the enumeration.
        </summary>
        <remarks>
            The standard default for an enum is 0, but this method will honor a 
            PIEBALD.Attributes.EnumDefaultValueAttribute if one has been applied by the developer of the enum.
        </remarks>
        <returns>
            The default value of the enumeration.
        </returns>
        */

        public static T
            Default<T>
            (
            )
        {
            T result;

            EnumDefaultValueAttribute.GetDefaultValue(out result);

            return (result);
        }

        /**************************************************************************************************************/
    }

    #endregion
    #region EnumDefaultValue

    #pragma warning disable 3001

    /**
        <summary>
            An attribute to apply a different default value to an enum.
        </summary>
        */
    [AttributeUsage
    (
        System.AttributeTargets.Enum
    ,
        AllowMultiple = false
    ,
        Inherited = false
    )]
    public sealed class EnumDefaultValueAttribute : System.Attribute
    {
        private static readonly System.Collections.Generic.Dictionary
        <
            System.Type
        ,
            object
        > defaults = new System.Collections.Generic.Dictionary
        <
            System.Type
        ,
            object
        >();

        private readonly object value;

        /**************************************************************************************************************/

        /**
        <summary>
            Specifies that the provided value should be the default for the affected enum.
        </summary>
        <param name="Value">
            The value to use as the default for the enum.
        </param>
        */
        public EnumDefaultValueAttribute
        (
            System.Enum Value
        )
            : base()
        {
            this.value = Value;

            return;
        }

        /**
        <summary>
            Specifies that the provided value should be the default for the affected enum.
        </summary>
        <param name="Value">
            The value to use as the default for the enum.
        </param>
        */
        public EnumDefaultValueAttribute
        (
            System.SByte Value
        )
            : base()
        {
            this.value = Value;

            return;
        }

        /**
        <summary>
            Specifies that the provided value should be the default for the affected enum.
        </summary>
        <param name="Value">
            The value to use as the default for the enum.
        </param>
        */
        public EnumDefaultValueAttribute
        (
            System.Byte Value
        )
            : base()
        {
            this.value = Value;

            return;
        }

        /**
        <summary>
            Specifies that the provided value should be the default for the affected enum.
        </summary>
        <param name="Value">
            The value to use as the default for the enum.
        </param>
        */
        public EnumDefaultValueAttribute
        (
            System.Int16 Value
        )
            : base()
        {
            this.value = Value;

            return;
        }

        /**
        <summary>
            Specifies that the provided value should be the default for the affected enum.
        </summary>
        <param name="Value">
            The value to use as the default for the enum.
        </param>
        */
        public EnumDefaultValueAttribute
        (
            System.UInt16 Value
        )
            : base()
        {
            this.value = Value;

            return;
        }

        /**
        <summary>
            Specifies that the provided value should be the default for the affected enum.
        </summary>
        <param name="Value">
            The value to use as the default for the enum.
        </param>
        */
        public EnumDefaultValueAttribute
        (
            System.Int32 Value
        )
            : base()
        {
            this.value = Value;

            return;
        }

        /**
        <summary>
            Specifies that the provided value should be the default for the affected enum.
        </summary>
        <param name="Value">
            The value to use as the default for the enum.
        </param>
        */
        public EnumDefaultValueAttribute
        (
            System.UInt32 Value
        )
            : base()
        {
            this.value = Value;

            return;
        }

        /**
        <summary>
            Specifies that the provided value should be the default for the affected enum.
        </summary>
        <param name="Value">
            The value to use as the default for the enum.
        </param>
        */
        public EnumDefaultValueAttribute
        (
            System.Int64 Value
        )
            : base()
        {
            this.value = Value;

            return;
        }

        /**
        <summary>
            Specifies that the provided value should be the default for the affected enum.
        </summary>
        <param name="Value">
            The value to use as the default for the enum.
        </param>
        */
        public EnumDefaultValueAttribute
        (
            System.UInt64 Value
        )
            : base()
        {
            this.value = Value;

            return;
        }

        /**
        <summary>
            Specifies that the provided value should be the default for the affected enum.
        </summary>
        <param name="Value">
            The value to use as the default for the enum.
        </param>
        */
        public EnumDefaultValueAttribute
        (
            System.String Value
        )
            : base()
        {
            this.value = Value;

            return;
        }

        /**************************************************************************************************************/

        /**
        <summary>
            Performs ToString() on the value.
        </summary>
        <returns>
            The value as a string.
        </returns>
        */
        public override string
        ToString
        (
        )
        {
            return (this.value.ToString());
        }

        /**************************************************************************************************************/

        /**
        <summary>
            Retrieve the default value for the enum.
        </summary>
        <param name="Result">
            Will contain the default value.
        </param>
        <returns>
            Whether or not the default value is from an EnumDefaultValueAttribute.
        </returns>
        <exception cref="System.ArgumentException">
            If the supplied type is not an enum.
        </exception>
        */
        public static bool
        GetDefaultValue<T>
        (
            out T Result
        )
        {
            bool result = false;

            lock (defaults)
            {
                System.Type thetype = typeof(T);

                if (defaults.ContainsKey(thetype))
                {
                    Result = (T)defaults[thetype];

                    result = true;
                }
                else
                {
                    if (!thetype.IsEnum)
                    {
                        throw (new System.ArgumentException("T must be an Enum"));
                    }

                    EnumDefaultValueAttribute[] atts =
                        (EnumDefaultValueAttribute[])thetype.GetCustomAttributes
                        (
                            typeof(EnumDefaultValueAttribute)
                        ,
                            false
                        );

                    Result = (T)System.Enum.GetValues(thetype).GetValue(0);

                    if (atts.Length > 0)
                    {
                        EnumDefaultValueAttribute att = atts[0];

                        if (att.value != null)
                        {
                            if (att.value is System.String)
                            {
                                try
                                {
                                    Result = (T)System.Enum.Parse
                                    (
                                        thetype
                                    ,
                                        (string)att.value
                                    );

                                    result = true;
                                }
                                catch (System.ArgumentException err)
                                {
                                    throw (new System.ArgumentException
                                    (
                                        string.Format
                                        (
                                            "The value ({0}) specified by the EnumDefaultValueAttribute " +
                                            "on {1} could not be parsed"
                                        ,
                                            att.value
                                        ,
                                            thetype.ToString()
                                        )
                                    ,
                                        err
                                    ));
                                }
                            }
                            else
                            {
                                try
                                {
                                    Result = (T)System.Convert.ChangeType
                                    (
                                        att.value
                                    ,
                                        System.Enum.GetUnderlyingType(thetype)
                                    );

                                    result = true;
                                }
                                catch (System.OverflowException err)
                                {
                                    throw (new System.ArgumentException
                                    (
                                        string.Format
                                        (
                                            "The value ({0}) specified by the EnumDefaultValueAttribute " +
                                            "on {1} cannot be converted to {2}"
                                        ,
                                            att.value
                                        ,
                                            thetype.ToString()
                                        ,
                                            System.Enum.GetUnderlyingType(thetype).ToString()
                                        )
                                    ,
                                        err
                                    ));
                                }
                                catch (System.InvalidCastException err)
                                {
                                    throw (new System.ArgumentException
                                    (
                                        string.Format
                                        (
                                            "The value ({0}) specified by the EnumDefaultValueAttribute " +
                                            "on {1} cannot be converted to {2}"
                                        ,
                                            att.value
                                        ,
                                            thetype.ToString()
                                        ,
                                            System.Enum.GetUnderlyingType(thetype).ToString()
                                        )
                                    ,
                                        err
                                    ));
                                }
                            }
                        }
                        else
                        {
                            throw (new System.ArgumentNullException
                            (
                                null
                            ,
                                string.Format
                                (
                                    "The value (<null>) specified by the EnumDefaultValueAttribute " +
                                    "on {0} could not be parsed"
                                ,
                                    thetype.ToString()
                                )
                            ));
                        }
                    }

                    defaults[thetype] = Result;
                }
            }

            return (result);
        }

        /**************************************************************************************************************/
    }
    #pragma warning restore 3001

    #endregion

}