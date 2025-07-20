namespace Kampai.Game.Trigger
{
	public class TSMTriggerDefinition : TriggerDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1149;
			}
		}

		public override TriggerDefinitionType.Identifier type
		{
			get
			{
				return TriggerDefinitionType.Identifier.TSM;
			}
		}

		public override TriggerInstance Build()
		{
			return new TSMTriggerInstance(this);
		}
	}
}
