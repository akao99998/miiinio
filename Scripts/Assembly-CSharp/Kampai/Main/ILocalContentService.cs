using System.Collections.Generic;

namespace Kampai.Main
{
	public interface ILocalContentService
	{
		void SetDLCQuality(string levelName);

		bool IsLocalAsset(string name);

		bool IsStreamingAsset(string name);

		string GetAssetPath(string name);

		List<string> GetStreamingAudioBanks();
	}
}
