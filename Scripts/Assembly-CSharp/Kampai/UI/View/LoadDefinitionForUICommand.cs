using System;
using System.Collections;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.UI.View
{
	public class LoadDefinitionForUICommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("LoadDefinitionForUICommand") as IKampaiLogger;

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public BuildMenuDefinitionLoadedSignal buildMenuLoadedSignal { get; set; }

		[Inject]
		public AddStoreTabSignal addTabSignal { get; set; }

		[Inject]
		public SetLevelSignal setLevelSignal { get; set; }

		[Inject]
		public SetXPSignal setXPSignal { get; set; }

		[Inject]
		public SetGrindCurrencySignal setGrindCurrencySignal { get; set; }

		[Inject]
		public SetPremiumCurrencySignal setPremiumCurrencySignal { get; set; }

		[Inject]
		public SetStorageCapacitySignal setStorageSignal { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ReconcileLevelUnlocksSignal reconcileUnlocks { get; set; }

		[Inject]
		public LoadMTXStoreSignal loadMTXStoreSignal { get; set; }

		[Inject]
		public ICurrencyStoreService currencyStoreService { get; set; }

		public override void Execute()
		{
			logger.EventStart("LoadDefinitionForUICommand.Execute");
			TimeProfiler.StartSection("unlocks");
			reconcileUnlocks.Dispatch();
			TimeProfiler.EndSection("unlocks");
			TimeProfiler.StartSection("load ui defs");
			TimeProfiler.StartSection("parse");
			TimeProfiler.StartSection("load");
			IList<StoreItemDefinition> all = definitionService.GetAll<StoreItemDefinition>();
			playerService.UpdateMinionPartyPointValues();
			TimeProfiler.EndSection("load");
			Dictionary<StoreItemType, List<Definition>> buildMenuItems = new Dictionary<StoreItemType, List<Definition>>();
			foreach (StoreItemDefinition item in all)
			{
				switch (item.Type)
				{
				case StoreItemType.BaseResource:
				case StoreItemType.Crafting:
				case StoreItemType.Decoration:
				case StoreItemType.Leisure:
				case StoreItemType.Special:
				case StoreItemType.SpecialEvent:
				case StoreItemType.MasterPlanLeftOvers:
				case StoreItemType.Connectable:
					AddBuildStoreItem(buildMenuItems, item, item.Type);
					if (item.IsFeatured)
					{
						AddBuildStoreItem(buildMenuItems, item, StoreItemType.Featured);
					}
					break;
				}
			}
			TimeProfiler.EndSection("parse");
			logger.EventStart("LoadDefinitionForUICommand.BuildMenu");
			routineRunner.StartCoroutine(WaitAFrame(buildMenuItems));
			logger.EventStop("LoadDefinitionForUICommand.Execute");
		}

		private void AddBuildStoreItem(Dictionary<StoreItemType, List<Definition>> buildMenuItems, StoreItemDefinition storeItemDef, StoreItemType type)
		{
			if (!buildMenuItems.ContainsKey(type))
			{
				buildMenuItems[type] = new List<Definition>();
			}
			if (storeItemDef.OnlyShowIfInInventory || storeItemDef.OnlyShowIfOwned)
			{
				buildMenuItems[type].Insert(0, storeItemDef);
			}
			else
			{
				buildMenuItems[type].Add(storeItemDef);
			}
		}

		public static string LocaleForType(StoreItemType type)
		{
			string result = string.Empty;
			switch (type)
			{
			case StoreItemType.BaseResource:
				result = "MakeStuff";
				break;
			case StoreItemType.Crafting:
				result = "MixStuff";
				break;
			case StoreItemType.Decoration:
				result = "DecorateStuff";
				break;
			case StoreItemType.Leisure:
				result = "LeisureStuff";
				break;
			case StoreItemType.Special:
				result = "OtherStuff";
				break;
			case StoreItemType.Featured:
				result = "FeaturedStuff";
				break;
			case StoreItemType.SpecialEvent:
				result = "SpecialEventStuff";
				break;
			case StoreItemType.MasterPlanLeftOvers:
				result = "SpecialItems";
				break;
			case StoreItemType.Connectable:
				result = "ConnectableStuff";
				break;
			}
			return result;
		}

		private IEnumerator WaitAFrame(Dictionary<StoreItemType, List<Definition>> buildMenuItems)
		{
			yield return null;
			TimeProfiler.StartSection("signals");
			bool isEventActive = false;
			foreach (SpecialEventItemDefinition specialEvent in definitionService.GetAll<SpecialEventItemDefinition>())
			{
				if (specialEvent.IsActive)
				{
					isEventActive = true;
					break;
				}
			}
			if (isEventActive && buildMenuItems.ContainsKey(StoreItemType.SpecialEvent))
			{
				addTabSignal.Dispatch(new StoreTab(localService.GetString(LocaleForType(StoreItemType.SpecialEvent)), StoreItemType.SpecialEvent));
			}
			foreach (int type in Enum.GetValues(typeof(StoreItemType)))
			{
				if (type != 10 && buildMenuItems.ContainsKey((StoreItemType)type))
				{
					addTabSignal.Dispatch(new StoreTab(localService.GetString(LocaleForType((StoreItemType)type)), (StoreItemType)type));
				}
			}
			if (!isEventActive && buildMenuItems.ContainsKey(StoreItemType.SpecialEvent))
			{
				addTabSignal.Dispatch(new StoreTab(localService.GetString(LocaleForType(StoreItemType.SpecialEvent)), StoreItemType.SpecialEvent));
			}
			buildMenuLoadedSignal.Dispatch(buildMenuItems);
			loadMTXStoreSignal.Dispatch();
			currencyStoreService.Initialize();
			setLevelSignal.Dispatch();
			setXPSignal.Dispatch();
			setGrindCurrencySignal.Dispatch();
			setPremiumCurrencySignal.Dispatch();
			setStorageSignal.Dispatch();
			TimeProfiler.EndSection("signals");
			TimeProfiler.EndSection("load ui defs");
			logger.EventStop("LoadDefinitionForUICommand.BuildMenu");
		}
	}
}
