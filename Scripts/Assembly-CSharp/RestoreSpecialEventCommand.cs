using Elevation.Logging;
using Kampai.Game;
using Kampai.Util;
using strange.extensions.command.impl;

public class RestoreSpecialEventCommand : Command
{
	public IKampaiLogger logger = LogManager.GetClassLogger("RestoreSpecialEventCommand") as IKampaiLogger;

	[Inject]
	public SpecialEventItemDefinition specialEventItemDefinition { get; set; }

	[Inject]
	public LoadSpecialEventPaintoverSignal loadPaintoverSignal { get; set; }

	public override void Execute()
	{
		logger.Info("Restoring special event {0}", specialEventItemDefinition.LocalizedKey);
		loadPaintoverSignal.Dispatch(specialEventItemDefinition);
	}
}
