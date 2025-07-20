using System.Collections.Generic;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class SocialPartyFillOrderButtonView : ButtonView
	{
		public GameObject orderClosedPanel;

		public GameObject orderOpenPanel;

		public FillOrderButtonView fillOrderButton;

		public KampaiImage orderOpenImageIngredient;

		public Text orderOpenTextAmount;

		public Image orderOpenImageCheck;

		public KampaiImage grindImage;

		public KampaiImage xpImage;

		public Text xpReward;

		public Text grindReward;

		public Image orderClosedImagePicture;

		public Image orderClosedImageCheck;

		public KampaiImage orderClosedNoLoginImageCheck;

		public GameObject xpIcon;

		public GameObject funIcon;

		internal IList<QuantityItem> missingItems;

		public void CreatFillOrderPopupIndicator(string rewardImagePath, string rewardMaskPath)
		{
			orderOpenImageIngredient.sprite = UIUtils.LoadSpriteFromPath(rewardImagePath);
			orderOpenImageIngredient.maskSprite = UIUtils.LoadSpriteFromPath(rewardMaskPath);
		}

		public void SetButtonInteractable(bool isInteractable)
		{
			fillOrderButton.GetComponent<Button>().interactable = isInteractable;
		}
	}
}
