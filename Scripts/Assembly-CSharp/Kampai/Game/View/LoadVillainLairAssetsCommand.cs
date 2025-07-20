using Elevation.Logging;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game.View
{
	public class LoadVillainLairAssetsCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("LoadVillainLairAssetsCommand") as IKampaiLogger;

		private VillainLairResourcePlotDefinition resourcePlotDefinition;

		private VillainLair currentLair;

		[Inject]
		public int villainLairInstanceId { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public VillainLairModel villainLairModel { get; set; }

		[Inject]
		public VillainLairAssetsLoadedSignal loadingCompleteSignal { get; set; }

		public override void Execute()
		{
			currentLair = playerService.GetByInstanceId<VillainLair>(villainLairInstanceId);
			if (currentLair == null)
			{
				logger.Info("Lair ID {0} is not in player service: aborting load of lair and all instances associated with it.", villainLairInstanceId);
			}
			else
			{
				resourcePlotDefinition = definitionService.Get<VillainLairResourcePlotDefinition>(currentLair.Definition.ResourceBuildingDefID);
				LoadNextPrefab(0);
			}
		}

		public void LoadNextPrefab(int indexIntoPrefabs)
		{
			if (!villainLairModel.areLairAssetsLoaded)
			{
				switch (indexIntoPrefabs)
				{
				case 0:
					KampaiResources.LoadAsync(currentLair.Definition.Prefab, routineRunner, LoadLairPrefab);
					break;
				case 1:
					KampaiResources.LoadAsync(resourcePlotDefinition.brokenPrefab_loaded, routineRunner, LoadLockedResourcesPrefab);
					break;
				case 2:
					KampaiResources.LoadAsync(resourcePlotDefinition.prefab_loaded, routineRunner, LoadUnlockedResourcesPrefab);
					break;
				default:
					logger.Error("Villain lair prefabs aren't fully loaded, but we're attempting to use an invalid index ({0}) into the prefab types", indexIntoPrefabs);
					break;
				}
			}
			else
			{
				LoadingFinished();
			}
		}

		private void LoadingFinished()
		{
			loadingCompleteSignal.Dispatch(true);
		}

		private void LoadUnlockedResourcesPrefab(Object prefabObj)
		{
			VillainLairModel.LairPrefabType lairPrefabType = VillainLairModel.LairPrefabType.UNLOCKED_PLOT;
			villainLairModel.asyncLoadedPrefabs[(int)lairPrefabType] = prefabObj as GameObject;
			LoadNextPrefab((int)(lairPrefabType + 1));
		}

		private void LoadLockedResourcesPrefab(Object prefabObj)
		{
			VillainLairModel.LairPrefabType lairPrefabType = VillainLairModel.LairPrefabType.LOCKED_PLOT;
			villainLairModel.asyncLoadedPrefabs[(int)lairPrefabType] = prefabObj as GameObject;
			LoadNextPrefab((int)(lairPrefabType + 1));
		}

		private void LoadLairPrefab(Object prefabObj)
		{
			VillainLairModel.LairPrefabType lairPrefabType = VillainLairModel.LairPrefabType.LAIR;
			villainLairModel.asyncLoadedPrefabs[(int)lairPrefabType] = prefabObj as GameObject;
			LoadNextPrefab((int)(lairPrefabType + 1));
		}
	}
}
