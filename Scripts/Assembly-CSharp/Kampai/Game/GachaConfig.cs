using System.Collections.Generic;

namespace Kampai.Game
{
	public class GachaConfig
	{
		public IList<GachaAnimationDefinition> GatchaAnimationDefinitions { get; set; }

		public IList<GachaWeightedDefinition> DistributionTables { get; set; }
	}
}
