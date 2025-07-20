using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Kampai.Game.Mignette.AlligatorSkiing;
using Kampai.Game.Mignette.BalloonBarrage;
using Kampai.Game.Mignette.ButterflyCatch;
using Kampai.Game.Mignette.EdwardMinionHands;
using Kampai.Game.Mignette.WaterSlide;
using UnityEngine;

namespace Kampai.Util
{
	public static class GameObjectUtil
	{
		private static Dictionary<string, Type> typesLookup = new Dictionary<string, Type>
		{
			{
				"EdwardMinionHandsMignetteRoot",
				typeof(EdwardMinionHandsMignetteRoot)
			},
			{
				"ButterflyCatchMignetteRoot",
				typeof(ButterflyCatchMignetteRoot)
			},
			{
				"BalloonBarrageMignetteRoot",
				typeof(BalloonBarrageMignetteRoot)
			},
			{
				"WaterSlideMignetteRoot",
				typeof(WaterSlideMignetteRoot)
			},
			{
				"AlligatorSkiingMignetteRoot",
				typeof(AlligatorSkiingMignetteRoot)
			}
		};

		public static void TryRemoveComponent<T>(GameObject obj) where T : Component
		{
			T component = obj.GetComponent<T>();
			if (component != null)
			{
				UnityEngine.Object.Destroy(component);
			}
		}

		public static void TryEnableBehaviour<T>(GameObject obj, bool enable) where T : MonoBehaviour
		{
			T component = obj.GetComponent<T>();
			if (component != null)
			{
				component.enabled = enable;
			}
		}

		public static Component AddComponent(GameObject obj, string componentClassName, IKampaiLogger logger)
		{
			Type type = resolveComponentType(componentClassName, logger);
			return (type == null) ? null : obj.AddComponent(type);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static Type resolveComponentType(string componentClassName, IKampaiLogger logger)
		{
			Type value;
			if (typesLookup.TryGetValue(componentClassName, out value))
			{
				return value;
			}
			Assembly callingAssembly = Assembly.GetCallingAssembly();
			value = callingAssembly.GetType(componentClassName, false, false);
			if (value != null)
			{
				logger.Error("SLOW OPERATION: component type {0} found using reflection. Please add me to GameObjectUtil.typesLookup!", componentClassName);
				typesLookup.Add(componentClassName, value);
				return value;
			}
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				Type[] types = assembly.GetTypes();
				foreach (Type type in types)
				{
					if (type.Name == componentClassName || type.FullName == componentClassName)
					{
						logger.Error("(EVEN) SLOW(ER) OPERATION: component type {0} found using reflection. Please add me to GameObjectUtil.typesLookup!", componentClassName);
						typesLookup.Add(componentClassName, value);
						return value;
					}
				}
			}
			return null;
		}
	}
}
