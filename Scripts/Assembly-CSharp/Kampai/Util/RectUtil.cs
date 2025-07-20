using UnityEngine;

namespace Kampai.Util
{
	public static class RectUtil
	{
		public static void Copy(RectTransform from, RectTransform to)
		{
			to.anchorMax = from.anchorMax;
			to.anchorMin = from.anchorMin;
			to.offsetMax = from.offsetMax;
			to.offsetMin = from.offsetMin;
			to.anchoredPosition = from.anchoredPosition;
			to.localScale = from.localScale;
			to.localPosition = from.localPosition;
		}
	}
}
