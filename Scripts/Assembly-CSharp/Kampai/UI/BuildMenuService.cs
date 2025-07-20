using System;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Game;
using Kampai.Main;
using Kampai.UI.View;
using Kampai.Util;
using Newtonsoft.Json;

namespace Kampai.UI
{
	public class BuildMenuService : IBuildMenuService
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("BuildMenuService") as IKampaiLogger;

		private BuildMenuLocalState localState;

		private Dictionary<int, int> storeItemDefinitionMap;

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public ILocalPersistanceService localPersistanceService { get; set; }

		[Inject]
		public IMasterPlanService masterPlanService { get; set; }

		[Inject]
		public SetBadgeForStoreTabSignal setBadgeForTabSignal { get; set; }

		[Inject]
		public SetNewUnlockForStoreTabSignal setNewUnlockForTabSignal { get; set; }

		[Inject]
		public SetNewUnlockForBuildMenuSignal setNewUnlockForBuildMenuSignal { get; set; }

		[Inject]
		public SetInventoryCountForBuildMenuSignal setInventoryCountForBuildMenuSignal { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public ILocalizationService localeService { get; set; }

		[Inject]
		public UpdateUIButtonsSignal updateStoreButtonsSignal { get; set; }

		[Inject]
		public IncreaseInventoryCountForBuildMenuSignal increaseInventoryCountSignal { get; set; }

		[PostConstruct]
		public void PostConstruct()
		{
			LoadPersist();
		}

		public void RetoreBuidMenuState(Dictionary<StoreItemType, List<StoreButtonView>> buttonViews)
		{
			UpdateNewUnlockList(buttonViews);
			if (localState.UncheckedInventoryItemOnTabs.Count <= 0)
			{
				return;
			}
			int num = 0;
			foreach (KeyValuePair<StoreItemType, IDictionary<int, bool>> uncheckedInventoryItemOnTab in localState.UncheckedInventoryItemOnTabs)
			{
				int num2 = 0;
				foreach (KeyValuePair<int, bool> item in uncheckedInventoryItemOnTab.Value)
				{
					if (!item.Value)
					{
						num2++;
					}
				}
				if (num2 > 0)
				{
					setBadgeForTabSignal.Dispatch(uncheckedInventoryItemOnTab.Key, num2);
					num += num2;
				}
			}
			if (num > 0)
			{
				setInventoryCountForBuildMenuSignal.Dispatch(num);
			}
		}

		public void SetStoreUnlockChecked()
		{
			List<Tuple<StoreItemType, int>> list = new List<Tuple<StoreItemType, int>>();
			foreach (KeyValuePair<StoreItemType, IDictionary<int, bool>> uncheckedInventoryItemOnTab in localState.UncheckedInventoryItemOnTabs)
			{
				foreach (KeyValuePair<int, bool> item in uncheckedInventoryItemOnTab.Value)
				{
					list.Add(new Tuple<StoreItemType, int>(uncheckedInventoryItemOnTab.Key, item.Key));
				}
			}
			foreach (Tuple<StoreItemType, int> item2 in list)
			{
				localState.UncheckedInventoryItemOnTabs[item2.Item1][item2.Item2] = true;
			}
			PersistLocalState();
		}

		public void AddNewUnlockedItem(StoreItemType type, int buildingDefinitionID)
		{
			if (localState.NewUnlockedItemOnTabs.ContainsKey(type))
			{
				if (!localState.NewUnlockedItemOnTabs[type].Contains(buildingDefinitionID))
				{
					localState.NewUnlockedItemOnTabs[type].Add(buildingDefinitionID);
				}
				else
				{
					logger.Warning("New unlock list already contains this item {0}", buildingDefinitionID);
				}
			}
			else
			{
				List<int> list = new List<int>();
				list.Add(buildingDefinitionID);
				localState.NewUnlockedItemOnTabs.Add(type, list);
			}
			PersistLocalState();
		}

		public bool RemoveNewUnlockedItem(StoreItemType type, int buildingDefinitionID)
		{
			bool result = false;
			if (localState.NewUnlockedItemOnTabs.ContainsKey(type) && localState.NewUnlockedItemOnTabs[type].Contains(buildingDefinitionID))
			{
				localState.NewUnlockedItemOnTabs[type].Remove(buildingDefinitionID);
				if (localState.NewUnlockedItemOnTabs[type].Count == 0)
				{
					localState.NewUnlockedItemOnTabs.Remove(type);
					result = true;
					if (localState.UncheckedTabs.Contains(type))
					{
						setNewUnlockForTabSignal.Dispatch(type, 0);
						localState.UncheckedTabs.Remove(type);
					}
				}
				PersistLocalState();
			}
			return result;
		}

		public void ClearAllNewUnlockItems()
		{
			localState.UncheckedTabs.Clear();
			localState.NewUnlockedItemOnTabs.Clear();
		}

		public bool ShouldRenderStoreDef(StoreItemDefinition storeDef)
		{
			bool flag = true;
			int unlockedQuantityOfID = playerService.GetUnlockedQuantityOfID(storeDef.ReferencedDefID);
			if (storeDef.OnlyShowIfUnlocked)
			{
				flag = unlockedQuantityOfID > 0;
			}
			ICollection<Building> byDefinitionId = playerService.GetByDefinitionId<Building>(storeDef.ReferencedDefID);
			int count = byDefinitionId.Count;
			if (storeDef.OnlyShowIfOwned)
			{
				flag = count > 0;
			}
			if (storeDef.OnlyShowIfInInventory)
			{
				flag = false;
				foreach (Building item in byDefinitionId)
				{
					if (item.State == BuildingState.Inventory)
					{
						flag = true;
					}
				}
			}
			if (storeDef.SpecialEventID > 0 && flag)
			{
				SpecialEventItemDefinition definition;
				bool flag2 = definitionService.TryGet<SpecialEventItemDefinition>(storeDef.SpecialEventID, out definition);
				if ((flag2 && !definition.IsActive) || !flag2)
				{
					flag = count > 0;
				}
			}
			return flag;
		}

		public bool ShowingAChild(List<StoreButtonView> children, bool notifyShouldBeRendered = true)
		{
			if (children == null)
			{
				return false;
			}
			bool flag = false;
			foreach (StoreButtonView child in children)
			{
				if (child.storeItemDefinition.OnlyShowIfInInventory || child.storeItemDefinition.OnlyShowIfOwned || child.storeItemDefinition.OnlyShowIfUnlocked || child.storeItemDefinition.SpecialEventID > 0)
				{
					bool flag2 = ShouldRenderStoreDef(child.storeItemDefinition);
					flag = flag || flag2;
					if (notifyShouldBeRendered)
					{
						child.SetShouldBerendered(flag2);
					}
				}
				else
				{
					flag = true;
				}
			}
			return flag;
		}

		public void UpdateNewUnlockList(Dictionary<StoreItemType, List<StoreButtonView>> buttonViews, bool updateBuildMenuButton = true, bool updateBadge = true)
		{
			Dictionary<int, int> buildingOnBoardCountMap = playerService.GetBuildingOnBoardCountMap();
			int num = 0;
			foreach (KeyValuePair<StoreItemType, List<StoreButtonView>> buttonView in buttonViews)
			{
				int num2 = 0;
				int num3 = 0;
				int num4 = 0;
				StoreItemType key = buttonView.Key;
				if (!ShowingAChild(buttonView.Value, false) && key == StoreItemType.SpecialEvent)
				{
					continue;
				}
				foreach (StoreButtonView item in buttonView.Value)
				{
					if (key == StoreItemType.Featured)
					{
						continue;
					}
					num3++;
					if (StoreButtonBuilder.DetermineUnlock(item, playerService, buildingOnBoardCountMap, definitionService, logger, timeService, localeService, masterPlanService, this))
					{
						if (!localState.UncheckedTabs.Contains(key))
						{
							localState.UncheckedTabs.Add(key);
						}
						AddNewUnlockedItem(key, item.definition.ID);
						num4++;
						num++;
						item.SetNewUnlockState(true);
					}
					else if (localState.NewUnlockedItemOnTabs.ContainsKey(key) && localState.NewUnlockedItemOnTabs[key].Contains(item.definition.ID))
					{
						item.SetNewUnlockState(true);
						num4++;
						num++;
					}
					item.SetShouldBerendered(true);
					bool flag = item.IsUnlocked();
					if (flag)
					{
						num2++;
					}
					if (!flag)
					{
						item.ItemIcon.gameObject.SetActive(false);
					}
					if (num3 - num2 > 4 && !flag && item.storeItemDefinition.Type != StoreItemType.MasterPlanLeftOvers)
					{
						item.SetShouldBerendered(false);
					}
				}
				if (updateBadge)
				{
					setNewUnlockForTabSignal.Dispatch(buttonView.Key, num4);
				}
			}
			if (updateBadge && updateBuildMenuButton && num > 0)
			{
				setNewUnlockForBuildMenuSignal.Dispatch(num);
			}
		}

		public void AddUncheckedInventoryItem(StoreItemType type, int buildingDefinitionID)
		{
			if (localState.UncheckedInventoryItemOnTabs.ContainsKey(type))
			{
				if (!localState.UncheckedInventoryItemOnTabs[type].ContainsKey(buildingDefinitionID))
				{
					localState.UncheckedInventoryItemOnTabs[type][buildingDefinitionID] = false;
				}
				else
				{
					logger.Warning("Unchecked list already contains this item {0}", buildingDefinitionID);
				}
			}
			else
			{
				localState.UncheckedInventoryItemOnTabs[type] = new Dictionary<int, bool>();
				localState.UncheckedInventoryItemOnTabs[type][buildingDefinitionID] = false;
			}
			setBadgeForTabSignal.Dispatch(type, localState.UncheckedInventoryItemOnTabs[type].Count);
			PersistLocalState();
		}

		public void RemoveUncheckedInventoryItem(StoreItemType type, int buildingDefinitionID)
		{
			if (localState.UncheckedInventoryItemOnTabs.ContainsKey(type))
			{
				if (localState.UncheckedInventoryItemOnTabs[type].ContainsKey(buildingDefinitionID))
				{
					localState.UncheckedInventoryItemOnTabs[type].Remove(buildingDefinitionID);
					if (localState.UncheckedInventoryItemOnTabs[type].Count == 0)
					{
						localState.UncheckedInventoryItemOnTabs.Remove(type);
					}
				}
				else
				{
					logger.Warning("Unchecked list doesn't contain this item {0}", buildingDefinitionID);
				}
				PersistLocalState();
			}
			else
			{
				logger.Warning("Unchecked list doesn't contain this type {0}", type);
			}
		}

		public void ClearTab(StoreItemType type)
		{
			localState.NewUnlockedItemOnTabs.Remove(type);
			if (localState.UncheckedInventoryItemOnTabs.ContainsKey(type))
			{
				localState.UncheckedInventoryItemOnTabs.Remove(type);
			}
			if (localState.UncheckedTabs.Contains(type))
			{
				localState.UncheckedTabs.Remove(type);
			}
			PersistLocalState();
		}

		public int GetStoreItemDefinitionIDFromBuildingID(int buildingID)
		{
			if (storeItemDefinitionMap == null)
			{
				storeItemDefinitionMap = new Dictionary<int, int>();
				IList<StoreItemDefinition> all = definitionService.GetAll<StoreItemDefinition>();
				foreach (StoreItemDefinition item in all)
				{
					if (item.ReferencedDefID != 0)
					{
						storeItemDefinitionMap[item.ReferencedDefID] = item.ID;
					}
				}
			}
			if (storeItemDefinitionMap.ContainsKey(buildingID))
			{
				return storeItemDefinitionMap[buildingID];
			}
			return 0;
		}

		public void CompleteBuildMenuUpdate(BuildingType.BuildingTypeIdentifier buildingDefType, int buildingDefinitionID)
		{
			switch (buildingDefType)
			{
			case BuildingType.BuildingTypeIdentifier.RESOURCE:
				AddUncheckedInventoryItem(StoreItemType.BaseResource, buildingDefinitionID);
				increaseInventoryCountSignal.Dispatch();
				break;
			case BuildingType.BuildingTypeIdentifier.LEISURE:
				AddUncheckedInventoryItem(StoreItemType.Leisure, buildingDefinitionID);
				increaseInventoryCountSignal.Dispatch();
				break;
			case BuildingType.BuildingTypeIdentifier.CRAFTING:
				AddUncheckedInventoryItem(StoreItemType.Crafting, buildingDefinitionID);
				increaseInventoryCountSignal.Dispatch();
				break;
			case BuildingType.BuildingTypeIdentifier.DECORATION:
				AddUncheckedInventoryItem(StoreItemType.Decoration, buildingDefinitionID);
				increaseInventoryCountSignal.Dispatch();
				break;
			case BuildingType.BuildingTypeIdentifier.MASTER_LEFTOVER:
				AddUncheckedInventoryItem(StoreItemType.MasterPlanLeftOvers, buildingDefinitionID);
				updateStoreButtonsSignal.Dispatch(false);
				increaseInventoryCountSignal.Dispatch();
				break;
			}
		}

		private void PersistLocalState()
		{
			if (localState != null)
			{
				try
				{
					string data = JsonConvert.SerializeObject(localState);
					localPersistanceService.PutDataPlayer("BuildMenuLocalSave", data);
					return;
				}
				catch (JsonSerializationException ex)
				{
					logger.Error("PersistLocalState(): Json Parse Err: {0}", ex);
					return;
				}
			}
			localPersistanceService.DeleteKeyPlayer("BuildMenuLocalSave");
		}

		private void LoadPersist()
		{
			if (localPersistanceService.HasKeyPlayer("BuildMenuLocalSave"))
			{
				string dataPlayer = localPersistanceService.GetDataPlayer("BuildMenuLocalSave");
				if (dataPlayer != null)
				{
					try
					{
						localState = JsonConvert.DeserializeObject<BuildMenuLocalState>(dataPlayer);
					}
					catch (JsonSerializationException e)
					{
						HandleJsonException(e);
					}
					catch (JsonReaderException e2)
					{
						HandleJsonException(e2);
					}
				}
			}
			if (localState == null)
			{
				localState = new BuildMenuLocalState();
			}
		}

		private void HandleJsonException(Exception e)
		{
			logger.Error("BuildMenuService.LoadFromPersistence(): Json Parse Err: {0}", e);
		}
	}
}
