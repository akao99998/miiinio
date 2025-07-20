using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class CancelTSMQuestTaskCommand : Command
	{
		[Inject]
		public Quest Quest { get; set; }

		[Inject]
		public UpdateTSMQuestTaskSignal UpdateTSMQuestTaskSignal { get; set; }

		public override void Execute()
		{
			UpdateTSMQuestTaskSignal.Dispatch(Quest, false);
		}
	}
}
