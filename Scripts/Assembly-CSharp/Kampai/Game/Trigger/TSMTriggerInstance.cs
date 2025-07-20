namespace Kampai.Game.Trigger
{
	public class TSMTriggerInstance : TriggerInstance<TSMTriggerDefinition>
	{
		public TSMTriggerInstance(TSMTriggerDefinition definition)
			: base(definition)
		{
		}

		public override void RewardPlayer(IPlayerService playerService)
		{
		}
	}
}
