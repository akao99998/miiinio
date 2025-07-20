using Kampai.Game.Mignette;
using Kampai.UI.View;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class ShowAndIncreaseMignetteScoreCommand : Command
	{
		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public MignetteGameModel mignetteGameModel { get; set; }

		[Inject]
		public EjectAllMinionsFromBuildingSignal ejectAllMinionsFromBuildingSignal { get; set; }

		[Inject]
		public MignetteCollectionService collectionService { get; set; }

		[Inject]
		public ShowHUDSignal showHUDSignal { get; set; }

		[Inject]
		public SetHUDButtonsVisibleSignal setHUDButtonsVisibleSignal { get; set; }

		[Inject]
		public DestroyMignetteContextSignal destroyMignetteContextSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public IAchievementService achievementService { get; set; }

		[Inject]
		public IMasterPlanService masterPlanService { get; set; }

		public override void Execute()
		{
			collectionService.IncreaseScoreForMignetteCollection(mignetteGameModel.BuildingId, mignetteGameModel.CurrentGameScore);
			if (!playerService.HasPurchasedMinigamePack())
			{
				ejectAllMinionsFromBuildingSignal.Dispatch(mignetteGameModel.BuildingId);
			}
			destroyMignetteContextSignal.Dispatch();
			MignetteBuilding byInstanceId = playerService.GetByInstanceId<MignetteBuilding>(mignetteGameModel.BuildingId);
			if (byInstanceId != null)
			{
				achievementService.UpdateIncrementalAchievement(byInstanceId.MignetteBuildingDefinition.ID, 1);
				questService.UpdateAllQuestsWithQuestStepType(QuestStepType.Mignette, QuestTaskTransition.Complete, byInstanceId);
				UpdateMasterPlanComponentTasks(byInstanceId);
			}
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Load, "MignetteScoreSummary");
			iGUICommand.Args.Add(true);
			iGUICommand.Args.Add(mignetteGameModel.BuildingId);
			iGUICommand.skrimScreen = "MignetteSkrim";
			iGUICommand.darkSkrim = true;
			iGUICommand.disableSkrimButton = true;
			guiService.Execute(iGUICommand);
			showHUDSignal.Dispatch(true);
			setHUDButtonsVisibleSignal.Dispatch(false);
		}

		private void UpdateMasterPlanComponentTasks(MignetteBuilding building)
		{
			uint currentGameScore = (uint)mignetteGameModel.CurrentGameScore;
			int iD = building.Definition.ID;
			uint progress = (uint)building.Definition.XPRewardFactor;
			masterPlanService.ProcessActiveComponent(MasterPlanComponentTaskType.PlayMiniGame, 1u, iD);
			masterPlanService.ProcessActiveComponent(MasterPlanComponentTaskType.MiniGameScore, currentGameScore, iD);
			masterPlanService.ProcessActiveComponent(MasterPlanComponentTaskType.EarnPartyPoints, progress, iD);
			masterPlanService.ProcessActiveComponent(MasterPlanComponentTaskType.EarnMignettePartyPoints, progress, iD);
		}
	}
}
