namespace Conversations.Common.Configuration
{
    using System;
    using System.ComponentModel;

    public class GenericConverter
    {
        public static T GetValue<T>(object value,
                                    T defaultValue)
        {
            var nullCheck = value as DBNull;
            if (nullCheck != null)
            {
                return default(T);
            }
            if (value.ToString() == string.Empty)
            {
                return default(T);
            }

            if (TypeDescriptor.GetConverter(typeof(T)) != null && TypeDescriptor.GetConverter(typeof(T)).CanConvertFrom(typeof(string)))
            {
                return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(value.ToString());
            }
            if (TypeDescriptor.GetConverter(typeof(string)).CanConvertTo(typeof(T)))
            {
                return (T)TypeDescriptor.GetConverter(typeof(string)).ConvertTo(value, typeof(T));
            }
            throw new Exception(string.Format("Cannot convert value: {0} to type {1}", value, typeof(T)));
        }

        public static T GetValue<T>(object value)
        {
            var nullCheck = value as DBNull;
            if (nullCheck != null)
            {
                return default(T);
            }

            var val = (T)value;
            if (!Equals(val, default(T)))
            {
                return val;
            }

            if (value.ToString() == string.Empty)
            {
                return default(T);
            }

            if (TypeDescriptor.GetConverter(typeof(T)) != null && TypeDescriptor.GetConverter(typeof(T)).CanConvertFrom(typeof(string)))
            {
                return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(value.ToString());
            }
            else if (TypeDescriptor.GetConverter(typeof(string)).CanConvertTo(typeof(T)))
            {
                return (T)TypeDescriptor.GetConverter(typeof(string)).ConvertTo(value, typeof(T));
            }
            else
            {
                throw new Exception(string.Format("Cannot convert value: {0} to type {1}", value, typeof(T)));
            }
        }

        public static T Parse<T>(string sourceValue,
                                  IFormatProvider provider) where T : IConvertible
        {
            return (T)Convert.ChangeType(sourceValue, typeof(T), provider);
        }
    }
}
