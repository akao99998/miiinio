using System;
using UnityEngine;
using strange.framework.api;

namespace Kampai.Util
{
	public class ResourceInstanceProvider : IInstanceProvider
	{
		private GameObject prototype;

		private string resourceName;

		public ResourceInstanceProvider(string name)
		{
			resourceName = name;
		}

		public T GetInstance<T>()
		{
			object instance = GetInstance(typeof(T));
			return (T)instance;
		}

		public object GetInstance(Type key)
		{
			if (prototype == null)
			{
				prototype = Resources.Load<GameObject>(resourceName);
			}
			return UnityEngine.Object.Instantiate(prototype);
		}
	}
}
