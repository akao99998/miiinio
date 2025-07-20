using System.Collections;
using Elevation.Logging;
using Kampai.Game;
using Kampai.Util;
using strange.extensions.command.impl;

public class RestoreNamedCharacterCommand : Command
{
	public IKampaiLogger logger = LogManager.GetClassLogger("RestoreNamedCharacterCommand") as IKampaiLogger;

	[Inject]
	public NamedCharacter character { get; set; }

	[Inject]
	public Prestige prestige { get; set; }

	[Inject]
	public IRoutineRunner routineRunner { get; set; }

	[Inject]
	public IPlayerService playerService { get; set; }

	[Inject]
	public KevinGoToWelcomeHutSignal gotoWelcomeHutSignal { get; set; }

	[Inject]
	public CreateNamedCharacterViewSignal createSignal { get; set; }

	[Inject]
	public PhilSitAtBarSignal sitAtBarSignal { get; set; }

	[Inject]
	public RestoreMinionAtTikiBarSignal restoreMinionAtTikiBarSignal { get; set; }

	[Inject]
	public RestoreStuartSignal restoreStuartSignal { get; set; }

	[Inject]
	public FrolicSignal frolicSignal { get; set; }

	public override void Execute()
	{
		if (character.Definition.Type != NamedCharacterType.TSM)
		{
			logger.Info("Restoring Character: {0} with prestige state: {1}", character.Name, prestige.state);
			createSignal.Dispatch(character);
			routineRunner.StartCoroutine(WaitAFrame());
		}
	}

	private IEnumerator WaitAFrame()
	{
		yield return null;
		PrestigeState state = prestige.state;
		if (state == PrestigeState.Questing)
		{
			if (character.Definition.Type == NamedCharacterType.PHIL)
			{
				sitAtBarSignal.Dispatch(true);
			}
			else if (character.Definition.Type != NamedCharacterType.KEVIN)
			{
				restoreMinionAtTikiBarSignal.Dispatch(character);
			}
			else
			{
				KevinCharacter kevin = character as KevinCharacter;
				if (kevin != null)
				{
					WelcomeHutBuilding welcomeHutBuilding = playerService.GetByInstanceId<WelcomeHutBuilding>(372);
					if (welcomeHutBuilding.IsBuildingRepaired())
					{
						gotoWelcomeHutSignal.Dispatch(true);
					}
					else
					{
						frolicSignal.Dispatch(kevin.ID);
					}
				}
			}
		}
		if (character.Definition.Type == NamedCharacterType.STUART)
		{
			restoreStuartSignal.Dispatch();
		}
	}
}
