using System;
using System.Collections.Generic;
using Kampai.Util;

namespace Kampai.UI.View
{
	public class GUIArguments
	{
		private readonly IKampaiLogger logger;

		public Dictionary<Type, object> arguments = new Dictionary<Type, object>();

		public int Count
		{
			get
			{
				return arguments.Count;
			}
		}

		public GUIArguments(IKampaiLogger logger)
		{
			this.logger = logger;
		}

		public T Get<T>()
		{
			if (arguments.ContainsKey(typeof(T)))
			{
				return (T)arguments[typeof(T)];
			}
			return default(T);
		}

		public bool Contains<T>()
		{
			if (arguments.ContainsKey(typeof(T)))
			{
				return true;
			}
			return false;
		}

		public GUIArguments Add(object value)
		{
			return Add(value.GetType(), value);
		}

		public GUIArguments Add(Type type, object value)
		{
			if (value != null)
			{
				if (arguments.ContainsKey(type))
				{
					logger.Debug(string.Format("Overwriting previous GUIArguments value for type: {0}", type));
					arguments.Remove(type);
				}
				arguments.Add(type, value);
			}
			return this;
		}

		public void Remove(Type type)
		{
			if (arguments.ContainsKey(type))
			{
				arguments.Remove(type);
			}
		}

		public void AddArguments(GUIArguments other)
		{
			foreach (object value in other.arguments.Values)
			{
				Add(value);
			}
		}
	}
}
