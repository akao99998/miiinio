using System;
using System.Collections;
using System.Collections.Generic;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;

namespace Kampai.UI.View
{
	public class LandExpansionRushDialogMediator : RushDialogMediator<LandExpansionRushDialogView>
	{
		private LandExpansionConfig expansionDefinition;

		private BridgeBuilding bridgeBuilding;

		private TransactionDefinition transactionDef;

		private IEnumerator PointerDownWait;

		[Inject]
		public ILandExpansionConfigService landExpansionConfigService { get; set; }

		[Inject]
		public PurchaseLandExpansionSignal purchaseSignal { get; set; }

		[Inject]
		public RepairBridgeSignal repairBridgeSignal { get; set; }

		[Inject]
		public IQuestService questService { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public DisplayItemPopupSignal displayItemPopupSignal { get; set; }

		[Inject]
		public HideItemPopupSignal hideItemPopupSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playSFXSignal { get; set; }

		[Inject]
		public SetStorageCapacitySignal setStorageSignal { get; set; }

		[Inject]
		public OpenStoreHighlightItemSignal openStoreSignal { get; set; }

		[Inject]
		public PanAndOpenModalSignal moveToBuildingSignal { get; set; }

		[Inject]
		public CameraAutoZoomSignal autoZoomSignal { get; set; }

		[Inject(MainElement.CAMERA)]
		public Camera mainCamera { get; set; }

		[Inject]
		public CraftingModalParams craftingModalParams { get; set; }

		public override void Initialize(GUIArguments args)
		{
			BuildingPopupPositionData buildingPopupPositionData = args.Get<BuildingPopupPositionData>();
			base.view.InitProgrammatic(buildingPopupPositionData, base.localService);
			dialogType = args.Get<RushDialogView.RushDialogType>();
			switch (dialogType)
			{
			case RushDialogView.RushDialogType.LAND_EXPANSION:
			{
				int expansion = args.Get<int>();
				expansionDefinition = landExpansionConfigService.GetExpansionConfig(expansion);
				transactionDef = base.definitionService.Get<TransactionDefinition>(expansionDefinition.transactionId);
				break;
			}
			case RushDialogView.RushDialogType.BRIDGE_QUEST:
			{
				int id = args.Get<int>();
				bridgeBuilding = base.playerService.GetByInstanceId<BridgeBuilding>(id);
				if (bridgeBuilding.BridgeId == 0)
				{
					return;
				}
				BridgeDefinition bridgeDefinition = base.definitionService.Get(bridgeBuilding.BridgeId) as BridgeDefinition;
				transactionDef = base.definitionService.Get(bridgeDefinition.TransactionId) as TransactionDefinition;
				break;
			}
			default:
				logger.Error("Unsupported dialog type: {0}", dialogType);
				break;
			}
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
						if (item.FullyAvailable || !item.IsIngredient)
						{
							item.pointerUpSignal.RemoveListener(PointerUpAvailable);
							item.pointerDownSignal.RemoveListener(PointerDownAvailable);
						}
						else
						{
							item.pointerUpSignal.RemoveListener(PointerUpUnavailable);
							item.pointerDownSignal.RemoveListener(PointerDownUnavailable);
						}
					}
				}
			}
			switch (dialogType)
			{
			case RushDialogView.RushDialogType.LAND_EXPANSION:
				base.hideSkrim.Dispatch("LandExpansionSkrim");
				break;
			case RushDialogView.RushDialogType.BRIDGE_QUEST:
				base.hideSkrim.Dispatch("BridgeSkrim");
				break;
			}
			base.guiService.Execute(GUIOperation.Unload, "popup_Confirmation_Expansion");
		}

		private void PointerDown(int itemDefinitionID, RectTransform rectTransform, bool isAvailable)
		{
			if (!isClosing && itemDefinitionID != 0)
			{
				if (PointerDownWait != null)
				{
					StopCoroutine(PointerDownWait);
					PointerDownWait = null;
				}
				displayItemPopupSignal.Dispatch(itemDefinitionID, rectTransform, (!isAvailable) ? UIPopupType.GENERICGOTO : UIPopupType.GENERIC);
			}
		}

		private void PointerUp(bool isAvailable)
		{
			if (PointerDownWait == null)
			{
				PointerDownWait = HideItemPopupAfter((!isAvailable) ? 1f : 0.5f);
				StartCoroutine(PointerDownWait);
			}
		}

		private void PointerDownAvailable(int itemDefinitionID, RectTransform rectTransform)
		{
			PointerDown(itemDefinitionID, rectTransform, true);
		}

		private void PointerUpAvailable()
		{
			PointerUp(true);
		}

		private void PointerDownUnavailable(int itemDefinitionID, RectTransform rectTransform)
		{
			PointerDown(itemDefinitionID, rectTransform, false);
		}

		private void PointerUpUnavailable()
		{
			PointerUp(false);
		}

		private IEnumerator HideItemPopupAfter(float seconds)
		{
			yield return new WaitForSeconds(seconds);
			hideItemPopupSignal.Dispatch();
		}

		protected override void PurchaseSuccess()
		{
			purchaseSuccess = true;
			playSFXSignal.Dispatch("Play_button_click_01");
			TryRunTheActualTransaction();
		}

		private void PerformTransactionSuccessAction(LandExpansionConfig config)
		{
			switch (dialogType)
			{
			case RushDialogView.RushDialogType.LAND_EXPANSION:
				purchaseSignal.Dispatch(config.expansionId, true);
				break;
			case RushDialogView.RushDialogType.BRIDGE_QUEST:
				questService.UpdateAllQuestsWithQuestStepType(QuestStepType.BridgeRepair, QuestTaskTransition.Start, bridgeBuilding);
				questService.UpdateAllQuestsWithQuestStepType(QuestStepType.BridgeRepair, QuestTaskTransition.Complete, bridgeBuilding);
				repairBridgeSignal.Dispatch(bridgeBuilding);
				break;
			}
			base.setGrindCurrencySignal.Dispatch();
		}

		protected override void Close()
		{
			base.Close();
			if (dialogType == RushDialogView.RushDialogType.LAND_EXPANSION)
			{
				HighlightLandExpansionSignal instance = gameContext.injectionBinder.GetInstance<HighlightLandExpansionSignal>();
				instance.Dispatch(expansionDefinition.expansionId, false);
			}
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
			GameObject original = ((definition.ID != 0) ? (KampaiResources.Load("cmp_RequiredItem_ForShow") as GameObject) : (KampaiResources.Load("cmp_CurrencyRequiredItem_ForShow") as GameObject));
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
			if (definition.ID == 0)
			{
				component.ItemQuantity.text = UIUtils.FormatLargeNumber(num);
			}
			else
			{
				component.ItemQuantity.text = string.Format("{0}/{1}", itemsInInventory, num);
			}
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
			component.FullyAvailable = isAvailable;
			component.IsIngredient = itemDefinition is IngredientsItemDefinition;
			if (!isAvailable)
			{
				component.ItemQuantity.color = Color.red;
				QuantityItem item = new QuantityItem(itemDefinition.ID, (uint)num2);
				component.RushBtn.RushCost = num3;
				component.RushBtn.Item = item;
			}
			if (isAvailable || !component.IsIngredient)
			{
				component.pointerUpSignal.AddListener(PointerUpAvailable);
				component.pointerDownSignal.AddListener(PointerDownAvailable);
			}
			else
			{
				component.pointerUpSignal.AddListener(PointerUpUnavailable);
				component.pointerDownSignal.AddListener(PointerDownUnavailable);
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

		private void ExpansionTransactionCallback(PendingCurrencyTransaction pct)
		{
			if (!pct.Success)
			{
				return;
			}
			foreach (QuantityItem output in pct.GetPendingTransaction().Outputs)
			{
				Definition definition = base.definitionService.Get<Definition>(output.ID);
				LandExpansionConfig landExpansionConfig = definition as LandExpansionConfig;
				if (landExpansionConfig != null)
				{
					PerformTransactionSuccessAction(landExpansionConfig);
				}
			}
			setStorageSignal.Dispatch();
			Close();
		}

		private void TryRunTheActualTransaction()
		{
			switch (dialogType)
			{
			case RushDialogView.RushDialogType.BRIDGE_QUEST:
				base.playerService.RunEntireTransaction(transactionDef, TransactionTarget.REPAIR_BRIDGE, ExpansionTransactionCallback);
				break;
			case RushDialogView.RushDialogType.LAND_EXPANSION:
				base.playerService.RunEntireTransaction(transactionDef, TransactionTarget.LAND_EXPANSION, ExpansionTransactionCallback);
				break;
			}
		}

		protected override void LoadItems(TransactionDefinition transactionDefinition, RushDialogView.RushDialogType type)
		{
			if (type == RushDialogView.RushDialogType.LAND_EXPANSION)
			{
				LoadExpansionItems(transactionDefinition, type);
			}
			else
			{
				LoadNormalItems(transactionDef, type);
			}
		}

		private void LoadExpansionItems(TransactionDefinition transactionDefinition, RushDialogView.RushDialogType type)
		{
			IList<QuantityItem> inputs = transactionDefinition.Inputs;
			if (inputs == null)
			{
				return;
			}
			bool showPurchaseButton = false;
			requiredItems = new List<QuantityItem>();
			requiredItemPremiumCosts = new List<int>();
			int num = 0;
			for (int i = 0; i < inputs.Count; i++)
			{
				int iD = inputs[i].ID;
				int num2 = -1;
				ItemDefinition definition = base.definitionService.Get<ItemDefinition>(iD);
				if (iD > 1)
				{
					num2 = num++;
				}
				QuantityItem quantityItem = null;
				uint quantityByDefinitionId = base.playerService.GetQuantityByDefinitionId(iD);
				int num3 = (int)(inputs[i].Quantity - quantityByDefinitionId);
				bool flag = false;
				if (num3 <= 0)
				{
					flag = true;
				}
				else
				{
					flag = false;
					quantityItem = new QuantityItem(iD, (uint)num3);
					requiredItems.Add(quantityItem);
				}
				RequiredItemView requiredItemView = BuildItem(definition, quantityByDefinitionId, num3, flag, base.localService);
				if (!flag)
				{
					requiredItemView.RushBtn.RushButtonClickedSignal.AddListener(base.IndividualRushButtonClicked);
				}
				int intFromFormattedLargeNumber = UIUtils.GetIntFromFormattedLargeNumber(requiredItemView.ItemCost.text);
				if (intFromFormattedLargeNumber != 0)
				{
					requiredItemPremiumCosts.Add(intFromFormattedLargeNumber);
				}
				if (num2 != -1)
				{
					base.view.AddRequiredItem(requiredItemView, num2, base.view.ScrollViewParent);
				}
				else
				{
					base.view.AddRequiredItem(requiredItemView, -1, base.view.CurrencyScrollViewParent);
				}
			}
			if (num <= 2)
			{
				base.view.MoveRequiredItem();
			}
			if (requiredItems.Count != 0)
			{
				rushCost = base.playerService.CalculateRushCost(requiredItems);
				base.view.SetupItemCost(rushCost);
				showPurchaseButton = true;
			}
			base.view.SetupItemCount(num);
			base.view.SetupDialog(type, showPurchaseButton);
			base.gameObject.SetActive(true);
		}

		private void LoadNormalItems(TransactionDefinition transactionDefinition, RushDialogView.RushDialogType type)
		{
			IList<QuantityItem> inputs = transactionDefinition.Inputs;
			if (inputs != null)
			{
				int count = inputs.Count;
				int num = -1;
				int index = 0;
				bool showPurchaseButton = false;
				requiredItems = new List<QuantityItem>();
				requiredItemPremiumCosts = new List<int>();
				for (int i = 0; i < count; i++)
				{
					ItemDefinition itemDefinition = base.definitionService.Get<ItemDefinition>(inputs[i].ID);
					if (itemDefinition.ID == 0)
					{
						num = i;
					}
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
					RequiredItemView requiredItemView = BuildItem(itemDefinition, quantityByDefinitionId, num2, flag, base.localService);
					if (!flag)
					{
						requiredItemView.RushBtn.RushButtonClickedSignal.AddListener(base.IndividualRushButtonClicked);
					}
					int intFromFormattedLargeNumber = UIUtils.GetIntFromFormattedLargeNumber(requiredItemView.ItemCost.text);
					if (intFromFormattedLargeNumber != 0)
					{
						requiredItemPremiumCosts.Add(intFromFormattedLargeNumber);
					}
					if (num < 0)
					{
						index = i;
					}
					else if (num == i)
					{
						index = -1;
					}
					else if (i > num)
					{
						index = i - 1;
					}
					base.view.AddRequiredItem(requiredItemView, index, (num != i) ? base.view.ScrollViewParent : base.view.CurrencyScrollViewParent);
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
				string sourceName = "unknown";
				switch (dialogType)
				{
				case RushDialogView.RushDialogType.LAND_EXPANSION:
					sourceName = "LandExpansion";
					break;
				case RushDialogView.RushDialogType.BRIDGE_QUEST:
					sourceName = "BrokenBridge";
					break;
				}
				PendingCurrencyTransaction pct = new PendingCurrencyTransaction(transactionDef, true, rushCost, null, null);
				bool flag = purchaseSuccess | purchaseInProgress;
				base.telemetryService.Send_Telemetry_EVT_PINCH_PROMPT(sourceName, pct, requiredItems, flag.ToString());
			}
		}

		private void OpenBuildingsStore(int buildingDefId)
		{
			openStoreSignal.Dispatch(buildingDefId, true);
			float currentPercentage = mainCamera.GetComponent<ZoomView>().GetCurrentPercentage();
			if (currentPercentage > 0.4f)
			{
				autoZoomSignal.Dispatch(0.4f);
			}
		}

		private void GotoResourceBuilding(int itemDefinitionId)
		{
			Close();
			int buildingDefintionIDFromItemDefintionID = base.definitionService.GetBuildingDefintionIDFromItemDefintionID(itemDefinitionId);
			ICollection<Building> byDefinitionId = base.playerService.GetByDefinitionId<Building>(buildingDefintionIDFromItemDefintionID);
			if (byDefinitionId.Count == 0)
			{
				OpenBuildingsStore(buildingDefintionIDFromItemDefintionID);
				return;
			}
			Building suitableBuilding = GotoBuildingHelpers.GetSuitableBuilding(byDefinitionId);
			if (suitableBuilding.State == BuildingState.Inventory)
			{
				OpenBuildingsStore(buildingDefintionIDFromItemDefintionID);
				return;
			}
			CraftingBuilding craftingBuilding = suitableBuilding as CraftingBuilding;
			if (craftingBuilding != null)
			{
				craftingModalParams.itemId = itemDefinitionId;
				craftingModalParams.highlight = true;
			}
			moveToBuildingSignal.Dispatch(suitableBuilding.ID, false);
		}

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
	}
}
