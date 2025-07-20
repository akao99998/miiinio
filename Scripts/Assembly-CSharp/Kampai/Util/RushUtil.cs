using System.Collections.Generic;
using Kampai.Game;

namespace Kampai.Util
{
	public static class RushUtil
	{
		public static void SortByTime(IList<RushTimeBandDefinition> items, bool ascending = true)
		{
			(items as List<RushTimeBandDefinition>).Sort((RushTimeBandDefinition p1, RushTimeBandDefinition p2) => (!ascending) ? (p2.TimeRemainingInSeconds - p1.TimeRemainingInSeconds) : (p1.TimeRemainingInSeconds - p2.TimeRemainingInSeconds));
		}
	}
}
