using System;
using System.Collections;
using System.Collections.Generic;

namespace ControlPanel
{
	public abstract class EnumDataProvider<T>
	{
        private string firstValue = null;

        public class NameValuePair
        {
            private string value;
            public string Name {get{return value;}}            
            public string Value {get{return value;}}
                        
            public NameValuePair(string value)
            {
                this.value = value;
            }
        }

		IEnumerable _data;

        public EnumDataProvider()
        {


        }

        /// <summary>
        /// Allow a value to be passed in which prepends to the usual list.
        /// </summary>
        /// <param name="firstValue"></param>
        public EnumDataProvider(string firstValue)
        {
            this.firstValue = firstValue;
        }

		public IEnumerable Data
		{
			get
			{
				if (this._data == null)
				{

					Type enumType = typeof(T);
					List<object> list = new List<object>();

                    if (firstValue != null)
                        list.Add(new NameValuePair(firstValue));
                    
                    int value = 0;

                    while (Enum.IsDefined(enumType, value))
					{
                        var e = Enum.ToObject(enumType, value);
                        list.Add(new NameValuePair(e.ToString()));
						value++;
					}

					this._data = list;
				}

				return this._data;
			}
		}
	}
}
