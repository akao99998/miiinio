using System;
using System.Collections;
using System.Collections.Generic;
using Kampai.Common;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;

namespace Kampai.UI.View
{
	public class VillainLairUnlockRushDialogMediator : RushDialogMediator<VillainLairUnlockRushDialogView>
	{
		private TransactionDefinition transactionDef;

		private IEnumerator PointerDownWait;

		private VillainLairEntranceBuilding lair;

		[Inject]
		public DisplayItemPopupSignal displayItemPopupSignal { get; set; }

		[Inject]
		public HideItemPopupSignal hideItemPopupSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playSFXSignal { get; set; }

		[Inject]
		public SetStorageCapacitySignal setStorageSignal { get; set; }

		[Inject]
		public RepairBuildingSignal repairBuildingSignal { get; set; }

		[Inject]
		public IGoToService gotoService { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			base.gotoSignal.AddListener(GotoResourceBuilding);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			base.gotoSignal.RemoveListener(GotoResourceBuilding);
		}

		public override void Initialize(GUIArguments args)
		{
			BuildingPopupPositionData buildingPopupPositionData = args.Get<BuildingPopupPositionData>();
			base.view.InitProgrammatic(buildingPopupPositionData, base.localService);
			dialogType = args.Get<RushDialogView.RushDialogType>();
			int id = args.Get<int>();
			lair = base.playerService.GetByInstanceId<VillainLairEntranceBuilding>(id);
			transactionDef = base.definitionService.Get<TransactionDefinition>(lair.Definition.TransactionID);
			LoadItems(transactionDef, dialogType);
		}

		protected override void OnMenuClose()
		{
			SendRushTelemetry();
			IList<RequiredItemView> itemList = base.view.GetItemList();
			if (itemList != null)
			{
				foreach (RequiredItemView item in itemList)
				{
					if (item != null)
					{
						if (!item.FullyAvailable && item.IsIngredient)
						{
							item.pointerUpSignal.RemoveListener(PointerUpWithGoto);
							item.pointerDownSignal.RemoveListener(PointerDownWithGoto);
						}
						else
						{
							item.pointerUpSignal.RemoveListener(PointerUpWithoutGoto);
							item.pointerDownSignal.RemoveListener(PointerDownWithoutGoto);
						}
					}
				}
			}
			base.guiService.Execute(GUIOperation.Unload, "screen_UnlockLair");
			base.hideSkrim.Dispatch("VillainLairPortalSkrim");
		}

		private void PointerDown(int itemDefinitionID, RectTransform rectTransform, bool showGoto)
		{
			if (!isClosing)
			{
				if (PointerDownWait != null)
				{
					StopCoroutine(PointerDownWait);
					PointerDownWait = null;
				}
				displayItemPopupSignal.Dispatch(itemDefinitionID, rectTransform, showGoto ? UIPopupType.GENERICGOTO : UIPopupType.GENERIC);
			}
		}

		private void PointerUp(bool showGoto)
		{
			if (PointerDownWait == null)
			{
				PointerDownWait = HideItemPopupAfter((!showGoto) ? 0.5f : 1f);
				StartCoroutine(PointerDownWait);
			}
		}

		private IEnumerator HideItemPopupAfter(float seconds)
		{
			yield return new WaitForSeconds(seconds);
			hideItemPopupSignal.Dispatch();
		}

		private void PointerDownWithGoto(int itemDefinitionID, RectTransform rectTransform)
		{
			PointerDown(itemDefinitionID, rectTransform, true);
		}

		private void PointerUpWithGoto()
		{
			PointerUp(true);
		}

		private void PointerDownWithoutGoto(int itemDefinitionID, RectTransform rectTransform)
		{
			PointerDown(itemDefinitionID, rectTransform, false);
		}

		private void PointerUpWithoutGoto()
		{
			PointerUp(false);
		}

		protected override void PurchaseSuccess()
		{
			purchaseSuccess = true;
			playSFXSignal.Dispatch("Play_button_click_01");
			base.playerService.RunEntireTransaction(transactionDef, TransactionTarget.NO_VISUAL, RepairPortalTransactionCallback);
		}

		protected override void Close()
		{
			base.Close();
			playSFXSignal.Dispatch("Play_menu_disappear_01");
			hideItemPopupSignal.Dispatch();
			base.view.Close();
		}

		internal override RequiredItemView BuildItem(Definition definition, uint itemsInInventory, int itemsLack, bool isAvailable, ILocalizationService localService)
		{
			if (definition == null)
			{
				throw new ArgumentNullException("definition", "RequiredItemBuilder: You are passing in null definitions!");
			}
			GameObject original = KampaiResources.Load("cmp_UnlockLair") as GameObject;
			GameObject gameObject = UnityEngine.Object.Instantiate(original);
			RequiredItemView component = gameObject.GetComponent<RequiredItemView>();
			ItemDefinition itemDefinition = definition as ItemDefinition;
			Sprite sprite = UIUtils.LoadSpriteFromPath(itemDefinition.Image);
			Sprite maskSprite = UIUtils.LoadSpriteFromPath(itemDefinition.Mask);
			component.ItemIcon.sprite = sprite;
			component.ItemIcon.maskSprite = maskSprite;
			int num = (int)itemsInInventory + itemsLack;
			int num2 = ((itemsLack >= 0) ? itemsLack : 0);
			component.ItemNeeded = num;
			component.ItemDefinitionID = itemDefinition.ID;
			component.ItemQuantity.text = string.Format("{0}/{1}", itemsInInventory, num);
			component.PurchasePanel.SetActive(!isAvailable);
			int num3 = Mathf.FloorToInt(itemDefinition.BasePremiumCost * (float)num2);
			num3 = ((num3 == 0 && num2 > 0) ? 1 : num3);
			component.ItemCost.text = UIUtils.FormatLargeNumber(num3);
			if (component.DashedCircle != null)
			{
				component.DashedCircle.SetActive(!isAvailable);
			}
			if (component.SolidCircle != null)
			{
				component.SolidCircle.SetActive(isAvailable);
			}
			if (!isAvailable)
			{
				component.ItemQuantity.color = Color.red;
				QuantityItem item = new QuantityItem(itemDefinition.ID, (uint)num2);
				component.RushBtn.RushCost = num3;
				component.RushBtn.Item = item;
			}
			component.FullyAvailable = isAvailable;
			component.IsIngredient = itemDefinition is IngredientsItemDefinition;
			if (!isAvailable && component.IsIngredient)
			{
				component.pointerUpSignal.AddListener(PointerUpWithGoto);
				component.pointerDownSignal.AddListener(PointerDownWithGoto);
			}
			else
			{
				component.pointerUpSignal.AddListener(PointerUpWithoutGoto);
				component.pointerDownSignal.AddListener(PointerDownWithoutGoto);
			}
			return component;
		}

		protected override void RushTransactionCallback(PendingCurrencyTransaction pct)
		{
			purchaseInProgress = false;
			if (pct.Success)
			{
				isClosing = true;
				PurchaseSuccess();
				base.soundFXSignal.Dispatch("Play_button_premium_01");
				base.setPremiumCurrencySignal.Dispatch();
				base.setGrindCurrencySignal.Dispatch();
			}
			else if (pct.ParentSuccess)
			{
				PurchaseButtonClicked();
			}
		}

		private void RepairPortalTransactionCallback(PendingCurrencyTransaction pct)
		{
			if (pct.Success)
			{
				setStorageSignal.Dispatch();
				lair.SetState(BuildingState.Broken);
				repairBuildingSignal.Dispatch(lair);
				Close();
			}
		}

		protected override void LoadItems(TransactionDefinition transactionDefinition, RushDialogView.RushDialogType type)
		{
			IList<QuantityItem> inputs = transactionDefinition.Inputs;
			if (inputs != null)
			{
				int count = inputs.Count;
				int num = -1;
				bool showPurchaseButton = false;
				requiredItems = new List<QuantityItem>();
				requiredItemPremiumCosts = new List<int>();
				for (int i = 0; i < count; i++)
				{
					ItemDefinition definition = base.definitionService.Get<ItemDefinition>(inputs[i].ID);
					QuantityItem quantityItem = null;
					uint quantityByDefinitionId = base.playerService.GetQuantityByDefinitionId(inputs[i].ID);
					int num2 = (int)(inputs[i].Quantity - quantityByDefinitionId);
					bool flag = false;
					if (num2 <= 0)
					{
						flag = true;
					}
					else
					{
						flag = false;
						quantityItem = new QuantityItem(inputs[i].ID, (uint)num2);
						requiredItems.Add(quantityItem);
					}
					RequiredItemView requiredItemView = BuildItem(definition, quantityByDefinitionId, num2, flag, base.localService);
					if (!flag)
					{
						requiredItemView.RushBtn.RushButtonClickedSignal.AddListener(base.IndividualRushButtonClicked);
					}
					int intFromFormattedLargeNumber = UIUtils.GetIntFromFormattedLargeNumber(requiredItemView.ItemCost.text);
					if (intFromFormattedLargeNumber != 0)
					{
						requiredItemPremiumCosts.Add(intFromFormattedLargeNumber);
					}
					base.view.AddRequiredItem(requiredItemView, i, base.view.ScrollViewParent);
				}
				if (requiredItems.Count != 0)
				{
					rushCost = base.playerService.CalculateRushCost(requiredItems);
					base.view.SetupItemCost(rushCost);
					showPurchaseButton = true;
				}
				if (num < 0)
				{
					base.view.SetupItemCount(count);
				}
				else
				{
					base.view.SetupItemCount(count - 1);
				}
				base.view.SetupDialog(type, showPurchaseButton);
				base.gameObject.SetActive(true);
			}
			else
			{
				logger.Debug("Showing rush dialog without require items");
			}
		}

		private void SendRushTelemetry()
		{
			if (requiredItems != null && requiredItems.Count != 0)
			{
				string sourceName = "VillainLairUnlock";
				PendingCurrencyTransaction pct = new PendingCurrencyTransaction(transactionDef, true, rushCost, null, null);
				bool flag = purchaseSuccess | purchaseInProgress;
				base.telemetryService.Send_Telemetry_EVT_PINCH_PROMPT(sourceName, pct, requiredItems, flag.ToString());
			}
		}

		private void GotoResourceBuilding(int itemDefinitionId)
		{
			Close();
			gotoService.GoToBuildingFromItem(itemDefinitionId);
		}
	}
}
