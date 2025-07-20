using System.Collections.Generic;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;

namespace Kampai.UI.View
{
	public static class StoreButtonBuilder
	{
		private static readonly string[] Textures = new string[4] { "icn_currency_Grind_fill", "icn_currency_Grind_mask", "icn_currency_premium_fill", "icn_currency_premium_mask" };

		public static StoreButtonView Build(Definition definition, TransactionDefinition transaction, StoreItemDefinition storeItemDefinition, Transform i_parent, ILocalizationService localService, IDefinitionService definitionService, IKampaiLogger logger, IPlayerService playerService)
		{
			if (definition == null)
			{
				logger.Fatal(FatalCode.EX_NULL_ARG);
			}
			GameObject original = KampaiResources.Load("cmp_SubMenuItem") as GameObject;
			GameObject gameObject = Object.Instantiate(original);
			StoreButtonView component = gameObject.GetComponent<StoreButtonView>();
			component.init(playerService);
			component.ItemName.text = localService.GetStringUpper(definition.LocalizedKey);
			component.definition = definition;
			component.transactionDef = transaction;
			component.storeItemDefinition = storeItemDefinition;
			DisplayableDefinition displayableDefinition = definition as DisplayableDefinition;
			if (displayableDefinition != null)
			{
				component.ItemDescription.text = localService.GetString(displayableDefinition.Description);
				component.UpdatePartyPointText(localService);
				if (string.IsNullOrEmpty(displayableDefinition.Mask))
				{
					logger.Log(KampaiLogLevel.Error, "Your Building Definition: {0} doesn' have a mask image defined for the building icon: {1}", displayableDefinition.ID, displayableDefinition.Image);
					displayableDefinition.Mask = "btn_Circle01_mask";
				}
				string key = ((component.storeItemDefinition.Type != StoreItemType.MasterPlanLeftOvers) ? "UnlockAt" : "UnlockForMPLeaveBehind");
				component.UnlockedAtLevel.text = localService.GetString(key, definitionService.GetLevelItemUnlocksAt(displayableDefinition.ID));
			}
			RectTransform rectTransform = gameObject.transform as RectTransform;
			rectTransform.SetParent(i_parent);
			rectTransform.SetAsFirstSibling();
			rectTransform.localPosition = new Vector3(rectTransform.localPosition.x, rectTransform.localPosition.y, 0f);
			rectTransform.localScale = Vector3.one;
			gameObject.SetActive(false);
			return component;
		}

		public static bool DetermineUnlock(StoreButtonView view, IPlayerService playerService, Dictionary<int, int> countMap, IDefinitionService definitionService, IKampaiLogger logger, ITimeService timeService, ILocalizationService localeService, IMasterPlanService masterPlanService, IBuildMenuService buildMenuService)
		{
			bool result = false;
			int iD = view.definition.ID;
			int quantity = (int)playerService.GetQuantity(StaticItem.LEVEL_ID);
			DisplayableDefinition displayableDefinition = view.definition as DisplayableDefinition;
			StoreItemDefinition storeItemDefinition = view.storeItemDefinition;
			int num = playerService.GetUnlockedQuantityOfID(iD);
			bool flag = definitionService.GetLevelItemUnlocksAt(iD) > quantity;
			if (string.IsNullOrEmpty(displayableDefinition.Image))
			{
				logger.Log(KampaiLogLevel.Error, "Your Building Definition: {0} doesn' have a image defined", displayableDefinition.ID);
				displayableDefinition.Image = "btn_Circle01_mask";
			}
			if ((flag && num == 0) || IsMasterPlanItemForIncompletePlan(storeItemDefinition, iD, definitionService, masterPlanService))
			{
				ItemLocked(view);
			}
			else
			{
				if (buildMenuService.ShouldRenderStoreDef(storeItemDefinition))
				{
					Sprite sprite = UIUtils.LoadSpriteFromPath(displayableDefinition.Image);
					Sprite sprite2 = UIUtils.LoadSpriteFromPath(displayableDefinition.Mask);
					if (sprite != null && sprite2 != null)
					{
						KampaiImage itemIcon = view.ItemIcon;
						itemIcon.sprite = sprite;
						itemIcon.maskSprite = sprite2;
						view.DragSpritePath = displayableDefinition.Image;
						view.DragMaskPath = displayableDefinition.Mask;
						view.DragAnimationController = "asm_BuildStore_StoreDragHint";
					}
				}
				CheckBadge(view);
				result = CheckLocked(view);
				result = (num != 0 && result) || (view.CurrentCapacity > 0 && (view.CurrentCapacity < num || num < 0) && !result);
				if (storeItemDefinition.SpecialEventID > 0)
				{
					SpecialEventItemDefinition definition;
					bool flag2 = definitionService.TryGet<SpecialEventItemDefinition>(storeItemDefinition.SpecialEventID, out definition);
					if ((flag2 && !definition.IsActive) || !flag2)
					{
						num = playerService.GetCountByDefinitionId(iD);
					}
				}
				if (!storeItemDefinition.IsOnSale(Application.platform, timeService, localeService, logger))
				{
					num = playerService.GetCountByDefinitionId(iD);
				}
				int buildingCount = UpdateBuildingCount(view, iD, num, countMap);
				int incrementalCost = GetIncrementalCost(view, buildingCount, definitionService, playerService);
				CheckForTransactions(view, incrementalCost);
				view.DisplayOrHideUnlockedCostIcons();
			}
			return result;
		}

		private static int UpdateBuildingCount(StoreButtonView view, int itemDefintionId, int capacity, Dictionary<int, int> countMap)
		{
			view.SetCapacity(capacity);
			int num = 0;
			if (countMap.ContainsKey(itemDefintionId))
			{
				num = countMap[itemDefintionId];
				view.SetBuildingCount(num);
			}
			return num;
		}

		private static void CheckForTransactions(StoreButtonView view, int totalIncrementalCost)
		{
			if (TransactionUtil.IsOnlyPremiumInputs(view.transactionDef))
			{
				view.Cost.text = UIUtils.FormatLargeNumber(TransactionUtil.SumOutputsForStaticItem(view.transactionDef, StaticItem.PREMIUM_CURRENCY_ID, true) + totalIncrementalCost);
				view.MoneyIcon.sprite = UIUtils.LoadSpriteFromPath(Textures[2]);
				view.MoneyIcon.maskSprite = UIUtils.LoadSpriteFromPath(Textures[3]);
			}
			else
			{
				view.Cost.text = UIUtils.FormatLargeNumber(TransactionUtil.SumOutputsForStaticItem(view.transactionDef, StaticItem.GRIND_CURRENCY_ID, true) + totalIncrementalCost);
				view.MoneyIcon.sprite = UIUtils.LoadSpriteFromPath(Textures[0]);
				view.MoneyIcon.maskSprite = UIUtils.LoadSpriteFromPath(Textures[1]);
				view.DisableDoubleConfirm();
			}
		}

		private static bool CheckLocked(StoreButtonView view)
		{
			return view.ChangeStateToUnlocked();
		}

		private static void CheckBadge(StoreButtonView view)
		{
			view.ItemBadge.HideNew();
		}

		private static void ItemLocked(StoreButtonView view)
		{
			view.ChangeStateToLocked();
		}

		private static int GetIncrementalCost(StoreButtonView view, int buildingCount, IDefinitionService definitionService, IPlayerService playerService)
		{
			int incrementalCost = definitionService.GetIncrementalCost(view.definition);
			int num = 0;
			if (incrementalCost > 0)
			{
				num = playerService.GetInventoryCountByDefinitionID(view.definition.ID);
			}
			return (buildingCount + num) * incrementalCost;
		}

		private static bool IsMasterPlanItemForIncompletePlan(StoreItemDefinition storeItemDef, int itemDefinitionId, IDefinitionService definitionService, IMasterPlanService masterPlanService)
		{
			if (storeItemDef.Type != StoreItemType.MasterPlanLeftOvers)
			{
				return false;
			}
			List<MasterPlanDefinition> all = definitionService.GetAll<MasterPlanDefinition>();
			foreach (MasterPlanDefinition item in all)
			{
				if (itemDefinitionId == item.LeavebehindBuildingDefID)
				{
					if (!masterPlanService.HasReceivedInitialRewardFromPlanDefinition(item))
					{
						return true;
					}
					break;
				}
			}
			return false;
		}
	}
}
