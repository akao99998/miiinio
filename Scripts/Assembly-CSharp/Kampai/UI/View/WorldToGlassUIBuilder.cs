using Kampai.Util;
using UnityEngine;

namespace Kampai.UI.View
{
	public static class WorldToGlassUIBuilder
	{
		public static T Build<T>(string prefab, Transform i_parent, WorldToGlassUISettings settings, IKampaiLogger logger) where T : WorldToGlassView
		{
			if (string.IsNullOrEmpty(prefab) || i_parent == null)
			{
				logger.Fatal(FatalCode.EX_NULL_ARG);
			}
			GameObject gameObject = Object.Instantiate(KampaiResources.Load(prefab)) as GameObject;
			gameObject.transform.SetParent(i_parent, false);
			gameObject.transform.SetAsFirstSibling();
			gameObject.SetActive(true);
			WorldToGlassUIModal component = gameObject.GetComponent<WorldToGlassUIModal>();
			component.Settings = settings;
			return gameObject.AddComponent<T>();
		}
	}
}
