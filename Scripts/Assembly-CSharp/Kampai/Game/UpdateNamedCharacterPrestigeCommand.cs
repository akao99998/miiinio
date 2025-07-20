using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class UpdateNamedCharacterPrestigeCommand : Command
	{
		[Inject]
		public Prestige prestige { get; set; }

		[Inject]
		public Tuple<PrestigeState, PrestigeState> states { get; set; }

		[Inject]
		public MinionPrestigeCompleteSignal minionPrestigeCompleteSignal { get; set; }

		[Inject]
		public RemoveMinionFromTikiBarSignal removeMinionFromTikiBarSignal { get; set; }

		[Inject(UIElement.CONTEXT)]
		public ICrossContextCapable uiContext { get; set; }

		public override void Execute()
		{
			switch (states.Item2)
			{
			case PrestigeState.InQueue:
				minionPrestigeCompleteSignal.Dispatch(prestige);
				break;
			case PrestigeState.Taskable:
				SwitchCharacterToTaskable();
				break;
			case PrestigeState.TaskableWhileQuesting:
				removeMinionFromTikiBarSignal.Dispatch(prestige);
				break;
			case PrestigeState.Questing:
				break;
			}
		}

		private void SwitchCharacterToTaskable()
		{
			if (states.Item1 != PrestigeState.TaskableWhileQuesting)
			{
				if (prestige.CurrentPrestigeLevel == 0)
				{
					uiContext.injectionBinder.GetInstance<ShowBuddyWelcomePanelUISignal>().Dispatch(new Boxed<Prestige>(prestige), CharacterWelcomeState.Farewell, 0);
				}
				removeMinionFromTikiBarSignal.Dispatch(prestige);
			}
		}
	}
}
