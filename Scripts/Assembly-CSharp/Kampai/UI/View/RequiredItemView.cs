using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class RequiredItemView : PopupInfoButtonView
	{
		public KampaiImage ItemIcon;

		public GameObject SolidCircle;

		public GameObject DashedCircle;

		public Text ItemQuantity;

		public Text ItemCost;

		public GameObject PurchasePanel;

		public RushButtonView RushBtn;

		public float PaddingInPixels;

		public new Signal<int, RectTransform> pointerDownSignal = new Signal<int, RectTransform>();

		public int ItemNeeded { get; set; }

		public int ItemDefinitionID { get; set; }

		public bool FullyAvailable { get; set; }

		public bool IsIngredient { get; set; }

		public override void OnPointerDown(PointerEventData eventData)
		{
			pointerDownSignal.Dispatch(ItemDefinitionID, ItemIcon.rectTransform);
		}

		protected override void Start()
		{
			base.Start();
			if (PurchasePanel != null)
			{
				PurchasePanel.gameObject.SetActive(false);
			}
		}
	}
}
