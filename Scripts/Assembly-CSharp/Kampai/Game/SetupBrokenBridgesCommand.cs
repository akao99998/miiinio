using System.Collections.Generic;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class SetupBrokenBridgesCommand : Command
	{
		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public CreateInventoryBuildingSignal createInventroyBuildingSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public BuildingChangeStateSignal changeState { get; set; }

		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public RepairBridgeSignal repairSignal { get; set; }

		[Inject]
		public Environment environment { get; set; }

		[Inject]
		public DebugUpdateGridSignal gridSignal { get; set; }

		public override void Execute()
		{
			IList<BridgeDefinition> all = definitionService.GetAll<BridgeDefinition>();
			foreach (BridgeDefinition item in all)
			{
				Building building = environment.GetBuilding(item.location.x, item.location.y);
				BridgeBuilding bridgeBuilding = building as BridgeBuilding;
				if (building == null)
				{
					BuildingDefinition buildingDefinition = definitionService.Get(item.BuildingId) as BuildingDefinition;
					bridgeBuilding = buildingDefinition.Build() as BridgeBuilding;
					bridgeBuilding.BridgeId = item.ID;
					bridgeBuilding.Location = new Location(item.location.x, item.location.y);
					Vector3 pos = new Vector3(item.location.x, 0f, item.location.y);
					Location type = new Location(pos);
					playerService.Add(bridgeBuilding);
					createInventroyBuildingSignal.Dispatch(bridgeBuilding, type);
					changeState.Dispatch(bridgeBuilding.ID, BuildingState.Idle);
				}
				else if (bridgeBuilding.BridgeId != 0 && questService.IsBridgeQuestComplete(bridgeBuilding.BridgeId))
				{
					repairSignal.Dispatch(bridgeBuilding);
				}
				if (bridgeBuilding != null)
				{
					bridgeBuilding.UnlockLevel = GetBridgeUnlockLevel(item);
				}
			}
			gridSignal.Dispatch();
		}

		private int GetBridgeUnlockLevel(BridgeDefinition bridgeDef)
		{
			IList<QuestDefinition> all = definitionService.GetAll<QuestDefinition>();
			foreach (QuestDefinition item in all)
			{
				if (item.SurfaceType != QuestSurfaceType.Character)
				{
					continue;
				}
				for (int i = 0; i < item.QuestSteps.Count; i++)
				{
					if (item.QuestSteps[i].ItemDefinitionID == bridgeDef.ID && item.QuestSteps[i].Type == QuestStepType.BridgeRepair)
					{
						return item.UnlockLevel;
					}
				}
			}
			return 0;
		}
	}
}
