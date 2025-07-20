using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class QuestStepRushCommand : Command
	{
		[Inject]
		public Tuple<int, int> tuple { get; set; }

		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public SetGrindCurrencySignal setGrindCurrencySignal { get; set; }

		[Inject]
		public SetPremiumCurrencySignal setPremiumCurrencySignal { get; set; }

		public override void Execute()
		{
			int item = tuple.Item1;
			int item2 = tuple.Item2;
			setGrindCurrencySignal.Dispatch();
			setPremiumCurrencySignal.Dispatch();
			questService.RushQuestStep(item, item2);
		}
	}
}
