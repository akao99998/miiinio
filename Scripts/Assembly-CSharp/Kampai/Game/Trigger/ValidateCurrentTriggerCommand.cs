using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.Game.Trigger
{
	public class ValidateCurrentTriggerCommand : Command
	{
		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public ITriggerService triggerService { get; set; }

		[Inject]
		public IPlayerDurationService playerDurationService { get; set; }

		[Inject]
		public CheckTriggersSignal checkTriggersSignal { get; set; }

		public override void Execute()
		{
			TriggerInstance activeTrigger = triggerService.ActiveTrigger;
			if (activeTrigger != null && activeTrigger.StartGameTime == 0 && !activeTrigger.IsTriggered(gameContext))
			{
				activeTrigger.StartGameTime = playerDurationService.TotalGamePlaySeconds;
				checkTriggersSignal.Dispatch(301);
			}
		}
	}
}
