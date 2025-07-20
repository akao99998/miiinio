using Elevation.Logging;
using Kampai.Game;
using Kampai.Util;
using strange.extensions.command.impl;

public class StartSpecialEventCommand : Command
{
	public IKampaiLogger logger = LogManager.GetClassLogger("StartSpecialEventCommand") as IKampaiLogger;

	[Inject]
	public SpecialEventItemDefinition specialEventItemDefinition { get; set; }

	[Inject]
	public RestoreSpecialEventSignal restoreSpecialEventSignal { get; set; }

	[Inject]
	public IDefinitionService definitionService { get; set; }

	[Inject]
	public IPlayerService playerService { get; set; }

	public override void Execute()
	{
		logger.Info("Starting special event {0}", specialEventItemDefinition.LocalizedKey);
		SpecialEventCharacterDefinition specialEventCharacterDefinition = definitionService.Get<SpecialEventCharacterDefinition>(70009);
		if (specialEventCharacterDefinition == null)
		{
			logger.Error("Failed to find Special Event Character Definition");
			return;
		}
		CostumeItemDefinition costumeItemDefinition = definitionService.Get<CostumeItemDefinition>(specialEventItemDefinition.EventMinionCostumeId);
		if (costumeItemDefinition == null)
		{
			logger.Error("Failed to find Special Event Character Costume for CostumeID:" + specialEventItemDefinition.EventMinionCostumeId);
		}
		SpecialEventCharacter specialEventCharacter = new SpecialEventCharacter(specialEventCharacterDefinition);
		specialEventCharacter.SpecialEventID = specialEventItemDefinition.ID;
		specialEventCharacter.PrestigeDefinitionID = specialEventItemDefinition.PrestigeDefinitionID;
		playerService.Add(specialEventCharacter);
		restoreSpecialEventSignal.Dispatch(specialEventItemDefinition);
	}
}
