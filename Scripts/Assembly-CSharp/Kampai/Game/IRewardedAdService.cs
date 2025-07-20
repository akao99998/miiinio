using Kampai.Game.Transaction;

namespace Kampai.Game
{
	public interface IRewardedAdService
	{
		void Initialize();

		bool IsRewardedVideoAvailable();

		bool IsPlacementAvailable(AdPlacementName placementName);

		bool IsPlacementActive(AdPlacementName placementName, int instanceId = 0);

		AdPlacementInstance GetPlacementInstance(AdPlacementName placementName, int instanceId = 0);

		void ShowRewardedVideo(AdPlacementInstance instance, TransactionDefinition reward = null);

		void RewardPlayer(TransactionDefinition reward, AdPlacementInstance placementInstance);
	}
}
