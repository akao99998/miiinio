using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Game;
using Kampai.Util;
using strange.extensions.command.impl;

public class EndSpecialEventCommand : Command
{
	public IKampaiLogger logger = LogManager.GetClassLogger("EndSpecialEventCommand") as IKampaiLogger;

	[Inject]
	public SpecialEventItemDefinition specialEventItemDefinition { get; set; }

	[Inject]
	public IPlayerService playerService { get; set; }

	[Inject]
	public IDefinitionService definitionService { get; set; }

	public override void Execute()
	{
		logger.Info("Ending special event {0}", specialEventItemDefinition.LocalizedKey);
		CleanUpSpecialEventCharacter();
	}

	private void CleanUpSpecialEventCharacter()
	{
		ICollection<SpecialEventCharacter> instancesByType = playerService.GetInstancesByType<SpecialEventCharacter>();
		if (instancesByType == null)
		{
			return;
		}
		List<Quest> instancesByType2 = playerService.GetInstancesByType<Quest>();
		List<Quest> list = new List<Quest>();
		if (instancesByType2 != null)
		{
			foreach (Quest item in instancesByType2)
			{
				if (item.Definition.SurfaceID == specialEventItemDefinition.PrestigeDefinitionID)
				{
					list.Add(item);
				}
			}
			foreach (Quest item2 in list)
			{
				playerService.Remove(item2);
			}
		}
		foreach (SpecialEventCharacter item3 in instancesByType)
		{
			if (item3.SpecialEventID == specialEventItemDefinition.ID)
			{
				playerService.Remove(item3);
			}
		}
		GrantCostume();
	}

	private int FindMinionForCostumeGrant()
	{
		int awardCostumeId = specialEventItemDefinition.AwardCostumeId;
		if (awardCostumeId > 0)
		{
			Minion minion = null;
			foreach (Minion item in playerService.GetInstancesByType<Minion>())
			{
				if (!item.HasPrestige)
				{
					minion = item;
					continue;
				}
				Prestige byInstanceId = playerService.GetByInstanceId<Prestige>(item.PrestigeId);
				if (byInstanceId == null || byInstanceId.Definition.ID != specialEventItemDefinition.PrestigeDefinitionID)
				{
					continue;
				}
				logger.Error("Minion {0} already has costume!", item.ID);
				minion = null;
				break;
			}
			if (minion != null)
			{
				logger.Info("Transmuting generic minion {0} to event costume ID {1}", minion.ID, specialEventItemDefinition.AwardCostumeId);
				return minion.ID;
			}
			logger.Error("Unable to satisfy costume grant");
		}
		return -1;
	}

	private void GrantCostume()
	{
		int num = FindMinionForCostumeGrant();
		if (num <= 0)
		{
			return;
		}
		Minion byInstanceId = playerService.GetByInstanceId<Minion>(num);
		if (byInstanceId != null)
		{
			int prestigeDefinitionID = specialEventItemDefinition.PrestigeDefinitionID;
			Prestige prestige = playerService.GetFirstInstanceByDefinitionId<Prestige>(prestigeDefinitionID);
			if (prestige == null)
			{
				PrestigeDefinition def = definitionService.Get<PrestigeDefinition>(prestigeDefinitionID);
				prestige = new Prestige(def);
				playerService.Add(prestige);
			}
			byInstanceId.PrestigeId = prestige.ID;
			prestige.trackedInstanceId = byInstanceId.ID;
			prestige.state = PrestigeState.Taskable;
		}
	}
}
