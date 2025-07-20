using System.Collections;

namespace Kampai.Common.Service.Audio
{
	public interface IFMODService
	{
		IEnumerator InitializeSystem();

		string GetGuid(string eventName);

		bool LoadFromAssetBundleAsync(string bundleName);

		void StartAsyncBankLoadingProcessing();

		bool BanksLoadingInProgress();
	}
}
