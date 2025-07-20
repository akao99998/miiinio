using Elevation.Logging;
using Kampai.Common;
using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class UpdateVillainPrestigeCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("UpdateVillainPrestigeCommand") as IKampaiLogger;

		[Inject]
		public Prestige prestige { get; set; }

		[Inject]
		public Tuple<PrestigeState, PrestigeState> states { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject(UIElement.CONTEXT)]
		public ICrossContextCapable uiContext { get; set; }

		[Inject]
		public QueueCabanaSignal updateCabanaSignal { get; set; }

		[Inject]
		public MoveInCabanaSignal moveInSignal { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		public override void Execute()
		{
			switch (states.Item2)
			{
			case PrestigeState.Prestige:
			case PrestigeState.Taskable:
				SwitchToPrestige();
				break;
			case PrestigeState.InQueue:
				SwitchToInQueue();
				break;
			case PrestigeState.Questing:
				SwitchToQuesting();
				break;
			}
		}

		private void SwitchToInQueue()
		{
			if (prestige.trackedInstanceId == 0)
			{
				VillainDefinition villainDefinition = definitionService.Get<VillainDefinition>(prestige.Definition.TrackedDefinitionID);
				if (villainDefinition == null)
				{
					logger.Error("Trying to create a villain instance, but this prestige ({0}) doesn't have a villain definition!", prestige);
					return;
				}
				Villain villain = new Villain(villainDefinition);
				playerService.Add(villain);
				prestige.trackedInstanceId = villain.ID;
			}
			else
			{
				Villain villain = playerService.GetByInstanceId<Villain>(prestige.trackedInstanceId);
			}
			telemetryService.Send_TelemetryCharacterPrestiged(prestige);
			uiContext.injectionBinder.GetInstance<ShowBuddyWelcomePanelUISignal>().Dispatch(new Boxed<Prestige>(prestige), CharacterWelcomeState.Welcome, 0);
			updateCabanaSignal.Dispatch(prestige);
		}

		private void SwitchToQuesting()
		{
			moveInSignal.Dispatch(prestige);
		}

		private void SwitchToPrestige()
		{
			if (states.Item1 == PrestigeState.Questing)
			{
				uiContext.injectionBinder.GetInstance<ShowBuddyWelcomePanelUISignal>().Dispatch(new Boxed<Prestige>(prestige), CharacterWelcomeState.Farewell, 0);
			}
		}
	}
}
