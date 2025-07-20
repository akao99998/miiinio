namespace Kampai.Game.View
{
	public class TaskingMinionObject
	{
		private MinionObject minionObject;

		public bool IsResting;

		public int RoutingIndex;

		public int ID
		{
			get
			{
				return minionObject.ID;
			}
		}

		public MinionObject Minion
		{
			get
			{
				return minionObject;
			}
		}

		public TaskingMinionObject(MinionObject delegateMinionObject, int routingIndex)
		{
			minionObject = delegateMinionObject;
			RoutingIndex = routingIndex;
		}
	}
}
