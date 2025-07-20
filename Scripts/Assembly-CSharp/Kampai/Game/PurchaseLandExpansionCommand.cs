using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Common.Service.Telemetry;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class PurchaseLandExpansionCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("PurchaseLandExpansionCommand") as IKampaiLogger;

		private List<int> generatedDebris;

		private List<int> generatedAspirationalBuildings;

		private CommonLandExpansionDefinition commonDefinition;

		private LandExpansionConfig config;

		private GameObject flamethrowerMinion;

		private Animator flamethrowerAnimator;

		private BobCharacter bob;

		private BuildingManagerView buildingManagerView;

		private List<LandExpansionBuilding> expansionBuildings;

		[Inject]
		public int expansionID { get; set; }

		[Inject]
		public bool playFX { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public IRandomService randomService { get; set; }

		[Inject]
		public ILandExpansionService landExpansionService { get; set; }

		[Inject]
		public ILandExpansionConfigService landExpansionConfigService { get; set; }

		[Inject]
		public BurnLandExpansionSignal burnLandSignal { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public CreateInventoryBuildingSignal createInventoryBuildingSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public CreateForSaleSignSignal createForSaleSignSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal audioSignal { get; set; }

		[Inject]
		public PlaceBuildingSignal placeBuilding { get; set; }

		[Inject]
		public RemoveBuildingSignal removeFootprintSignal { get; set; }

		[Inject]
		public BobCelebrateLandExpansionSignal celebrateLandExpansionSignal { get; set; }

		[Inject]
		public BobCelebrateLandExpansionCompleteSignal celebrateLandExpansionCompleteSignal { get; set; }

		[Inject]
		public PointBobLandExpansionSignal pointBobLandExpansionSignal { get; set; }

		[Inject]
		public IPrestigeService prestigeService { get; set; }

		[Inject]
		public RemoveWayFinderSignal removeWayFinderSignal { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		public override void Execute()
		{
			config = landExpansionConfigService.GetExpansionConfig(expansionID);
			buildingManagerView = buildingManager.GetComponent<BuildingManagerView>();
			expansionBuildings = landExpansionService.GetBuildingsByExpansionID(expansionID) as List<LandExpansionBuilding>;
			if (expansionBuildings == null || expansionBuildings.Count == 0)
			{
				logger.Error("No expansion buildings for {0}", expansionID);
				return;
			}
			bob = playerService.GetFirstInstanceByDefinitionId<BobCharacter>(70002);
			if (bob != null && prestigeService.GetPrestigeFromMinionInstance(bob).state != PrestigeState.Questing)
			{
				removeWayFinderSignal.Dispatch(bob.ID);
			}
			int quantity = (int)playerService.GetQuantity(StaticItem.LEVEL_ID);
			foreach (int adjacentExpansionId in config.adjacentExpansionIds)
			{
				if (!landExpansionService.IsLevelGated(adjacentExpansionId, quantity))
				{
					createForSaleSignSignal.Dispatch(adjacentExpansionId, true);
				}
			}
			generatedDebris = new List<int>();
			generatedAspirationalBuildings = new List<int>();
			commonDefinition = definitionService.Get<CommonLandExpansionDefinition>();
			if (commonDefinition == null)
			{
				logger.Log(KampaiLogLevel.Error, "Unable to find the common expansion definition.");
			}
			GenerateDebris();
			HandleAspirationalBuildings();
			HandleBob();
		}

		private void HandleAspirationalBuildings()
		{
			foreach (int containedAspirationalBuilding in config.containedAspirationalBuildings)
			{
				Building aspirationalBuilding = landExpansionService.GetAspirationalBuilding(containedAspirationalBuilding);
				if (aspirationalBuilding != null && playerService.GetFirstInstanceByDefinitionId<Building>(aspirationalBuilding.Definition.ID) == null)
				{
					playerService.Add(aspirationalBuilding);
					generatedAspirationalBuildings.Add(containedAspirationalBuilding);
					aspirationalBuilding.SetState(BuildingState.Idle);
				}
			}
		}

		private void HandleBob()
		{
			if (playerService.GetTargetExpansion() == expansionID)
			{
				celebrateLandExpansionCompleteSignal.AddListener(BobCelebrationComplete);
				celebrateLandExpansionSignal.Dispatch();
				return;
			}
			if (bob != null && generatedAspirationalBuildings.Count == 0)
			{
				pointBobLandExpansionSignal.Dispatch();
			}
			routineRunner.StartCoroutine(BurnGrass());
		}

		public void BobCelebrationComplete()
		{
			celebrateLandExpansionCompleteSignal.RemoveListener(BobCelebrationComplete);
			pointBobLandExpansionSignal.Dispatch();
			routineRunner.StartCoroutine(BurnGrass());
		}

		private void GenerateDebris()
		{
			foreach (int containedDebri in config.containedDebris)
			{
				Building debris = landExpansionService.GetDebris(containedDebri);
				playerService.Add(debris);
				generatedDebris.Add(containedDebri);
				debris.SetState(BuildingState.Idle);
			}
		}

		private IEnumerator BurnGrass()
		{
			if (expansionBuildings.Count == 0)
			{
				yield break;
			}
			int startBuildingIndex = randomService.NextInt(expansionBuildings.Count);
			LandExpansionBuilding startBuilding = expansionBuildings[startBuildingIndex];
			List<LandExpansionBuilding> burningBuildings = new List<LandExpansionBuilding>(expansionBuildings.Count);
			List<LandExpansionBuilding> freshBurnList = new List<LandExpansionBuilding>(expansionBuildings.Count);
			audioSignal.Dispatch("Play_grass_clear_01");
			BurnBuilding(startBuilding, buildingManagerView);
			burningBuildings.Add(startBuilding);
			yield return new WaitForSeconds(0.25f);
			landExpansionService.RemoveForSaleSign(expansionID);
			while (burningBuildings.Count < expansionBuildings.Count)
			{
				foreach (LandExpansionBuilding burningBuilding in burningBuildings)
				{
					foreach (LandExpansionBuilding building in expansionBuildings)
					{
						if (!burningBuildings.Contains(building) && !freshBurnList.Contains(building) && IsAdjacentBuilding(building, burningBuilding))
						{
							BurnBuilding(building, buildingManagerView);
							freshBurnList.Add(building);
						}
					}
				}
				burningBuildings.AddRange(freshBurnList);
				freshBurnList.Clear();
				yield return new WaitForSeconds(0.25f);
			}
			UpdateDebrisState();
			UpdateAspirationalBuildingState();
			string landExpansionName = new StringBuilder().Append("LandExpansion").Append(config.ID).ToString();
			telemetryService.Send_Telemetry_EVT_GP_ACHIEVEMENTS_CHECKPOINTS_EAL(landExpansionName, TelemetryAchievementType.LandExpansion, 0, string.Empty);
		}

		internal void BurnBuilding(LandExpansionBuilding building, BuildingManagerView buildingManagerView)
		{
			removeFootprintSignal.Dispatch(building.Location, definitionService.GetBuildingFootprint(building.Definition.FootprintID));
			LandExpansionBuildingObject landExpansionBuildingObject = buildingManagerView.GetBuildingObject(building.ID) as LandExpansionBuildingObject;
			if (!(landExpansionBuildingObject == null))
			{
				if (playFX && landExpansionBuildingObject != null)
				{
					landExpansionBuildingObject.Burn(burnLandSignal, building.ID, commonDefinition.VFXGrassClearing);
				}
				else
				{
					burnLandSignal.Dispatch(building.ID);
				}
			}
		}

		internal bool IsAdjacentBuilding(LandExpansionBuilding building, LandExpansionBuilding burningBuilding)
		{
			if (Math.Abs(building.Location.x - burningBuilding.Location.x) <= 3 && Math.Abs(building.Location.y - burningBuilding.Location.y) <= 3)
			{
				return true;
			}
			return false;
		}

		internal void UpdateDebrisState()
		{
			BuildingManagerView component = buildingManager.GetComponent<BuildingManagerView>();
			foreach (int generatedDebri in generatedDebris)
			{
				Building debris = landExpansionService.GetDebris(generatedDebri);
				DebrisDefinition debrisDefinition = definitionService.Get<DebrisDefinition>(generatedDebri);
				component.CleanupBuilding(-debrisDefinition.ID);
				createInventoryBuildingSignal.Dispatch(debris, debris.Location);
				placeBuilding.Dispatch(debris.ID, debris.Location);
			}
		}

		internal void UpdateAspirationalBuildingState()
		{
			BuildingManagerView component = buildingManager.GetComponent<BuildingManagerView>();
			foreach (int generatedAspirationalBuilding in generatedAspirationalBuildings)
			{
				Building aspirationalBuilding = landExpansionService.GetAspirationalBuilding(generatedAspirationalBuilding);
				AspirationalBuildingDefinition aspirationalBuildingDefinition = definitionService.Get<AspirationalBuildingDefinition>(generatedAspirationalBuilding);
				component.CleanupBuilding(-aspirationalBuildingDefinition.ID);
				createInventoryBuildingSignal.Dispatch(aspirationalBuilding, aspirationalBuilding.Location);
			}
		}
	}
}
