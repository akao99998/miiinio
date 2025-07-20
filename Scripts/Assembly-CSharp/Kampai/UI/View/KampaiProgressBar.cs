using UnityEngine;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class KampaiProgressBar : strange.extensions.mediation.impl.View
	{
		public RectTransform FillImage;

		public void SetProgress(float ratio)
		{
			FillImage.anchorMax = new Vector2(ratio, FillImage.anchorMax.y);
		}
	}
}
