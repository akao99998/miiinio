using strange.extensions.context.api;

namespace Kampai.Game.Trigger
{
	public class TSMMessageTriggerRewardDefinition : TriggerRewardDefinition
	{
		public override int TypeCode
		{
			get
			{
				return 1185;
			}
		}

		public override TriggerRewardType.Identifier type
		{
			get
			{
				return TriggerRewardType.Identifier.TSMMesssage;
			}
		}

		public override bool IsUniquePerInstance
		{
			get
			{
				return false;
			}
		}

		public override void RewardPlayer(ICrossContextCapable context)
		{
		}
	}
}
