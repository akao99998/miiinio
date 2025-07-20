using Kampai.Util;

namespace Kampai.Game
{
	public class DLCService : IDLCService
	{
		[Inject]
		public ILocalPersistanceService localPersistService { get; set; }

		public int GetPlayerDLCTier()
		{
			return GetPlayerDLCTier(localPersistService);
		}

		public void SetPlayerDLCTier(int tier)
		{
			int playerDLCTier = GetPlayerDLCTier();
			if (tier > playerDLCTier)
			{
				localPersistService.PutDataInt("dlcTier", tier);
			}
		}

		public static int GetPlayerDLCTier(ILocalPersistanceService localService)
		{
			return localService.GetDataInt("dlcTier");
		}

		public void SetDownloadQualityLevel(TargetPerformance target)
		{
			localPersistService.PutData("dlcQuality", target.ToString().ToLower());
		}

		public string GetDownloadQualityLevel()
		{
			string text = localPersistService.GetData("dlcQuality");
			if (string.IsNullOrEmpty(text))
			{
				text = TargetPerformance.LOW.ToString().ToLower();
			}
			return text;
		}

		public void SetDisplayQualityLevel(string qualityDef)
		{
			localPersistService.PutData("DlcDisplayQuality", qualityDef);
		}

		public string GetDisplayQualityLevel()
		{
			return localPersistService.GetData("DlcDisplayQuality");
		}
	}
}
