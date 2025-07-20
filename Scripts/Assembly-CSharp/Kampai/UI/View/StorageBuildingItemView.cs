using Kampai.Game;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class StorageBuildingItemView : KampaiView
	{
		public PopupInfoButtonView InfoButtonView;

		public KampaiImage ItemIcon;

		public Text ItemQuantity;

		public Item StorageItem { get; set; }

		public Vector2 OffsetMinDestination { get; set; }

		public Vector2 OffsetMaxDestination { get; set; }

		public Vector2 AnchorMinDestination { get; set; }

		public Vector2 AnchorMaxDestination { get; set; }

		public void UpdatePos()
		{
			RectTransform rectTransform = base.transform as RectTransform;
			rectTransform.offsetMin = OffsetMinDestination;
			rectTransform.offsetMax = OffsetMaxDestination;
			rectTransform.anchorMin = AnchorMinDestination;
			rectTransform.anchorMax = AnchorMaxDestination;
		}

		public void MoveToAnchorOffset(float moveTime, Vector2 newOffsetMin, Vector2 newOffsetMax, Vector2 newAnchorMin, Vector2 newAnchorMax)
		{
			RectTransform rectTransform = base.transform as RectTransform;
			OffsetMinDestination = rectTransform.offsetMin;
			OffsetMaxDestination = rectTransform.offsetMax;
			AnchorMinDestination = rectTransform.anchorMin;
			AnchorMaxDestination = rectTransform.anchorMax;
			Go.to(this, moveTime, new GoTweenConfig().setEaseType(GoEaseType.Linear).vector2Prop("OffsetMinDestination", newOffsetMin).vector2Prop("OffsetMaxDestination", newOffsetMax)
				.vector2Prop("AnchorMinDestination", newAnchorMin)
				.vector2Prop("AnchorMaxDestination", newAnchorMax)
				.onUpdate(delegate
				{
					UpdatePos();
				})
				.onComplete(delegate(AbstractGoTween thisTween)
				{
					thisTween.destroy();
				}));
		}

		internal void SelectItem(bool isSelected)
		{
			if (isSelected)
			{
				Vector3 originalScale = Vector3.one;
				TweenUtil.Throb(ItemIcon.transform, 0.85f, 0.5f, out originalScale);
			}
			else
			{
				Go.killAllTweensWithTarget(ItemIcon.transform);
				ItemIcon.transform.localScale = Vector3.one;
			}
		}
	}
}
