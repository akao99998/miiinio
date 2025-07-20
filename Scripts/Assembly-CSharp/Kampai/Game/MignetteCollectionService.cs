using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Game.Mignette;
using Kampai.Game.Transaction;
using Kampai.Util;

namespace Kampai.Game
{
	public class MignetteCollectionService
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("MignetteCollectionService") as IKampaiLogger;

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public MignetteGameModel mignetteGameModel { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		public TransactionDefinition pendingRewardTransaction { get; set; }

		public RewardCollection GetCollectionForActiveMignette()
		{
			return GetActiveCollectionForMignette(mignetteGameModel.BuildingId);
		}

		public RewardCollection GetActiveCollectionForMignette(int mignetteBuildingId, bool persistCreatedCollection = true)
		{
			MignetteBuilding byInstanceId = playerService.GetByInstanceId<MignetteBuilding>(mignetteBuildingId);
			return GetActiveCollectionForMignette(byInstanceId, persistCreatedCollection);
		}

		public RewardCollection GetActiveCollectionForMignette(MignetteBuilding building, bool persistCreatedCollection = true)
		{
			RewardCollection orCreateActiveCollection = getOrCreateActiveCollection(building.StartedMainCollectionIDs, building.Definition.MainCollectionDefinitionIDs, persistCreatedCollection, true);
			if (orCreateActiveCollection != null)
			{
				return orCreateActiveCollection;
			}
			orCreateActiveCollection = getOrCreateActiveCollection(building.StartedRepeatableCollectionIDs, building.Definition.RepeatableCollectionDefinitionIDs, persistCreatedCollection, false);
			if (orCreateActiveCollection != null)
			{
				return orCreateActiveCollection;
			}
			RewardCollection rewardCollection = null;
			foreach (int startedRepeatableCollectionID in building.StartedRepeatableCollectionIDs)
			{
				orCreateActiveCollection = playerService.GetByInstanceId<RewardCollection>(startedRepeatableCollectionID);
				orCreateActiveCollection.CollectionScorePreReset = orCreateActiveCollection.CollectionScoreProgress;
				orCreateActiveCollection.ResetProgress();
				if (rewardCollection == null)
				{
					rewardCollection = orCreateActiveCollection;
				}
			}
			return rewardCollection;
		}

		private RewardCollection getOrCreateActiveCollection(IList<int> startedCollectionIDs, IList<int> collectionDefinitionIDs, bool persistCreatedCollection, bool isMain)
		{
			HashSet<int> hashSet = new HashSet<int>();
			foreach (int startedCollectionID in startedCollectionIDs)
			{
				RewardCollection byInstanceId = playerService.GetByInstanceId<RewardCollection>(startedCollectionID);
				if (!byInstanceId.IsCompleted())
				{
					bool flag = false;
					if (isMain)
					{
						flag = checkAndCompleteFinalRewardEarned(byInstanceId);
					}
					if (!flag)
					{
						return byInstanceId;
					}
				}
				hashSet.Add(byInstanceId.Definition.ID);
			}
			if (startedCollectionIDs.Count < collectionDefinitionIDs.Count)
			{
				foreach (int collectionDefinitionID in collectionDefinitionIDs)
				{
					if (!hashSet.Contains(collectionDefinitionID))
					{
						RewardCollectionDefinition definition = definitionService.Get<RewardCollectionDefinition>(collectionDefinitionID);
						RewardCollection rewardCollection = new RewardCollection(definition);
						if (persistCreatedCollection)
						{
							playerService.Add(rewardCollection);
							startedCollectionIDs.Add(rewardCollection.ID);
						}
						return rewardCollection;
					}
				}
			}
			return null;
		}

		public bool checkAndCompleteFinalRewardEarned(RewardCollection collection)
		{
			bool result = false;
			int transactionID = collection.Definition.Rewards[collection.Definition.Rewards.Count - 1].TransactionID;
			TransactionDefinition transactionDefinition = definitionService.Get<TransactionDefinition>(transactionID);
			if (transactionDefinition != null)
			{
				foreach (QuantityItem output in transactionDefinition.Outputs)
				{
					CompositeBuildingPiece firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<CompositeBuildingPiece>(output.ID);
					if (firstInstanceByDefinitionId != null)
					{
						result = true;
						logger.Log(KampaiLogLevel.Info, "User already has earned last reward of reward collection, marking this collection as completed");
						collection.NumRewardsCollected = collection.Definition.Rewards.Count;
						break;
					}
				}
			}
			return result;
		}

		public void IncreaseScoreForMignetteCollection(int mignetteBuildingId, int scoreIncrease)
		{
			MignetteBuilding byInstanceId = playerService.GetByInstanceId<MignetteBuilding>(mignetteBuildingId);
			RewardCollection activeCollectionForMignette = GetActiveCollectionForMignette(byInstanceId);
			byInstanceId.TotalScore += scoreIncrease;
			activeCollectionForMignette.IncreaseScore(scoreIncrease);
		}

		public TransactionDefinition CreditRewardForActiveMignette()
		{
			RewardCollection collectionForActiveMignette = GetCollectionForActiveMignette();
			if (!collectionForActiveMignette.HasRewardReadyForCollection())
			{
				logger.Log(KampaiLogLevel.Error, "CreditRewardForActiveMignette called, but no reward is available! collectionID: " + collectionForActiveMignette.ID);
			}
			int transactionIDReadyForCollection = collectionForActiveMignette.GetTransactionIDReadyForCollection();
			TransactionDefinition transactionDefinition = definitionService.Get<TransactionDefinition>(transactionIDReadyForCollection);
			playerService.RunEntireTransaction(transactionDefinition, TransactionTarget.NO_VISUAL, null);
			collectionForActiveMignette.NumRewardsCollected++;
			if (collectionForActiveMignette.IsCompleted())
			{
				int num = collectionForActiveMignette.CollectionScoreProgress - collectionForActiveMignette.GetMaxScore();
				if (num > 0)
				{
					GetCollectionForActiveMignette().CollectionScoreProgress += num;
				}
			}
			return transactionDefinition;
		}

		public void ResetMignetteProgress()
		{
			List<MignetteBuilding> instancesByType = playerService.GetInstancesByType<MignetteBuilding>();
			foreach (MignetteBuilding item in instancesByType)
			{
				item.TotalScore = 0;
				RewardCollection activeCollectionForMignette = GetActiveCollectionForMignette(item.ID);
				activeCollectionForMignette.ResetProgress();
			}
		}
	}
}
