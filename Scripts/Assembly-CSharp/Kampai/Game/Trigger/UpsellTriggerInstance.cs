namespace Kampai.Game.Trigger
{
	public class UpsellTriggerInstance : TriggerInstance<UpsellTriggerDefinition>
	{
		public UpsellTriggerInstance(UpsellTriggerDefinition definition)
			: base(definition)
		{
		}

		public override void RewardPlayer(IPlayerService playerService)
		{
		}
	}
}
