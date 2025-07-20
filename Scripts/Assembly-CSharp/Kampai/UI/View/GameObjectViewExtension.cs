using UnityEngine;

namespace Kampai.UI.View
{
	public static class GameObjectViewExtension
	{
		public static void SafeDestoryChildViews<T>(this GameObject gameObject) where T : MonoBehaviour
		{
			if (!(gameObject == null))
			{
				UIUtils.SafeDestoryViews(gameObject.GetComponentsInChildren<T>());
			}
		}

		public static void SafeDestoryChildViews<T>(this RectTransform rectTransform) where T : MonoBehaviour
		{
			if (!(rectTransform == null))
			{
				rectTransform.gameObject.SafeDestoryChildViews<T>();
			}
		}
	}
}
