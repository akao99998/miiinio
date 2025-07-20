using Elevation.Logging;
using Kampai.Game;
using Kampai.Util;
using strange.extensions.command.impl;

public class InitializeSpecialEventCommand : Command
{
	public IKampaiLogger logger = LogManager.GetClassLogger("InitializeSpecialEventCommand") as IKampaiLogger;

	[Inject]
	public IPlayerService playerService { get; set; }

	[Inject]
	public IDefinitionService definitionService { get; set; }

	[Inject]
	public StartSpecialEventSignal startSpecialEventSignal { get; set; }

	[Inject]
	public RestoreSpecialEventSignal restoreSpecialEventSignal { get; set; }

	[Inject]
	public EndSpecialEventSignal endSpecialEventSignal { get; set; }

	public override void Execute()
	{
		foreach (SpecialEventItemDefinition item in definitionService.GetAll<SpecialEventItemDefinition>())
		{
			if (!ValidateEvent(item))
			{
				break;
			}
			SpecialEventItem firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<SpecialEventItem>(item.ID);
			logger.Info("Resolving special event {0}:{1}", item.LocalizedKey, firstInstanceByDefinitionId);
			if (item.IsActive)
			{
				if (firstInstanceByDefinitionId != null)
				{
					if (firstInstanceByDefinitionId.HasEnded)
					{
						logger.Error("Event {0} slated to start, but has already ended!", item.ID);
						break;
					}
					restoreSpecialEventSignal.Dispatch(item);
				}
				else
				{
					playerService.Add(item.Build());
					startSpecialEventSignal.Dispatch(item);
				}
			}
			else if (firstInstanceByDefinitionId != null && !firstInstanceByDefinitionId.HasEnded)
			{
				firstInstanceByDefinitionId.HasEnded = true;
				endSpecialEventSignal.Dispatch(item);
			}
		}
	}

	private bool ValidateEvent(SpecialEventItemDefinition specialEvent)
	{
		return ValidateCostume(FatalCode.SE_INVALID_COSTUME, specialEvent.AwardCostumeId) && ValidateCostume(FatalCode.SE_INVALID_COSTUME, specialEvent.EventMinionCostumeId);
	}

	private bool ValidateCostume(FatalCode code, int costumeId)
	{
		if (costumeId > 0)
		{
			CostumeItemDefinition definition = null;
			if (!definitionService.TryGet<CostumeItemDefinition>(costumeId, out definition))
			{
				logger.Fatal(code, costumeId);
				return false;
			}
		}
		return true;
	}
}
