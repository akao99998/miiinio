using System;
using Kampai.Game;
using Kampai.Util;

namespace Kampai.Tools.AnimationToolKit
{
	public class AnimationToolKitDLCService : IDLCService
	{
		public int GetPlayerDLCTier()
		{
			throw new NotImplementedException();
		}

		public void SetPlayerDLCTier(int tier)
		{
			throw new NotImplementedException();
		}

		public void SetDownloadQualityLevel(TargetPerformance target)
		{
			throw new NotImplementedException();
		}

		public string GetDownloadQualityLevel()
		{
			return TargetPerformance.HIGH.ToString().ToLower();
		}

		public void SetDisplayQualityLevel(string qualityDef)
		{
			throw new NotImplementedException();
		}

		public string GetDisplayQualityLevel()
		{
			throw new NotImplementedException();
		}
	}
}
