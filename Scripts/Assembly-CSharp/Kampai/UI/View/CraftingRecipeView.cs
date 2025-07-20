using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Util;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class CraftingRecipeView : KampaiView, IPointerDownHandler, IDragHandler, IPointerUpHandler, IEventSystemHandler
	{
		public RectTransform lockedPanel;

		public RectTransform unlockedPanel;

		public RectTransform greenCircle;

		public RectTransform redCircle;

		public RectTransform groupQuantity;

		public KampaiImage lockedImage;

		public KampaiImage unlockedImage;

		public Text itemQuantity;

		private IDefinitionService definitionService;

		private IPlayerService playerService;

		private ItemDefinition itemDefinition;

		private IngredientsItemDefinition ingredientItemDefinition;

		private TransactionDefinition transactionDef;

		private Vector3 originalScaleUnlocked;

		public Signal<PointerEventData, IngredientsItemDefinition> pointerDownSignal = new Signal<PointerEventData, IngredientsItemDefinition>();

		public Signal<PointerEventData> pointerDragSignal = new Signal<PointerEventData>();

		public Signal<PointerEventData, IngredientsItemDefinition> pointerUpSignal = new Signal<PointerEventData, IngredientsItemDefinition>();

		public Signal<bool, bool> IsValidDragAreaSignal = new Signal<bool, bool>();

		public bool isHighlighted { get; set; }

		public bool isUnlocked { get; set; }

		public int recipeID { get; set; }

		public int instanceID { get; set; }

		internal void Init(IDefinitionService defService, IPlayerService pService)
		{
			definitionService = defService;
			playerService = pService;
			itemDefinition = definitionService.Get<ItemDefinition>(recipeID);
			ingredientItemDefinition = definitionService.Get<IngredientsItemDefinition>(recipeID);
			SetItemImageAndQuantity();
			SetImageBorder();
			if (unlockedImage != null)
			{
				originalScaleUnlocked = unlockedImage.gameObject.transform.localScale;
			}
			SetHighlight(isHighlighted);
		}

		private void SetItemImageAndQuantity()
		{
			if (ValidRecipe())
			{
				isUnlocked = true;
				unlockedImage.sprite = UIUtils.LoadSpriteFromPath(itemDefinition.Image);
				unlockedImage.maskSprite = UIUtils.LoadSpriteFromPath(itemDefinition.Mask);
				transactionDef = definitionService.Get<TransactionDefinition>(ingredientItemDefinition.TransactionId);
				SetQuantity();
			}
			else
			{
				isUnlocked = false;
				lockedPanel.gameObject.SetActive(true);
				unlockedPanel.gameObject.SetActive(false);
				lockedImage.maskSprite = UIUtils.LoadSpriteFromPath(itemDefinition.Mask);
			}
		}

		private bool ValidRecipe()
		{
			DynamicIngredientsDefinition definition;
			if (definitionService.TryGet<DynamicIngredientsDefinition>(recipeID, out definition))
			{
				return true;
			}
			return playerService.GetUnlockedQuantityOfID(recipeID) > 0;
		}

		public void SetQuantity()
		{
			itemQuantity.text = playerService.GetQuantityByDefinitionId(recipeID).ToString();
		}

		public void SetImageBorder()
		{
			if (!isUnlocked)
			{
				return;
			}
			bool flag = true;
			foreach (QuantityItem input in transactionDef.Inputs)
			{
				flag &= playerService.GetQuantityByDefinitionId(input.ID) >= input.Quantity;
			}
			if (flag)
			{
				greenCircle.gameObject.SetActive(true);
				redCircle.gameObject.SetActive(false);
			}
			else
			{
				greenCircle.gameObject.SetActive(false);
				redCircle.gameObject.SetActive(true);
			}
		}

		internal void SetHighlight(bool isHighlighted)
		{
			if (isUnlocked && !(unlockedImage == null))
			{
				if (isHighlighted)
				{
					TweenUtil.Throb(unlockedImage.gameObject.transform, 0.85f, 0.5f, out originalScaleUnlocked);
					return;
				}
				Go.killAllTweensWithTarget(unlockedImage.gameObject.transform);
				unlockedImage.gameObject.transform.localScale = originalScaleUnlocked;
			}
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			pointerDownSignal.Dispatch(eventData, ingredientItemDefinition);
		}

		public void OnDrag(PointerEventData eventData)
		{
			pointerDragSignal.Dispatch(eventData);
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			pointerUpSignal.Dispatch(eventData, ingredientItemDefinition);
		}
	}
}
