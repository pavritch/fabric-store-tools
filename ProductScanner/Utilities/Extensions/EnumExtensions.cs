using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Utilities.Extensions
{
    public static class ErrorExtensions
    {
        public static void WriteEventLog(this Exception ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine(ex.ToJSON());
            WriteEventLog(EventLogEntryType.Error, sb.ToString());
        }

        public static void WriteEventLog(EventLogEntryType evtType, string msg)
        {
            //string _source = "ProductScanner.App";
            //string _log = "Application";

            //if (!EventLog.SourceExists(_source))
                //EventLog.CreateEventSource(_source, _log);

            //EventLog.WriteEntry(_source, msg, evtType);
        }
    }

    public static class EnumExtensions
    {
        public static bool HasAttribute<T>(this Enum enumVal) where T : Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
            return (attributes.Length > 0);
        }

        public static IEnumerable<T> MaskToList<T>(this Enum mask)
        {
            if (typeof(T).IsSubclassOf(typeof(Enum)) == false)
                throw new ArgumentException();

            return Enum.GetValues(typeof (T))
                .Cast<Enum>()
                .Where(mask.HasFlag)
                // we don't want to count the 0 value enum
                .Where(x => Convert.ToInt32(x) != 0)
                .Cast<T>();
        }
    }
}