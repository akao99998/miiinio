using Kampai.Util;

namespace Kampai.Game
{
	public interface IDLCService
	{
		int GetPlayerDLCTier();

		void SetPlayerDLCTier(int tier);

		void SetDownloadQualityLevel(TargetPerformance target);

		string GetDownloadQualityLevel();

		void SetDisplayQualityLevel(string qualityDef);

		string GetDisplayQualityLevel();
	}
}
