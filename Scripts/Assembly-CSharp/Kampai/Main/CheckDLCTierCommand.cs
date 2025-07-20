using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Main
{
	public class CheckDLCTierCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("CheckDLCTierCommand") as IKampaiLogger;

		[Inject]
		public IManifestService manifestService { get; set; }

		[Inject]
		public IDLCService dlcService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		public override void Execute()
		{
			List<string> list = new List<string>();
			list.AddRange(checkPrestige());
			list.AddRange(checkUnlocks());
			int num = 0;
			foreach (string item in list)
			{
				string assetLocation = manifestService.GetAssetLocation(item);
				if (!string.IsNullOrEmpty(assetLocation))
				{
					int bundleTier = manifestService.GetBundleTier(assetLocation);
					if (bundleTier != int.MaxValue && bundleTier > num)
					{
						num = bundleTier;
					}
				}
			}
			int quantity = (int)playerService.GetQuantity(StaticItem.TIER_ID);
			int quantity2 = (int)playerService.GetQuantity(StaticItem.TIER_GATE_ID);
			if (num > quantity2)
			{
				logger.Log(KampaiLogLevel.Info, "Current Tier Gate " + quantity2 + " Is less than required Tier " + num);
				playerService.AlterQuantity(StaticItem.TIER_GATE_ID, num - quantity2);
			}
			if (num > quantity)
			{
				logger.Log(KampaiLogLevel.Info, "Current Tier " + quantity + " Is less than required Tier " + num);
				playerService.AlterQuantity(StaticItem.TIER_ID, num - quantity);
			}
		}

		private List<string> checkPrestige()
		{
			List<string> list = new List<string>();
			IList<Prestige> instancesByType = playerService.GetInstancesByType<Prestige>();
			foreach (Prestige item2 in instancesByType)
			{
				NamedCharacterDefinition definition;
				if (definitionService.TryGet<NamedCharacterDefinition>(item2.Definition.TrackedDefinitionID, out definition))
				{
					string text = dlcService.GetDownloadQualityLevel().ToUpper();
					if (!string.IsNullOrEmpty(definition.Prefab) && !string.IsNullOrEmpty(text))
					{
						string item = string.Format("{0}_{1}", definition.Prefab, text);
						list.Add(item);
					}
				}
				else
				{
					CostumeItemDefinition definition2;
					if (!definitionService.TryGet<CostumeItemDefinition>(item2.Definition.TrackedDefinitionID, out definition2))
					{
						continue;
					}
					if (string.IsNullOrEmpty(definition2.Skeleton))
					{
						list.Add(definition2.Skeleton);
					}
					if (definition2.MeshList == null || definition2.MeshList.Count <= 0)
					{
						continue;
					}
					foreach (string mesh in definition2.MeshList)
					{
						list.Add(mesh);
					}
				}
			}
			return list;
		}

		private List<string> checkUnlocks()
		{
			List<string> list = new List<string>();
			IList<BuildingDefinition> unlockedDefsByType = playerService.GetUnlockedDefsByType<BuildingDefinition>();
			foreach (BuildingDefinition item in unlockedDefsByType)
			{
				ConnectableBuildingDefinition connectableBuildingDefinition = item as ConnectableBuildingDefinition;
				if (connectableBuildingDefinition != null && connectableBuildingDefinition.piecePrefabs != null)
				{
					for (int i = 0; i < connectableBuildingDefinition.GetNumPrefabs(); i++)
					{
						list.Add(connectableBuildingDefinition.GetPrefab(i));
					}
				}
				else if (!string.IsNullOrEmpty(item.GetPrefab()))
				{
					list.Add(item.GetPrefab());
				}
			}
			return list;
		}
	}
}
