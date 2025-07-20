using System.Collections.Generic;
using Kampai.Game.View;
using UnityEngine;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class RemoveCharacterFromPartyStageCommand : Command
	{
		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService defService { get; set; }

		[Inject(ContextKeys.CONTEXT_VIEW)]
		public GameObject contextView { get; set; }

		[Inject(GameElement.NAMED_CHARACTER_MANAGER)]
		public GameObject namedCharacterManager { get; set; }

		[Inject(GameElement.MINION_MANAGER)]
		public GameObject minionManager { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		[Inject]
		public SocialEventAvailableSignal socialEventAvailableSignal { get; set; }

		public override void Execute()
		{
			GameObject gameObject = contextView.FindChild("StageCharacters");
			if (gameObject != null)
			{
				Object.Destroy(gameObject);
			}
			StageBuilding firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<StageBuilding>(3054);
			if (firstInstanceByDefinitionId == null || !firstInstanceByDefinitionId.IsBuildingRepaired())
			{
				return;
			}
			MinionParty firstInstanceByDefinitionId2 = playerService.GetFirstInstanceByDefinitionId<MinionParty>(80000);
			List<int> lastGuestsOfHonorPrestigeIDs = firstInstanceByDefinitionId2.lastGuestsOfHonorPrestigeIDs;
			foreach (int item in lastGuestsOfHonorPrestigeIDs)
			{
				if (item == 0)
				{
					continue;
				}
				PrestigeDefinition prestigeDefinition = defService.Get<PrestigeDefinition>(item);
				Prestige firstInstanceByDefinitionId3 = playerService.GetFirstInstanceByDefinitionId<Prestige>(item);
				if (prestigeDefinition != null)
				{
					Character byInstanceId = playerService.GetByInstanceId<Character>(firstInstanceByDefinitionId3.trackedInstanceId);
					CharacterObject characterObject;
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
						characterObject.transform.localScale = new Vector3(1f, 1f, 1f);
					}
				}
			}
			BuildingManagerView component3 = buildingManager.GetComponent<BuildingManagerView>();
			BuildingObject buildingObject = component3.GetBuildingObject(firstInstanceByDefinitionId.ID);
			StageBuildingObject stageBuildingObject = buildingObject as StageBuildingObject;
			if (stageBuildingObject != null)
			{
				stageBuildingObject.SetHideMic(false);
			}
			StuartCharacter firstInstanceByDefinitionId4 = playerService.GetFirstInstanceByDefinitionId<StuartCharacter>(70001);
			if (firstInstanceByDefinitionId4 != null)
			{
				Prestige firstInstanceByDefinitionId5 = playerService.GetFirstInstanceByDefinitionId<Prestige>(40003);
				if (firstInstanceByDefinitionId5.CurrentPrestigeLevel >= 1)
				{
					socialEventAvailableSignal.Dispatch();
				}
			}
		}
	}
}
