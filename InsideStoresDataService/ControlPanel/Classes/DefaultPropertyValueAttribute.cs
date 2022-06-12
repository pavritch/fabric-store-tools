namespace ControlPanel
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public class DefaultPropertyValueAttribute : Attribute
    {
        public object Value { get; set; }
    }
}

