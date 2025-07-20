using Kampai.Game;
using Kampai.Game.View;
using Kampai.Main;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class ProceduralQuestView : PopupMenuView
	{
		public Text Title;

		public GameObject BackGround;

		public Text ItemAmountText;

		public Text CurrencyAmountText;

		public Text YesSellButtonText;

		public Text NoSellButtonText;

		public KampaiImage InputIconImage;

		public KampaiImage OutputIconImage;

		public Button YesSellButton;

		public Button NoSellButton;

		public GameObject CheckMarkImage;

		public GameObject CrossMarkImage;

		public MinionSlotModal minionSlot;

		private ILocalizationService localService;

		private DummyCharacterObject dummyCharacterObject;

		internal void Init(ILocalizationService localService, IFancyUIService fancyUIService, MoveAudioListenerSignal moveAudioListenerSignal)
		{
			base.Init();
			this.localService = localService;
			base.Open();
			DummyCharacterType type = DummyCharacterType.NamedCharacter;
			dummyCharacterObject = fancyUIService.CreateCharacter(type, DummyCharacterAnimationState.SelectedHappy, minionSlot.transform, minionSlot.VillainScale, minionSlot.VillainPositionOffset, 40014);
			dummyCharacterObject.gameObject.SetActive(true);
			moveAudioListenerSignal.Dispatch(false, dummyCharacterObject.transform);
		}

		internal void InitSellView(int requiredCount, int inventoryCount, int rewardCount, ItemDefinition inputItem, ItemDefinition outputItem)
		{
			if (requiredCount > inventoryCount)
			{
				CheckMarkImage.SetActive(false);
				CrossMarkImage.SetActive(true);
			}
			else
			{
				CheckMarkImage.SetActive(true);
				CrossMarkImage.SetActive(false);
			}
			ItemAmountText.text = string.Format("{0}/{1}", inventoryCount, requiredCount);
			CurrencyAmountText.text = UIUtils.FormatLargeNumber(rewardCount);
			YesSellButtonText.text = localService.GetString("Yes");
			NoSellButtonText.text = localService.GetString("No");
			Title.text = localService.GetString("SellTitle");
			InputIconImage.sprite = UIUtils.LoadSpriteFromPath(inputItem.Image);
			InputIconImage.maskSprite = UIUtils.LoadSpriteFromPath(inputItem.Mask);
			OutputIconImage.sprite = UIUtils.LoadSpriteFromPath(outputItem.Image);
			OutputIconImage.maskSprite = UIUtils.LoadSpriteFromPath(outputItem.Mask);
		}
	}
}
