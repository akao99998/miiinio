using System;
using System.Collections.Generic;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;

namespace Kampai.UI.View
{
	public class GenericRushDialogMediator : RushDialogMediator<GenericRushDialogView>
	{
		private readonly AdPlacementName adPlacementName = AdPlacementName.MISSING_RESOURCES;

		private AdPlacementInstance adPlacementInstance;

		[Inject]
		public IRewardedAdService rewardedAdService { get; set; }

		[Inject]
		public RewardedAdRewardSignal rewardedAdRewardSignal { get; set; }

		[Inject]
		public AdPlacementActivityStateChangedSignal adPlacementActivityStateChangedSignal { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			base.view.AdVideoButton.ClickedSignal.AddListener(AdVideoButtonClicked);
			rewardedAdRewardSignal.AddListener(OnRewardedAdReward);
			adPlacementActivityStateChangedSignal.AddListener(OnAdPlacementActivityStateChanged);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			base.view.AdVideoButton.ClickedSignal.RemoveListener(AdVideoButtonClicked);
			rewardedAdRewardSignal.RemoveListener(OnRewardedAdReward);
			adPlacementActivityStateChangedSignal.RemoveListener(OnAdPlacementActivityStateChanged);
		}

		protected override void SetHeadline(PendingCurrencyTransaction pct)
		{
			string transactionItemName = TransactionUtil.GetTransactionItemName(pct.GetPendingTransaction(), base.definitionService);
			if (!string.IsNullOrEmpty(transactionItemName))
			{
				base.view.HeadlineTitle.text = base.localService.GetString(transactionItemName);
			}
			else if (dialogType == RushDialogView.RushDialogType.STORAGE_EXPAND)
			{
				base.view.HeadlineTitle.text = base.localService.GetString("ExpandStorage");
			}
			else if (pct.GetTransactionTarget() == TransactionTarget.CLEAR_DEBRIS)
			{
				base.view.HeadlineTitle.text = base.localService.GetString("ClearX", base.localService.GetString("Debris"));
			}
		}

		internal override RequiredItemView BuildItem(Definition definition, uint itemsInInventory, int itemsLack, bool isAvailable, ILocalizationService localService)
		{
			if (definition == null)
			{
				throw new ArgumentNullException("definition", "GenericRequiredItemBuilder: You are passing in null definitions!");
			}
			GameObject original = KampaiResources.Load("cmp_CraftingResourceItem") as GameObject;
			GameObject gameObject = UnityEngine.Object.Instantiate(original);
			GenericRequiredItemView component = gameObject.GetComponent<GenericRequiredItemView>();
			ItemDefinition itemDefinition = definition as ItemDefinition;
			Sprite sprite = UIUtils.LoadSpriteFromPath(itemDefinition.Image);
			Sprite maskSprite = UIUtils.LoadSpriteFromPath(itemDefinition.Mask);
			component.ItemIcon.sprite = sprite;
			component.ItemIcon.maskSprite = maskSprite;
			int num = ((itemsLack >= 0) ? itemsLack : 0);
			int num2 = (int)itemsInInventory + itemsLack;
			component.ItemQuantity.text = string.Format("{0}/{1}", itemsInInventory, num2);
			int num3 = Mathf.FloorToInt(itemDefinition.BasePremiumCost * (float)num);
			num3 = ((num3 == 0 && num > 0) ? 1 : num3);
			component.Cost = num3;
			if (!isAvailable)
			{
				component.redBorder.gameObject.SetActive(true);
				component.ItemQuantity.color = Color.red;
			}
			else
			{
				component.greenBorder.gameObject.SetActive(true);
				component.ItemQuantity.color = GameConstants.UI.UI_TEXT_GREY;
			}
			return component;
		}

		protected override void LoadItems(TransactionDefinition transactionDefinition, RushDialogView.RushDialogType type)
		{
			IList<QuantityItem> inputs = transactionDefinition.Inputs;
			if (inputs != null)
			{
				int count = inputs.Count;
				bool showPurchaseButton = false;
				requiredItems = new List<QuantityItem>();
				requiredItemPremiumCosts = new List<int>();
				for (int i = 0; i < count; i++)
				{
					ItemDefinition itemdefinition = base.definitionService.Get<ItemDefinition>(inputs[i].ID);
					QuantityItem quantityItem = null;
					uint quantityByDefinitionId = base.playerService.GetQuantityByDefinitionId(inputs[i].ID);
					int num = (int)(inputs[i].Quantity - quantityByDefinitionId);
					bool flag = false;
					if (num <= 0)
					{
						flag = true;
					}
					else
					{
						flag = false;
						quantityItem = new QuantityItem(inputs[i].ID, (uint)num);
						requiredItems.Add(quantityItem);
					}
					RequiredItemView requiredItemView = BuildItem(itemdefinition, quantityByDefinitionId, num, flag, base.localService);
					GenericRequiredItemView genericRequiredItemView = requiredItemView as GenericRequiredItemView;
					if (genericRequiredItemView.Cost != 0)
					{
						requiredItemPremiumCosts.Add(genericRequiredItemView.Cost);
					}
					if (!flag)
					{
						requiredItemView.ClickedSignal.AddListener(delegate
						{
							gotoButtonHandler(itemdefinition.ID);
						});
					}
					base.view.AddRequiredItem(requiredItemView, i, base.view.ScrollViewParent);
				}
				if (requiredItems.Count != 0)
				{
					rushCost = base.playerService.CalculateRushCost(requiredItems);
					base.view.SetupItemCost(rushCost);
					showPurchaseButton = true;
				}
				base.view.SetupItemCount(count);
				base.view.SetupDialog(type, showPurchaseButton);
				UpdateAdButton();
				base.gameObject.SetActive(true);
			}
			else
			{
				logger.Debug("Showing rush dialog without require items");
			}
		}

		private void AdVideoButtonClicked()
		{
			if (adPlacementInstance != null)
			{
				rewardedAdService.ShowRewardedVideo(adPlacementInstance);
			}
		}

		private bool RewardedAdEnabledForDialogType()
		{
			if (pendingCurrencyTransaction == null)
			{
				return false;
			}
			TransactionTarget transactionTarget = pendingCurrencyTransaction.GetTransactionTarget();
			TransactionTarget transactionTarget2 = transactionTarget;
			if (transactionTarget2 == TransactionTarget.CLEAR_DEBRIS)
			{
				return true;
			}
			return false;
		}

		private void UpdateAdButton()
		{
			if (RewardedAdEnabledForDialogType())
			{
				bool flag = rewardedAdService.IsPlacementActive(adPlacementName);
				AdPlacementInstance placementInstance = rewardedAdService.GetPlacementInstance(adPlacementName);
				bool flag2 = SkipStorageCheckOnRushTransaction() || !base.playerService.isStorageFull();
				bool adButtonEnabled = flag && flag2 && IsRushCostAcceptableForAd(placementInstance) && placementInstance != null;
				base.view.EnableRewardedAdRushButton(adButtonEnabled);
				adPlacementInstance = placementInstance;
			}
		}

		private bool IsRushCostAcceptableForAd(AdPlacementInstance placement)
		{
			bool result = false;
			if (placement != null)
			{
				MissingResourcesRewardDefinition missingResourcesRewardDefinition = placement.Definition as MissingResourcesRewardDefinition;
				if (missingResourcesRewardDefinition != null && requiredItems.Count != 0)
				{
					int num = base.playerService.CalculateRushCost(requiredItems);
					result = num <= missingResourcesRewardDefinition.MaxCostPremiumCurrency;
				}
			}
			return result;
		}

		private void OnRewardedAdReward(AdPlacementInstance placement)
		{
			if (placement.Equals(adPlacementInstance))
			{
				ResetRushCosts();
				ExecuteRushTransaction();
				rewardedAdService.RewardPlayer(null, placement);
				base.telemetryService.Send_Telemetry_EVT_AD_INTERACTION(placement.Definition.Name, requiredItems, placement.RewardPerPeriodCount);
				adPlacementInstance = null;
			}
		}

		private void ResetRushCosts()
		{
			for (int i = 0; i < requiredItemPremiumCosts.Count; i++)
			{
				requiredItemPremiumCosts[i] = 0;
			}
		}

		private void OnAdPlacementActivityStateChanged(AdPlacementInstance placement, bool enabled)
		{
			UpdateAdButton();
		}
	}
}
