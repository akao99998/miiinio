using System;
using System.Collections.Generic;
using strange.extensions.injector.api;
using strange.extensions.pool.impl;

namespace Kampai.Util
{
	public class FastCommandPool
	{
		private const int warmupSize = 8;

		private object[] warmup = new object[8];

		private static object POOL_TAG = new object();

		private Dictionary<Type, Pool> pools = new Dictionary<Type, Pool>();

		private List<Tuple<Type, IInjectionBinder>> poolsToWarmUp = new List<Tuple<Type, IInjectionBinder>>();

		private Pool GetPoolForType(Type type, IInjectionBinder binder)
		{
			Pool value = null;
			if (pools.TryGetValue(type, out value))
			{
				return value;
			}
			Type o = typeof(Pool<>).MakeGenericType(type);
			binder.Bind(type).To(type);
			binder.Bind<Pool>().To(o).ToName(POOL_TAG);
			value = binder.GetInstance<Pool>(POOL_TAG);
			binder.Unbind<Pool>(POOL_TAG);
			pools.Add(type, value);
			for (int i = 0; i < 8; i++)
			{
				warmup[i] = value.GetInstance();
			}
			for (int j = 0; j < 8; j++)
			{
				value.ReturnInstance(warmup[j]);
			}
			return value;
		}

		public void ReturnToPool<T>(T command)
		{
			Pool value = null;
			if (pools.TryGetValue(command.GetType(), out value))
			{
				value.ReturnInstance(command);
			}
		}

		public T GetCommand<T>(IInjectionBinder binder) where T : class, IFastPooledCommandBase
		{
			T result = GetPoolForType(typeof(T), binder).GetInstance() as T;
			result.commandPool = this;
			return result;
		}

		public void WarmupPool<T>(IInjectionBinder binder)
		{
			poolsToWarmUp.Add(new Tuple<Type, IInjectionBinder>(typeof(T), binder));
		}

		public void Warmup()
		{
			foreach (Tuple<Type, IInjectionBinder> item in poolsToWarmUp)
			{
				GetPoolForType(item.Item1, item.Item2);
			}
			poolsToWarmUp.Clear();
		}
	}
}
