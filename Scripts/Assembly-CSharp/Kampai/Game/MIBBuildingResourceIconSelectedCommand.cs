using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Game.Transaction;
using Kampai.Main;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class MIBBuildingResourceIconSelectedCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("MIBBuildingResourceIconSelectedCommand") as IKampaiLogger;

		[Inject]
		public RemoveResourceIconSignal removeResourceIconSignal { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IMIBService mibService { get; set; }

		[Inject]
		public ILocalPersistanceService localPersistanceService { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playGlobalSoundFXSignal { get; set; }

		public override void Execute()
		{
			if (!mibService.IsUserReturning())
			{
				return;
			}
			logger.Debug("MIB: User selected resource icon, showing reward UI");
			MIBBuilding firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<MIBBuilding>(3129);
			removeResourceIconSignal.Dispatch(new Tuple<int, int>(firstInstanceByDefinitionId.ID, -1));
			mibService.ClearReturningKey();
			UserSegment segmentDefinition = GetSegmentDefinition(firstInstanceByDefinitionId);
			if (segmentDefinition == null)
			{
				logger.Error("MIB: Failed to find a suitable user segment");
				return;
			}
			int num = segmentDefinition.FirstXReturnRewardsWeightedDefinitionId;
			if (firstInstanceByDefinitionId.NumOfRewardsCollectedOnReturn > segmentDefinition.AfterXReturnRewards)
			{
				num = segmentDefinition.SecondXReturnRewardsWeightedDefinitionId;
			}
			logger.Info("MIB: Granting user reward {0} segmentedWeightedDefId:{1}", MIBRewardType.ON_RETURN, num);
			TransactionDefinition transactionDefinition = mibService.PickWeightedTransaction(num);
			if (transactionDefinition == null)
			{
				logger.Error("MIB: Failed to find weighted definition id {0}, will not grant user rewards", num);
				return;
			}
			MIBRewardView mIBRewardView = CreateUIView();
			if (mIBRewardView == null)
			{
				logger.Error("MIB: Invalid reward prefab, can not grant user rewards");
				return;
			}
			mIBRewardView.Init(transactionDefinition, mibService.GetItemDefinition(transactionDefinition), mibService.GetItemDefinitions(num), playGlobalSoundFXSignal);
			playGlobalSoundFXSignal.Dispatch("Play_UI_levelUp_rewards_01");
		}

		private UserSegment GetSegmentDefinition(MIBBuilding building)
		{
			string dataPlayer = localPersistanceService.GetDataPlayer("IsSpender");
			List<UserSegment> list = null;
			list = ((!(dataPlayer == "true")) ? building.Definition.ReturnNonSpenderLevelSegments : building.Definition.ReturnSpenderLevelSegments);
			if (list == null || list.Count <= 0)
			{
				logger.Error("MIB: Failed to find segment, please check definition id: {0}", building.Definition.ID);
				return null;
			}
			int quantity = (int)playerService.GetQuantity(StaticItem.LEVEL_ID);
			for (int i = 0; i < list.Count; i++)
			{
				if (quantity >= list[i].LevelGreaterThanOrEqualTo)
				{
					return list[i];
				}
			}
			return null;
		}

		private MIBRewardView CreateUIView()
		{
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Load, "screen_MessageInABottle");
			iGUICommand.darkSkrim = true;
			iGUICommand.disableSkrimButton = true;
			iGUICommand.skrimScreen = "MIBRewardScreenSkrim";
			GameObject gameObject = guiService.Execute(iGUICommand);
			return gameObject.GetComponent<MIBRewardView>();
		}
	}
}
