using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace Kampai.Util.MiniJSON
{
	public class Serializer
	{
		private StringBuilder builder;

		private Dictionary<Type, object> converters;

		public Serializer()
		{
			builder = new StringBuilder();
		}

		public void AddConverter(Type type, MiniJSONSerializeConverter converter)
		{
			if (converters == null)
			{
				converters = new Dictionary<Type, object>();
			}
			converters.Add(type, converter);
		}

		public static string Serialize(object obj)
		{
			Serializer serializer = new Serializer();
			return serializer.SerializeValue(obj).Build();
		}

		public string Build()
		{
			return builder.ToString();
		}

		public Serializer SerializeValue(object value)
		{
			if (value != null && converters != null)
			{
				Type type = value.GetType();
				foreach (Type key in converters.Keys)
				{
					if (key.IsAssignableFrom(type))
					{
						value = ((MiniJSONSerializeConverter)converters[key]).Convert(value);
					}
				}
			}
			string str;
			IList anArray;
			IDictionary obj;
			if (value == null)
			{
				builder.Append("null");
			}
			else if ((str = value as string) != null)
			{
				SerializeString(str);
			}
			else if (value is bool)
			{
				builder.Append((!(bool)value) ? "false" : "true");
			}
			else if ((anArray = value as IList) != null)
			{
				SerializeArray(anArray);
			}
			else if ((obj = value as IDictionary) != null)
			{
				SerializeObject(obj);
			}
			else if (value is char)
			{
				SerializeString(new string((char)value, 1));
			}
			else
			{
				SerializeOther(value);
			}
			return this;
		}

		public void SerializeObject(IDictionary obj)
		{
			bool flag = true;
			builder.Append('{');
			foreach (object key in obj.Keys)
			{
				if (!flag)
				{
					builder.Append(',');
				}
				SerializeString(key.ToString());
				builder.Append(':');
				SerializeValue(obj[key]);
				flag = false;
			}
			builder.Append('}');
		}

		public void SerializeArray(IList anArray)
		{
			builder.Append('[');
			bool flag = true;
			foreach (object item in anArray)
			{
				if (!flag)
				{
					builder.Append(',');
				}
				SerializeValue(item);
				flag = false;
			}
			builder.Append(']');
		}

		public void SerializeString(string str)
		{
			builder.Append('"');
			char[] array = str.ToCharArray();
			char[] array2 = array;
			foreach (char c in array2)
			{
				switch (c)
				{
				case '"':
					builder.Append("\\\"");
					continue;
				case '\\':
					builder.Append("\\\\");
					continue;
				case '\b':
					builder.Append("\\b");
					continue;
				case '\f':
					builder.Append("\\f");
					continue;
				case '\n':
					builder.Append("\\n");
					continue;
				case '\r':
					builder.Append("\\r");
					continue;
				case '\t':
					builder.Append("\\t");
					continue;
				}
				int num = Convert.ToInt32(c);
				if (num >= 32 && num <= 126)
				{
					builder.Append(c);
					continue;
				}
				builder.Append("\\u");
				builder.Append(num.ToString("x4"));
			}
			builder.Append('"');
		}

		public void SerializeOther(object value)
		{
			if (value is float)
			{
				builder.Append(((float)value).ToString("R"));
			}
			else if (value is int || value is uint || value is long || value is sbyte || value is byte || value is short || value is ushort || value is ulong)
			{
				builder.Append(value);
			}
			else if (value is double || value is decimal)
			{
				builder.Append(Convert.ToDouble(value).ToString("R"));
			}
			else if (value is Enum)
			{
				SerializeValue((int)value);
			}
			else
			{
				ReflectObject(value);
			}
		}

		public void ReflectObject(object obj)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			Type type = obj.GetType();
			FieldInfo[] fields = type.GetFields();
			FieldInfo[] array = fields;
			foreach (FieldInfo fieldInfo in array)
			{
				if (fieldInfo.GetCustomAttributes(typeof(JsonIgnoreAttribute), false).Length <= 0 && fieldInfo.IsPublic)
				{
					string name = fieldInfo.Name;
					object value = fieldInfo.GetValue(obj);
					dictionary.Add(name, value);
				}
			}
			PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
			PropertyInfo[] array2 = properties;
			foreach (PropertyInfo propertyInfo in array2)
			{
				if (propertyInfo.GetCustomAttributes(typeof(JsonIgnoreAttribute), false).Length <= 0 && propertyInfo.CanRead && propertyInfo.GetGetMethod() != null)
				{
					string name = propertyInfo.Name;
					object value = propertyInfo.GetValue(obj, null);
					dictionary.Add(name, value);
				}
			}
			SerializeObject(dictionary);
		}
	}
}
