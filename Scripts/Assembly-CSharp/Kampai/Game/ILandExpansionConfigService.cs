using System.Collections.Generic;

namespace Kampai.Game
{
	public interface ILandExpansionConfigService
	{
		IList<int> GetExpansionIds();

		LandExpansionConfig GetExpansionConfig(int expansion);
	}
}
