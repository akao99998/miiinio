using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	[RequireComponent(typeof(Animator))]
	public class OrderBoardRequiredItemView : PopupInfoButtonView
	{
		public KampaiImage ItemIcon;

		public Text ItemCount;

		public GameObject CheckMark;

		public GameObject XMark;

		public Animator IconAnimator;

		public new Signal<OrderBoardRequiredItemView, RectTransform> pointerDownSignal = new Signal<OrderBoardRequiredItemView, RectTransform>();

		public int ItemDefinitionID { get; set; }

		public bool playerHasEnoughItems { get; set; }

		public void Init()
		{
		}

		public override void OnPointerDown(PointerEventData eventData)
		{
			pointerDownSignal.Dispatch(this, ItemIcon.rectTransform);
		}
	}
}
