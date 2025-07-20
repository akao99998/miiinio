using System.Collections.Generic;
using Kampai.Game.View;
using Kampai.UI;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class AddCharacterToPartyStageCommand : Command
	{
		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService defService { get; set; }

		[Inject(GameElement.NAMED_CHARACTER_MANAGER)]
		public GameObject namedCharacterManager { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		[Inject(ContextKeys.CONTEXT_VIEW)]
		public GameObject contextView { get; set; }

		[Inject(GameElement.MINION_MANAGER)]
		public GameObject minionManager { get; set; }

		[Inject]
		public IFancyUIService uiService { get; set; }

		private void HandleStuartOnStage()
		{
			List<StuartCharacter> instancesByType = playerService.GetInstancesByType<StuartCharacter>();
			if (instancesByType != null && instancesByType.Count > 0)
			{
				NamedCharacterManagerView component = namedCharacterManager.GetComponent<NamedCharacterManagerView>();
				StuartView stuartView = (StuartView)component.Get(instancesByType[0].ID);
				if (stuartView.IsOnStage())
				{
					stuartView.GetOnStageImmediate(false);
				}
			}
		}

		public override void Execute()
		{
			GameObject gameObject = new GameObject("StageCharacters");
			gameObject.transform.parent = contextView.transform;
			StageBuilding firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<StageBuilding>(3054);
			if (firstInstanceByDefinitionId == null || !firstInstanceByDefinitionId.IsBuildingRepaired())
			{
				return;
			}
			HandleStuartOnStage();
			MinionParty firstInstanceByDefinitionId2 = playerService.GetFirstInstanceByDefinitionId<MinionParty>(80000);
			List<int> lastGuestsOfHonorPrestigeIDs = firstInstanceByDefinitionId2.lastGuestsOfHonorPrestigeIDs;
			foreach (int item in lastGuestsOfHonorPrestigeIDs)
			{
				if (item <= 0)
				{
					continue;
				}
				PrestigeDefinition prestigeDefinition = defService.Get<PrestigeDefinition>(item);
				Prestige firstInstanceByDefinitionId3 = playerService.GetFirstInstanceByDefinitionId<Prestige>(item);
				if (prestigeDefinition != null)
				{
					Character byInstanceId = playerService.GetByInstanceId<Character>(firstInstanceByDefinitionId3.trackedInstanceId);
					CharacterObject characterObject = null;
					if (byInstanceId is NamedCharacter)
					{
						NamedCharacterManagerView component = namedCharacterManager.GetComponent<NamedCharacterManagerView>();
						characterObject = component.Get(byInstanceId.ID);
					}
					else
					{
						MinionManagerView component2 = minionManager.GetComponent<MinionManagerView>();
						characterObject = component2.Get(byInstanceId.ID);
					}
					if (characterObject != null)
					{
						characterObject.transform.localScale = new Vector3(0f, 0f, 0f);
					}
				}
				DummyCharacterType characterType = uiService.GetCharacterType(item);
				DummyCharacterObject dummyCharacterObject = uiService.CreateCharacter(characterType, DummyCharacterAnimationState.Happy, gameObject.transform, new Vector3(1f, 1f, 1f), new Vector3(0f, 0f, 0f), item);
				GuestOfHonorDefinition guestOfHonorDefinition = defService.Get<GuestOfHonorDefinition>(prestigeDefinition.GuestOfHonorDefinitionID);
				if (guestOfHonorDefinition != null && guestOfHonorDefinition.gohAnimationID > 0)
				{
					MinionAnimationDefinition minionAnimationDefinition = defService.Get<MinionAnimationDefinition>(guestOfHonorDefinition.gohAnimationID);
					RuntimeAnimatorController animController = KampaiResources.Load<RuntimeAnimatorController>(minionAnimationDefinition.StateMachine);
					dummyCharacterObject.SetAnimController(animController);
				}
				dummyCharacterObject.gameObject.SetLayerRecursively(0);
				moveCharacterToStage(dummyCharacterObject, firstInstanceByDefinitionId);
				break;
			}
		}

		private void moveCharacterToStage(DummyCharacterObject characterObject, StageBuilding stage)
		{
			BuildingManagerView component = buildingManager.GetComponent<BuildingManagerView>();
			BuildingObject buildingObject = component.GetBuildingObject(stage.ID);
			StageBuildingObject stageBuildingObject = buildingObject as StageBuildingObject;
			if (stageBuildingObject != null)
			{
				Transform stageTransform = stageBuildingObject.GetStageTransform();
				float num = 0.35f;
				Vector3 position = new Vector3(stageTransform.position.x, stageTransform.position.y + num, stageTransform.position.z);
				characterObject.transform.position = position;
				characterObject.transform.rotation = stageTransform.localRotation;
				stageBuildingObject.SetHideMic(true);
			}
		}
	}
}
