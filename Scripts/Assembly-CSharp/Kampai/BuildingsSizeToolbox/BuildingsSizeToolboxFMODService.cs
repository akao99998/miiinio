using System.Collections;
using Kampai.Common.Service.Audio;

namespace Kampai.BuildingsSizeToolbox
{
	internal sealed class BuildingsSizeToolboxFMODService : IFMODService
	{
		public IEnumerator InitializeSystem()
		{
			yield break;
		}

		public string GetGuid(string eventName)
		{
			return string.Empty;
		}

		public bool LoadFromAssetBundleAsync(string bundleName)
		{
			return true;
		}

		public void StartAsyncBankLoadingProcessing()
		{
		}

		public bool BanksLoadingInProgress()
		{
			return false;
		}
	}
}
