namespace Kampai.Game.Trigger
{
	public class UpsellTriggerDefinition : TriggerDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1151;
			}
		}

		public override TriggerDefinitionType.Identifier type
		{
			get
			{
				return TriggerDefinitionType.Identifier.Upsell;
			}
		}

		public override TriggerInstance Build()
		{
			return new UpsellTriggerInstance(this);
		}
	}
}
