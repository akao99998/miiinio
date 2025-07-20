using Kampai.Game;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class ResourcePlotUnlockRushDialogView : RushDialogView
	{
		public Text productionDescription;

		public Button leftButton;

		public Button rightButton;

		public KampaiImage resourceItem;

		public Text resourceItemAmt;

		internal void EnableArrows(bool enable)
		{
			leftButton.gameObject.SetActive(enable);
			rightButton.gameObject.SetActive(enable);
		}

		internal void SetProductionDescription(ItemDefinition itemDef, int itemAmount, string prodDesc)
		{
			productionDescription.text = prodDesc;
			resourceItem.sprite = UIUtils.LoadSpriteFromPath(itemDef.Image);
			resourceItem.maskSprite = UIUtils.LoadSpriteFromPath(itemDef.Mask);
			resourceItemAmt.text = itemAmount.ToString();
		}

		internal override void SetupItemCount(int count)
		{
		}
	}
}
