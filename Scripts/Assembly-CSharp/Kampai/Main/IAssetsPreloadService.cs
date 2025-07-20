namespace Kampai.Main
{
	public interface IAssetsPreloadService
	{
		void AddAssetToPreloadQueue(PreloadableAsset asset);

		void PreloadAllAssets();

		void StopAssetsPreload();

		void SetIntegrationStepLength(int msec);
	}
}
