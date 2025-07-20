using System.Collections.Generic;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class CraftingPopupView : GenericPopupView
	{
		public Text ingredientsText;

		public Text unlockText;

		public Text ingredientOneQuantity;

		public Text ingredientTwoQuantity;

		public Text ingredientThreeQuantity;

		public Text ingredientFourQuantity;

		public KampaiImage ingredientOneIcon;

		public KampaiImage ingredientTwoIcon;

		public KampaiImage ingredientThreeIcon;

		public KampaiImage ingredientFourIcon;

		public RectTransform ingredientOne;

		public RectTransform ingredientTwo;

		public RectTransform ingredientThree;

		public RectTransform ingredientFour;

		public RectTransform ingredientOneGreen;

		public RectTransform ingredientOneRed;

		public RectTransform ingredientTwoGreen;

		public RectTransform ingredientTwoRed;

		public RectTransform ingredientThreeGreen;

		public RectTransform ingredientThreeRed;

		public RectTransform ingredientFourGreen;

		public RectTransform ingredientFourRed;

		public RectTransform pointer;

		private IPlayerService playerService;

		private IDefinitionService definitionService;

		private ILocalizationService localization;

		private float bufferAmt = 0.02f;

		internal void Display(Vector3 itemCenter, IPlayerService playerService, IDefinitionService defService, ILocalizationService localService, GameObject glassCanvas)
		{
			this.playerService = playerService;
			definitionService = defService;
			localization = localService;
			base.Init();
			SetPosition(itemCenter, 3f, glassCanvas);
			base.Open();
		}

		private void SetPosition(Vector3 itemCenter, float offsetValue, GameObject glassCanvas)
		{
			RectTransform rectTransform = base.transform as RectTransform;
			Vector3 anchoredPosition3D = rectTransform.sizeDelta / offsetValue;
			float x = (glassCanvas.transform as RectTransform).sizeDelta.x;
			float num = rectTransform.sizeDelta.x / x / 2f;
			anchoredPosition3D.x = 0f;
			rectTransform.anchoredPosition3D = anchoredPosition3D;
			float num2 = itemCenter.x - num;
			float num3 = itemCenter.x + num;
			float num4 = 0f;
			if (num2 < 0f)
			{
				num4 = 0f - num2 + bufferAmt;
			}
			else if (num3 > 1f)
			{
				num4 = 1f - num3 - bufferAmt;
			}
			Vector3 vector = new Vector2(num4, 0f);
			rectTransform.anchorMin = itemCenter + vector;
			rectTransform.anchorMax = rectTransform.anchorMin;
			Vector2 vector2 = new Vector2(num4 * x / rectTransform.sizeDelta.x, 0f);
			pointer.anchorMin -= vector2;
			pointer.anchorMax -= vector2;
		}

		internal void PopulateIngredients(IngredientsItemDefinition itemDef)
		{
			DisableAllIngredients();
			IList<QuantityItem> inputs = definitionService.Get<TransactionDefinition>(itemDef.TransactionId).Inputs;
			int count = inputs.Count;
			if (count < 1 || count > 4)
			{
				logger.Fatal(FatalCode.DS_INVALID_CRAFT, "Transaction {0} has an invaid number of inputs", itemDef.TransactionId);
			}
			DynamicIngredientsDefinition dynamicIngredientsDefinition = itemDef as DynamicIngredientsDefinition;
			int num = 0;
			if (dynamicIngredientsDefinition == null)
			{
				num = definitionService.GetLevelItemUnlocksAt(itemDef.ID);
			}
			if (num > (int)playerService.GetQuantity(StaticItem.LEVEL_ID))
			{
				ingredientsText.gameObject.SetActive(false);
				unlockText.gameObject.SetActive(true);
				unlockText.text = localization.GetString("UnlockAt", num);
				return;
			}
			for (int i = 0; i < count; i++)
			{
				int quantityByDefinitionId = (int)playerService.GetQuantityByDefinitionId(inputs[i].ID);
				switch (i)
				{
				case 0:
					SetupSlotOne(inputs[0], quantityByDefinitionId);
					break;
				case 1:
					SetupSlotTwo(inputs[1], quantityByDefinitionId);
					break;
				case 2:
					SetupSlotThree(inputs[2], quantityByDefinitionId);
					break;
				case 3:
					SetupSlotFour(inputs[3], quantityByDefinitionId);
					break;
				}
			}
		}

		private void DisableAllIngredients()
		{
			ingredientOne.gameObject.SetActive(false);
			ingredientTwo.gameObject.SetActive(false);
			ingredientThree.gameObject.SetActive(false);
			ingredientFour.gameObject.SetActive(false);
		}

		private void SetupSlotOne(QuantityItem input, int playerQuantity)
		{
			ItemDefinition itemDefinition = definitionService.Get<ItemDefinition>(input.ID);
			ingredientOneIcon.sprite = UIUtils.LoadSpriteFromPath(itemDefinition.Image);
			ingredientOneIcon.maskSprite = UIUtils.LoadSpriteFromPath(itemDefinition.Mask);
			ingredientOneQuantity.text = string.Format("{0}/{1}", playerQuantity, input.Quantity);
			if (playerQuantity < input.Quantity)
			{
				ingredientOneGreen.gameObject.SetActive(false);
				ingredientOneRed.gameObject.SetActive(true);
			}
			ingredientOne.gameObject.SetActive(true);
		}

		private void SetupSlotTwo(QuantityItem input, int playerQuantity)
		{
			ItemDefinition itemDefinition = definitionService.Get<ItemDefinition>(input.ID);
			ingredientTwoIcon.sprite = UIUtils.LoadSpriteFromPath(itemDefinition.Image);
			ingredientTwoIcon.maskSprite = UIUtils.LoadSpriteFromPath(itemDefinition.Mask);
			ingredientTwoQuantity.text = string.Format("{0}/{1}", playerQuantity, input.Quantity);
			if (playerQuantity < input.Quantity)
			{
				ingredientTwoGreen.gameObject.SetActive(false);
				ingredientTwoRed.gameObject.SetActive(true);
			}
			ingredientTwo.gameObject.SetActive(true);
		}

		private void SetupSlotThree(QuantityItem input, int playerQuantity)
		{
			ItemDefinition itemDefinition = definitionService.Get<ItemDefinition>(input.ID);
			ingredientThreeIcon.sprite = UIUtils.LoadSpriteFromPath(itemDefinition.Image);
			ingredientThreeIcon.maskSprite = UIUtils.LoadSpriteFromPath(itemDefinition.Mask);
			ingredientThreeQuantity.text = string.Format("{0}/{1}", playerQuantity, input.Quantity);
			if (playerQuantity < input.Quantity)
			{
				ingredientThreeGreen.gameObject.SetActive(false);
				ingredientThreeRed.gameObject.SetActive(true);
			}
			ingredientThree.gameObject.SetActive(true);
		}

		private void SetupSlotFour(QuantityItem input, int playerQuantity)
		{
			ItemDefinition itemDefinition = definitionService.Get<ItemDefinition>(input.ID);
			ingredientFourIcon.sprite = UIUtils.LoadSpriteFromPath(itemDefinition.Image);
			ingredientFourIcon.maskSprite = UIUtils.LoadSpriteFromPath(itemDefinition.Mask);
			ingredientFourQuantity.text = string.Format("{0}/{1}", playerQuantity, input.Quantity);
			if (playerQuantity < input.Quantity)
			{
				ingredientFourGreen.gameObject.SetActive(false);
				ingredientFourRed.gameObject.SetActive(true);
			}
			ingredientFour.gameObject.SetActive(true);
		}
	}
}
