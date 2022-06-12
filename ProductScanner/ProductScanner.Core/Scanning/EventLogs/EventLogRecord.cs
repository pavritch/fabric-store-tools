using ProductScanner.Core.VendorTesting;

namespace ProductScanner.Core.Scanning.EventLogs
{
    public class EventLogRecord
    {
        public EventType EventType {get;set;}
        public string Text {get;set;}

        public EventLogRecord() { }
        public EventLogRecord(string message, params object[] args)
        {
            Text = string.Format(message, args);
        }

        public EventLogRecord(EventType eventType, string message, params object[] args)
        {
            EventType = eventType;
            Text = string.Format(message, args);
        }

        public override string ToString()
        {
            return this.Text;
        }

        public static EventLogRecord Warn(string message)
        {
            return new EventLogRecord(EventType.Warning, message);
        }

        public static EventLogRecord Error(string message)
        {
            return new EventLogRecord(EventType.Error, message);
        }

        public static EventLogRecord Error(string message, params object[] args)
        {
            var log = new EventLogRecord(message, args);
            log.EventType = EventType.Error;
            return log;
        }

        public static EventLogRecord Warn(string message, params object[] args)
        {
            var log = new EventLogRecord(message, args);
            log.EventType = EventType.Warning;
            return log;
        }
    }
}
