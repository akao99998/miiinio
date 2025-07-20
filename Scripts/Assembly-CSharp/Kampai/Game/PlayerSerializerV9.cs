using System.Collections.Generic;
using Kampai.Util;

namespace Kampai.Game
{
	internal class PlayerSerializerV9 : PlayerSerializerV8
	{
		public override int Version
		{
			get
			{
				return 9;
			}
		}

		public override Player Deserialize(string json, IDefinitionService definitionService, ILocalPersistanceService localPersistanceService, IPartyService partyService, IKampaiLogger logger)
		{
			Player player = base.Deserialize(json, definitionService, localPersistanceService, partyService, logger);
			if (player.Version < 9)
			{
				if (localPersistanceService.HasKey("freezeTime"))
				{
					localPersistanceService.PutDataInt("freezeTime", 0);
					localPersistanceService.DeleteKey("freezeTime");
				}
				AwardKevin(player, definitionService, logger);
				AwardV9Buildings(player, definitionService, logger);
				CompositeBuilding byInstanceId = player.GetByInstanceId<CompositeBuilding>(371);
				byInstanceId.Location = new Location(152, 174);
				CleanupOrderBoard(player, logger);
				CleanupFlux(player, logger);
				CleanupOldSales(player, logger);
				RepairMinionPrestige(player, definitionService, logger);
				player.Version = 9;
			}
			return player;
		}

		private void AwardKevin(Player player, IDefinitionService definitionService, IKampaiLogger logger)
		{
			if (player.GetQuantity(StaticItem.LEVEL_ID) >= 10 && player.GetFirstInstanceByDefinitionId<Character>(70003) == null)
			{
				logger.Log(KampaiLogLevel.Info, "Awarding Kevin to the Player");
				PrestigeDefinition prestigeDefinition = definitionService.Get<PrestigeDefinition>(40004);
				KevinCharacterDefinition def = definitionService.Get<NamedCharacterDefinition>(prestigeDefinition.TrackedDefinitionID) as KevinCharacterDefinition;
				KevinCharacter kevinCharacter = new KevinCharacter(def);
				player.AssignNextInstanceId(kevinCharacter);
				player.Add(kevinCharacter);
				Prestige prestige = player.GetFirstInstanceByDefinitionId<Prestige>(40004);
				if (prestige == null)
				{
					prestige = new Prestige(prestigeDefinition);
					player.AssignNextInstanceId(prestige);
					player.Add(prestige);
				}
				prestige.state = PrestigeState.Questing;
				prestige.trackedInstanceId = kevinCharacter.ID;
			}
			else
			{
				if (player.GetFirstInstanceByDefinitionId<Character>(70003) == null)
				{
					return;
				}
				int num = 0;
				TikiBarBuilding byInstanceId = player.GetByInstanceId<TikiBarBuilding>(313);
				for (int i = 1; i < byInstanceId.minionQueue.Count; i++)
				{
					if (byInstanceId.minionQueue[i] == 40004)
					{
						num = i;
						break;
					}
				}
				if (num > 0)
				{
					byInstanceId.minionQueue[num] = -1;
				}
				Prestige firstInstanceByDefinitionId = player.GetFirstInstanceByDefinitionId<Prestige>(40004);
				if (firstInstanceByDefinitionId != null)
				{
					firstInstanceByDefinitionId.state = PrestigeState.Questing;
				}
			}
		}

		private void AwardV9Buildings(Player player, IDefinitionService definitionService, IKampaiLogger logger)
		{
			MinionUpgradeBuilding firstInstanceByDefinitionId = player.GetFirstInstanceByDefinitionId<MinionUpgradeBuilding>(3133);
			if (firstInstanceByDefinitionId == null)
			{
				logger.Log(KampaiLogLevel.Info, "Awarding the Minion Upgrade Building to the Player");
				MinionUpgradeBuildingDefinition definition = null;
				if (definitionService.TryGet<MinionUpgradeBuildingDefinition>(3133, out definition))
				{
					firstInstanceByDefinitionId = definition.Build() as MinionUpgradeBuilding;
					firstInstanceByDefinitionId.State = BuildingState.Inaccessible;
					firstInstanceByDefinitionId.Location = new Location(108, 163);
					firstInstanceByDefinitionId.ID = 375;
					player.Add(firstInstanceByDefinitionId);
				}
			}
			VillainLairEntranceBuilding firstInstanceByDefinitionId2 = player.GetFirstInstanceByDefinitionId<VillainLairEntranceBuilding>(3132);
			if (firstInstanceByDefinitionId2 == null)
			{
				logger.Log(KampaiLogLevel.Info, "Awarding the Villain Lair Entrance Building to the Player");
				VillainLairEntranceBuildingDefinition definition2 = null;
				if (definitionService.TryGet<VillainLairEntranceBuildingDefinition>(3132, out definition2))
				{
					firstInstanceByDefinitionId2 = definition2.Build() as VillainLairEntranceBuilding;
					firstInstanceByDefinitionId2.State = BuildingState.Inaccessible;
					firstInstanceByDefinitionId2.Location = new Location(157, 148);
					firstInstanceByDefinitionId2.ID = 374;
					player.Add(firstInstanceByDefinitionId2);
				}
			}
		}

		private void CleanupOrderBoard(Player player, IKampaiLogger logger)
		{
			OrderBoard firstInstanceByDefinitionId = player.GetFirstInstanceByDefinitionId<OrderBoard>(3022);
			if (firstInstanceByDefinitionId.tickets != null)
			{
				foreach (OrderBoardTicket ticket in firstInstanceByDefinitionId.tickets)
				{
					if (ticket.CharacterDefinitionId == 40004 || ticket.CharacterDefinitionId == 40001)
					{
						logger.Log(KampaiLogLevel.Info, "Changing a Ticket to a Nnon-Character one");
						ticket.CharacterDefinitionId = 0;
					}
				}
			}
			if (firstInstanceByDefinitionId.PriorityPrestigeDefinitionIDs != null)
			{
				if (firstInstanceByDefinitionId.PriorityPrestigeDefinitionIDs.Contains(40004))
				{
					firstInstanceByDefinitionId.PriorityPrestigeDefinitionIDs.Remove(40004);
				}
				if (firstInstanceByDefinitionId.PriorityPrestigeDefinitionIDs.Contains(40001))
				{
					firstInstanceByDefinitionId.PriorityPrestigeDefinitionIDs.Remove(40001);
				}
			}
		}

		private void CleanupFlux(Player player, IKampaiLogger logger)
		{
			foreach (Instance item in player.GetInstancesByDefinition<PrestigeDefinition>())
			{
				Prestige prestige = item as Prestige;
				if (prestige != null && prestige.Definition.Type == PrestigeType.Villain)
				{
					Villain byInstanceId = player.GetByInstanceId<Villain>(prestige.trackedInstanceId);
					if (byInstanceId != null && byInstanceId.Definition.ID == 70004)
					{
						prestige.state = PrestigeState.Locked;
						byInstanceId.CabanaBuildingId = 0;
						logger.Log(KampaiLogLevel.Info, "Removing Villain: " + byInstanceId.Name);
						player.Remove(byInstanceId);
					}
				}
			}
		}

		private void CleanupOldSales(Player player, IKampaiLogger logger)
		{
			List<Sale> instancesByType = player.GetInstancesByType<Sale>();
			List<Sale> list = new List<Sale>();
			foreach (Sale item in instancesByType)
			{
				if (item.Purchased)
				{
					player.AddPurchasedUpsells(item.Definition.ID);
				}
				if (item.Definition == null || (item.Definition.Type != SalePackType.Upsell && item.Definition.Type != SalePackType.Redeemable))
				{
					logger.Log(KampaiLogLevel.Info, "Removing Sale: {0}", item.Definition.ID);
					list.Add(item);
				}
			}
			foreach (Sale item2 in list)
			{
				player.Remove(item2);
			}
		}

		private void RepairMinionPrestige(Player player, IDefinitionService definitionService, IKampaiLogger logger)
		{
			foreach (Instance item in player.GetInstancesByDefinition<PrestigeDefinition>())
			{
				Prestige prestige = item as Prestige;
				if (prestige != null)
				{
					Minion byInstanceId = player.GetByInstanceId<Minion>(prestige.trackedInstanceId);
					if (byInstanceId != null)
					{
						byInstanceId.PrestigeId = prestige.ID;
					}
				}
			}
			SpecialEventItem firstInstanceByDefinitionId = player.GetFirstInstanceByDefinitionId<SpecialEventItem>(110000);
			if (firstInstanceByDefinitionId == null || firstInstanceByDefinitionId.Definition == null || !(firstInstanceByDefinitionId.Definition is SpecialEventItemDefinition))
			{
				return;
			}
			SpecialEventItemDefinition specialEventItemDefinition = firstInstanceByDefinitionId.Definition as SpecialEventItemDefinition;
			Minion minion = null;
			Prestige prestige2 = null;
			foreach (Minion item2 in player.GetInstancesByType<Minion>())
			{
				if (!item2.HasPrestige && minion == null)
				{
					minion = item2;
					continue;
				}
				prestige2 = player.GetFirstInstanceByDefinitionId<Prestige>(specialEventItemDefinition.PrestigeDefinitionID);
				if (prestige2 != null && prestige2.Definition.CostumeDefinitionID == specialEventItemDefinition.AwardCostumeId)
				{
					logger.Error("Winter minion already exists {0}", item2.ID);
					minion = null;
					break;
				}
			}
			if (minion == null)
			{
				return;
			}
			logger.Info("Transmuting generic minion {0} to event costume ID {1}", minion.ID, specialEventItemDefinition.AwardCostumeId);
			prestige2 = player.GetFirstInstanceByDefinitionId<Prestige>(specialEventItemDefinition.PrestigeDefinitionID);
			if (prestige2 == null)
			{
				PrestigeDefinition prestigeDefinition = definitionService.Get<PrestigeDefinition>(specialEventItemDefinition.PrestigeDefinitionID);
				if (prestigeDefinition != null)
				{
					prestige2 = new Prestige(prestigeDefinition);
					player.AssignNextInstanceId(prestige2);
					minion.PrestigeId = prestige2.ID;
					prestige2.trackedInstanceId = minion.ID;
					prestige2.state = PrestigeState.Taskable;
					player.Add(prestige2);
				}
			}
			else
			{
				Minion byInstanceId2 = player.GetByInstanceId<Minion>(prestige2.trackedInstanceId);
				if (byInstanceId2 == null)
				{
					minion.PrestigeId = prestige2.ID;
					prestige2.trackedInstanceId = minion.ID;
				}
				else
				{
					byInstanceId2.PrestigeId = prestige2.ID;
				}
				prestige2.state = PrestigeState.Taskable;
			}
		}
	}
}
