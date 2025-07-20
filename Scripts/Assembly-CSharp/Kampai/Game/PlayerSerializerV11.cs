using System.Collections.Generic;
using Kampai.Util;

namespace Kampai.Game
{
	internal class PlayerSerializerV11 : PlayerSerializerV10
	{
		public override int Version
		{
			get
			{
				return 11;
			}
		}

		public override Player Deserialize(string json, IDefinitionService definitionService, ILocalPersistanceService localPersistanceService, IPartyService partyService, IKampaiLogger logger)
		{
			Player player = base.Deserialize(json, definitionService, localPersistanceService, partyService, logger);
			if (player.Version < 11)
			{
				CleanUpStuartPrestige(player);
				CleanUpMasterPlanInfo(player, definitionService);
				FixPartyFTUEPopup(player, definitionService);
				player.Version = 11;
			}
			return player;
		}

		private void FixPartyFTUEPopup(Player player, IDefinitionService definitionService)
		{
			Quest firstInstanceByDefinitionId = player.GetFirstInstanceByDefinitionId<Quest>(31050);
			int[] array = new int[2] { 101133, 101222 };
			int[] array2 = new int[7] { 101130, 101201, 101211, 101131, 101220, 101221, 101132 };
			if (firstInstanceByDefinitionId == null)
			{
				return;
			}
			bool flag = false;
			int[] array3 = array;
			foreach (int num in array3)
			{
				Quest firstInstanceByDefinitionId2 = player.GetFirstInstanceByDefinitionId<Quest>(num);
				if (firstInstanceByDefinitionId2 == null)
				{
					QuestDefinition def = definitionService.Get<QuestDefinition>(num);
					firstInstanceByDefinitionId2 = new Quest(def);
					firstInstanceByDefinitionId2.state = QuestState.Complete;
					player.AssignNextInstanceId(firstInstanceByDefinitionId2);
					player.Add(firstInstanceByDefinitionId2);
					flag = true;
				}
			}
			if (!flag)
			{
				return;
			}
			List<Quest> instancesByType = player.GetInstancesByType<Quest>();
			foreach (Quest item in instancesByType)
			{
				int[] array4 = array2;
				foreach (int num2 in array4)
				{
					if (item.Definition.ID == num2)
					{
						player.Remove(item);
					}
				}
			}
		}

		private void CleanUpMasterPlanInfo(Player player, IDefinitionService definitionService)
		{
			MasterPlan firstInstanceByDefinitionId = player.GetFirstInstanceByDefinitionId<MasterPlan>(65000);
			if (firstInstanceByDefinitionId == null)
			{
				return;
			}
			if (player.GetQuantityByDefinitionId(710) == 0)
			{
				player.SetQuantityByStaticItem(StaticItem.MASTER_PLAN_COMPLETION_COUNT, (uint)firstInstanceByDefinitionId.completionCount);
				firstInstanceByDefinitionId.completionCount = 0;
			}
			VillainLair firstInstanceByDefinitionId2 = player.GetFirstInstanceByDefinitionId<VillainLair>(3137);
			if (firstInstanceByDefinitionId2.hasVisited)
			{
				firstInstanceByDefinitionId.introHasBeenDisplayed = true;
			}
			VillainLairDefinition villainLairDefinition = definitionService.Get<VillainLairDefinition>(StaticItem.VILLAIN_LAIR_DEFINITION_ID);
			List<PlatformDefinition> platforms = villainLairDefinition.Platforms;
			List<MasterPlanComponent> instancesByType = player.GetInstancesByType<MasterPlanComponent>();
			for (int i = 0; i < instancesByType.Count; i++)
			{
				MasterPlanComponent masterPlanComponent = instancesByType[i];
				switch (masterPlanComponent.Definition.ID)
				{
				case 66000:
					masterPlanComponent.buildingDefID = 3140;
					masterPlanComponent.buildingLocation = platforms[0].placementLocation;
					break;
				case 66001:
					masterPlanComponent.buildingDefID = 3141;
					masterPlanComponent.buildingLocation = platforms[1].placementLocation;
					break;
				case 66002:
					masterPlanComponent.buildingDefID = 3142;
					masterPlanComponent.buildingLocation = platforms[2].placementLocation;
					break;
				case 66003:
					masterPlanComponent.buildingDefID = 3143;
					masterPlanComponent.buildingLocation = platforms[3].placementLocation;
					break;
				case 66004:
					masterPlanComponent.buildingDefID = 3144;
					masterPlanComponent.buildingLocation = platforms[4].placementLocation;
					break;
				}
				MasterPlanComponentBuilding firstInstanceByDefinitionId3 = player.GetFirstInstanceByDefinitionId<MasterPlanComponentBuilding>(masterPlanComponent.buildingDefID);
				if (firstInstanceByDefinitionId3 != null)
				{
					firstInstanceByDefinitionId3.Location = masterPlanComponent.buildingLocation;
				}
				if (masterPlanComponent.reward == null)
				{
					masterPlanComponent.reward = GenerateComponentReward();
				}
			}
			MasterPlanComponentBuilding firstInstanceByDefinitionId4 = player.GetFirstInstanceByDefinitionId<MasterPlanComponentBuilding>(firstInstanceByDefinitionId.Definition.BuildingDefID);
			if (firstInstanceByDefinitionId4 != null)
			{
				firstInstanceByDefinitionId4.Location = platforms[5].placementLocation;
			}
		}

		private void CleanUpStuartPrestige(Player player)
		{
			IList<Prestige> byDefinitionId = player.GetByDefinitionId<Prestige>(40003);
			if (byDefinitionId != null && byDefinitionId.Count > 0)
			{
				Prestige prestige = byDefinitionId[0];
				int currentPrestigeLevel = prestige.CurrentPrestigeLevel;
				if (currentPrestigeLevel >= 3 && (prestige.state == PrestigeState.Questing || prestige.state == PrestigeState.Taskable) && currentPrestigeLevel++ < prestige.Definition.PrestigeLevelSettings.Count)
				{
					prestige.CurrentPrestigeLevel++;
				}
			}
		}

		private MasterPlanComponentReward GenerateComponentReward()
		{
			MasterPlanComponentReward masterPlanComponentReward = new MasterPlanComponentReward();
			masterPlanComponentReward.Definition = new MasterPlanComponentRewardDefinition();
			masterPlanComponentReward.Definition.grindReward = 7500u;
			masterPlanComponentReward.Definition.premiumReward = 5u;
			masterPlanComponentReward.Definition.rewardItemId = 1;
			masterPlanComponentReward.Definition.rewardQuantity = 5u;
			return masterPlanComponentReward;
		}
	}
}
