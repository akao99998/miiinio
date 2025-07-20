using System.Collections.Generic;

namespace Kampai.Game.Trigger
{
	public class TriggerRewardLayout
	{
		public enum Layout
		{
			None = 0,
			Horizontal = 1,
			Vertical = 2
		}

		public int index;

		public IList<int> itemIds;

		public Layout layout;
	}
}
