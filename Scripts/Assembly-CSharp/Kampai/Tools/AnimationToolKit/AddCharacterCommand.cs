using Kampai.Game;
using Kampai.Game.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Tools.AnimationToolKit
{
	public class AddCharacterCommand : Command
	{
		[Inject]
		public IPlayerService PlayerService { get; set; }

		[Inject]
		public IPrestigeService PrestigeService { get; set; }

		[Inject]
		public BuildingObject BuildingObject { get; set; }

		[Inject]
		public CharacterObject CharacterObject { get; set; }

		[Inject]
		public GetNewQuestSignal GetNewQuestSignal { get; set; }

		[Inject]
		public int RouteIndex { get; set; }

		public override void Execute()
		{
			Building byInstanceId = PlayerService.GetByInstanceId<Building>(BuildingObject.ID);
			StageBuilding stageBuilding = byInstanceId as StageBuilding;
			TikiBarBuilding tikiBarBuilding = byInstanceId as TikiBarBuilding;
			if (stageBuilding == null && tikiBarBuilding == null)
			{
				return;
			}
			StageBuildingObject stageBuildingObject = BuildingObject as StageBuildingObject;
			TikiBarBuildingObjectView tikiBarBuildingObjectView = BuildingObject as TikiBarBuildingObjectView;
			if (stageBuildingObject == null && tikiBarBuildingObjectView == null)
			{
				return;
			}
			TaskingCharacterObject taskingCharacterObject = null;
			taskingCharacterObject = new TaskingCharacterObject(CharacterObject, RouteIndex);
			taskingCharacterObject.RoutingIndex = RouteIndex;
			AnimatingBuildingDefinition animatingBuildingDefinition = null;
			if (stageBuilding != null)
			{
				animatingBuildingDefinition = stageBuilding.Definition;
			}
			else if (tikiBarBuilding != null)
			{
				animatingBuildingDefinition = tikiBarBuilding.Definition;
			}
			RuntimeAnimatorController animController = KampaiResources.Load<RuntimeAnimatorController>(animatingBuildingDefinition.AnimationDefinitions[0].MinionController);
			CharacterObject.SetAnimController(animController);
			TaskableBuilding taskableBuilding = byInstanceId as TaskableBuilding;
			if (taskableBuilding != null)
			{
				taskableBuilding.AddMinion(CharacterObject.ID, 0);
			}
			if (stageBuildingObject != null)
			{
				stageBuildingObject.MoveToRoutingPosition(CharacterObject, RouteIndex);
			}
			else if (tikiBarBuildingObjectView != null)
			{
				PhilView philView = CharacterObject as PhilView;
				if (philView != null)
				{
					tikiBarBuildingObjectView.MoveToRoutingPosition(CharacterObject, RouteIndex);
				}
				else
				{
					tikiBarBuildingObjectView.AddCharacterToBuildingActions(CharacterObject, PlayerService, RouteIndex, PrestigeService, GetNewQuestSignal);
				}
			}
		}
	}
}
