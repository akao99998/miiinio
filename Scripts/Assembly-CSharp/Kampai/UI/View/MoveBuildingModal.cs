using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class MoveBuildingModal : WorldToGlassUIModal
	{
		public ButtonView InventoryButton;

		public ButtonView AcceptButton;

		public ButtonView CloseButton;

		public GameObject ItemCostPanel;

		public KampaiImage ItemIcon;

		public Text ItemCost;

		public GameObject InventoryBacking;

		public Text InventoryCount;

		public KampaiImage ItemBacking;
	}
}
