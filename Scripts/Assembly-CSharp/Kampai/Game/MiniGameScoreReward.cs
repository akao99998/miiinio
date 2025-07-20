using System.Collections.Generic;

namespace Kampai.Game
{
	public class MiniGameScoreReward
	{
		public int MiniGameId { get; set; }

		public IList<Reward> rewardTable { get; set; }
	}
}
