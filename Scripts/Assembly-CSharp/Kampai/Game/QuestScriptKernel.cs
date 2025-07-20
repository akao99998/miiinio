using System;
using System.Collections.Generic;
using System.Reflection;
using Elevation.Logging;
using Kampai.Util;

namespace Kampai.Game
{
	public class QuestScriptKernel
	{
		private readonly Dictionary<string, Func<QuestScriptController, IArgRetriever, ReturnValueContainer, bool>> apiFunctions = new Dictionary<string, Func<QuestScriptController, IArgRetriever, ReturnValueContainer, bool>>();

		private readonly List<SignalListener> signalListeners = new List<SignalListener>();

		private IKampaiLogger logger = LogManager.GetClassLogger("QuestScriptKernel") as IKampaiLogger;

		public QuestScriptKernel()
		{
			Type typeFromHandle = typeof(QuestScriptController);
			MethodInfo[] methods = typeFromHandle.GetMethods(BindingFlags.Instance | BindingFlags.Public);
			int i = 0;
			for (int num = methods.Length; i < num; i++)
			{
				MethodInfo methodInfo = methods[i];
				object[] customAttributes = methodInfo.GetCustomAttributes(typeof(QuestScriptAPIAttribute), false);
				int j = 0;
				for (int num2 = customAttributes.Length; j < num2; j++)
				{
					QuestScriptAPIAttribute questScriptAPIAttribute = customAttributes[j] as QuestScriptAPIAttribute;
					if (questScriptAPIAttribute != null)
					{
						Func<QuestScriptController, IArgRetriever, ReturnValueContainer, bool> func = null;
						try
						{
							func = Delegate.CreateDelegate(typeof(Func<QuestScriptController, IArgRetriever, ReturnValueContainer, bool>), methodInfo) as Func<QuestScriptController, IArgRetriever, ReturnValueContainer, bool>;
						}
						catch (ArgumentException ex)
						{
							logger.Error("Failed grabbing {0}: {1}", methodInfo.Name, ex.Message);
							continue;
						}
						if (func == null)
						{
							logger.Error("Cannot use {0} as a Quest Script API command since it doesn't fit the delegate.", methodInfo.Name);
						}
						else
						{
							apiFunctions[questScriptAPIAttribute.Name] = func;
						}
					}
				}
			}
		}

		public Func<QuestScriptController, IArgRetriever, ReturnValueContainer, bool> GetApiFunction(string name)
		{
			Func<QuestScriptController, IArgRetriever, ReturnValueContainer, bool> value = null;
			apiFunctions.TryGetValue(name, out value);
			return value;
		}

		public bool HasApiFunction(string name)
		{
			return apiFunctions.ContainsKey(name);
		}

		public void SignalDispatched<T1, T2, T3, T4>(string name, int paramCount, T1 p1, T2 p2, T3 p3, T4 p4)
		{
			SignalListener[] array = signalListeners.ToArray();
			int i = 0;
			for (int num = array.Length; i < num; i++)
			{
				array[i].SignalDispatched(name, paramCount, p1, p2, p3, p4);
			}
		}

		public void AddSignalListener(SignalListener listener)
		{
			if (!signalListeners.Contains(listener))
			{
				signalListeners.Add(listener);
			}
		}

		public void RemoveSignalListener(SignalListener listener)
		{
			int num = signalListeners.IndexOf(listener);
			if (num != -1)
			{
				signalListeners.RemoveAt(num);
			}
		}
	}
}
