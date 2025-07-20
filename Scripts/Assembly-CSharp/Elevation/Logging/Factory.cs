using System;
using System.Collections.Generic;

namespace Elevation.Logging
{
	public class Factory<T, U> where T : class where U : class
	{
		private readonly Dictionary<string, Func<U, T>> _dict = new Dictionary<string, Func<U, T>>();

		public T Create(string name, U arg)
		{
			Func<U, T> value = null;
			if (_dict.TryGetValue(name, out value))
			{
				return value(arg);
			}
			return (T)null;
		}

		public void Register(string name, Func<U, T> builder)
		{
			if (_dict.ContainsKey(name))
			{
				_dict[name] = builder;
			}
			else
			{
				_dict.Add(name, builder);
			}
		}
	}
}
