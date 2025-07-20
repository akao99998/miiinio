using System;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.UI
{
	public class CurrencyStoreService : ICurrencyStoreService
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("CurrencyStoreService") as IKampaiLogger;

		private CurrencyStoreLocalState localState;

		private Dictionary<int, List<int>> badgeIgnoreList = new Dictionary<int, List<int>>();

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public ILocalPersistanceService localPersistanceService { get; set; }

		[Inject]
		public ICoppaService coppaService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public ILocalizationService localeService { get; set; }

		[Inject]
		public IQuestService questService { get; set; }

		[PostConstruct]
		public void PostConstruct()
		{
			LoadPersist();
		}

		public void Initialize()
		{
			IList<CurrencyStoreCategoryDefinition> currencyStoreCategoryDefinitions = definitionService.GetCurrencyStoreCategoryDefinitions();
			for (int i = 0; i < currencyStoreCategoryDefinitions.Count; i++)
			{
				CurrencyStoreCategoryDefinition currencyStoreCategoryDefinition = currencyStoreCategoryDefinitions[i];
				int iD = currencyStoreCategoryDefinition.ID;
				for (int j = 0; j < currencyStoreCategoryDefinition.StoreItemDefinitionIDs.Count; j++)
				{
					int num = currencyStoreCategoryDefinition.StoreItemDefinitionIDs[j];
					StoreItemDefinition definition;
					if (!definitionService.TryGet<StoreItemDefinition>(num, out definition))
					{
						logger.Warning("Unable to find store item def with id: {0}", num);
					}
					else if (!definition.EnableBadging)
					{
						if (badgeIgnoreList.ContainsKey(iD))
						{
							badgeIgnoreList[iD].Add(num);
							continue;
						}
						List<int> list = new List<int>();
						list.Add(num);
						badgeIgnoreList.Add(iD, list);
					}
				}
			}
		}

		public bool IsValidCurrencyItem(int storeItemDefinitionID, StoreCategoryType type, bool countInLocked = true)
		{
			StoreItemDefinition definition;
			if (!definitionService.TryGet<StoreItemDefinition>(storeItemDefinitionID, out definition))
			{
				logger.Warning("Unable to find store item def with id: {0}", storeItemDefinitionID);
				return false;
			}
			if (!definition.IsOnSale(Application.platform, timeService, localeService, logger))
			{
				logger.Warning("Item not valid for current store: {0}", storeItemDefinitionID);
				return false;
			}
			CurrencyItemDefinition definition2;
			if (!definitionService.TryGet<CurrencyItemDefinition>(definition.ReferencedDefID, out definition2))
			{
				logger.Warning("Unable to find currency item def with id: {0}", definition.ReferencedDefID);
				return false;
			}
			if (definition2.COPPAGated && coppaService.Restricted())
			{
				return false;
			}
			if (definition.Type == StoreItemType.SalePack)
			{
				CurrencyStorePackDefinition definition3;
				if (!definitionService.TryGet<CurrencyStorePackDefinition>(definition.ReferencedDefID, out definition3))
				{
					logger.Error("Unable to find CurrencyStorePackDefinition with id: {0}", definition.ReferencedDefID);
					return false;
				}
				if (PackUtil.HasPurchasedEnough(definition3, playerService))
				{
					return false;
				}
				if (!countInLocked && playerService.GetHighestFtueCompleted() < definition3.StoreUnlockFTUELevel)
				{
					return false;
				}
			}
			return true;
		}

		public bool ShouldPackBeVisuallyLocked(CurrencyStorePackDefinition currencyStorePackDefinition)
		{
			if (playerService.GetHighestFtueCompleted() < currencyStorePackDefinition.StoreUnlockFTUELevel)
			{
				return true;
			}
			if (playerService.GetQuantity(StaticItem.LEVEL_ID) < currencyStorePackDefinition.UnlockLevel)
			{
				return true;
			}
			if (currencyStorePackDefinition.UnlockQuestId != 0 && !questService.IsQuestCompleted(currencyStorePackDefinition.UnlockQuestId))
			{
				return true;
			}
			return false;
		}

		private bool ShouldItemBeMarkedAsViewed(StoreItemDefinition storeItemDef)
		{
			if (storeItemDef.Type != StoreItemType.SalePack)
			{
				return true;
			}
			CurrencyStorePackDefinition definition;
			return definitionService.TryGet<CurrencyStorePackDefinition>(storeItemDef.ReferencedDefID, out definition) && !ShouldPackBeVisuallyLocked(definition);
		}

		public bool HasPurchasedEnough(CurrencyStorePackDefinition currencyStorePackDefinition)
		{
			return PackUtil.HasPurchasedEnough(currencyStorePackDefinition, playerService);
		}

		public CurrencyStorePackDefinition GetCurrencyStorePackDefinition(int packDefinitionId)
		{
			CurrencyStorePackDefinition definition;
			if (definitionService.TryGet<CurrencyStorePackDefinition>(packDefinitionId, out definition))
			{
				return definition;
			}
			logger.Error("The Store Pack you are trying to find doesn't exist, id: {0}", packDefinitionId);
			return null;
		}

		public int GetBadgeCount(CurrencyStoreCategoryDefinition currencyStoreCategoryDef)
		{
			int num = 0;
			int iD = currencyStoreCategoryDef.ID;
			for (int i = 0; i < currencyStoreCategoryDef.StoreItemDefinitionIDs.Count; i++)
			{
				int num2 = currencyStoreCategoryDef.StoreItemDefinitionIDs[i];
				if ((!badgeIgnoreList.ContainsKey(iD) || !badgeIgnoreList[iD].Contains(num2)) && (!localState.ItemsViewedMap.ContainsKey(iD) || !localState.ItemsViewedMap[iD].Contains(num2)) && IsValidCurrencyItem(num2, currencyStoreCategoryDef.StoreCategoryType, false) && ShouldItemBeMarkedAsViewed(definitionService.Get<StoreItemDefinition>(num2)))
				{
					num++;
				}
			}
			return num;
		}

		public void MarkCategoryAsViewed(int categoryDefinitionID)
		{
			CurrencyStoreCategoryDefinition currencyStoreCategoryDef = definitionService.Get<CurrencyStoreCategoryDefinition>(categoryDefinitionID);
			MarkCategoryAsViewed(currencyStoreCategoryDef);
		}

		public void MarkCategoryAsViewed(CurrencyStoreCategoryDefinition currencyStoreCategoryDef)
		{
			int iD = currencyStoreCategoryDef.ID;
			for (int i = 0; i < currencyStoreCategoryDef.StoreItemDefinitionIDs.Count; i++)
			{
				int num = currencyStoreCategoryDef.StoreItemDefinitionIDs[i];
				if ((!badgeIgnoreList.ContainsKey(iD) || !badgeIgnoreList[iD].Contains(num)) && (!localState.ItemsViewedMap.ContainsKey(iD) || !localState.ItemsViewedMap[iD].Contains(num)) && IsValidCurrencyItem(num, currencyStoreCategoryDef.StoreCategoryType, false) && ShouldItemBeMarkedAsViewed(definitionService.Get<StoreItemDefinition>(num)))
				{
					if (!localState.ItemsViewedMap.ContainsKey(iD))
					{
						List<int> list = new List<int>();
						list.Add(num);
						localState.ItemsViewedMap.Add(iD, list);
					}
					else
					{
						localState.ItemsViewedMap[iD].Add(num);
					}
				}
			}
			PersistLocalState();
		}

		private void PersistLocalState()
		{
			if (localState != null)
			{
				try
				{
					string data = JsonConvert.SerializeObject(localState);
					localPersistanceService.PutDataPlayer("CurrencyStoreLocalSave", data);
					return;
				}
				catch (JsonSerializationException ex)
				{
					logger.Error("PersistLocalState(): Json Parse Err: {0}", ex);
					return;
				}
			}
			localPersistanceService.DeleteKeyPlayer("CurrencyStoreLocalSave");
		}

		private void LoadPersist()
		{
			if (localPersistanceService.HasKeyPlayer("CurrencyStoreLocalSave"))
			{
				string dataPlayer = localPersistanceService.GetDataPlayer("CurrencyStoreLocalSave");
				if (dataPlayer != null)
				{
					try
					{
						localState = JsonConvert.DeserializeObject<CurrencyStoreLocalState>(dataPlayer);
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
				localState = new CurrencyStoreLocalState();
			}
		}

		private void HandleJsonException(Exception e)
		{
			logger.Error("CurrencyStoreLocalState.LoadFromPersistence(): Json Parse Err: {0}", e);
			localState = new CurrencyStoreLocalState();
		}
	}
}
