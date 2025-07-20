using Kampai.Game;

namespace Kampai.UI.View
{
	public abstract class AbstractQuestWayFinderMediator : AbstractWayFinderMediator
	{
		[Inject]
		public ShowQuestPanelSignal ShowQuestPanelSignal { get; set; }

		[Inject]
		public ShowQuestRewardSignal ShowQuestRewardSignal { get; set; }

		[Inject]
		public IZoomCameraModel zoomCameraModel { get; set; }

		protected override void GoToClicked()
		{
			if (base.pickModel.PanningCameraBlocked || base.lairModel.goingToLair)
			{
				return;
			}
			IQuestWayFinderView questWayFinderView = View as IQuestWayFinderView;
			IChildWayFinderView childWayFinderView = questWayFinderView as IChildWayFinderView;
			if (questWayFinderView.IsTargetObjectVisible() || (childWayFinderView != null && childWayFinderView.ParentWayFinderTrackedId == 313 && zoomCameraModel.ZoomedIn))
			{
				HandleClick();
				return;
			}
			zoomCameraModel.ZoomedIn = false;
			base.PanToInstance();
			if (childWayFinderView != null && childWayFinderView.ParentWayFinderTrackedId == 313)
			{
				HandleClick();
			}
		}

		private void HandleClick()
		{
			AbstractQuestWayFinderView abstractQuestWayFinderView = View as AbstractQuestWayFinderView;
			abstractQuestWayFinderView.ClickedOnce = true;
			Quest currentActiveQuest = abstractQuestWayFinderView.CurrentActiveQuest;
			if (currentActiveQuest == null)
			{
				return;
			}
			bool flag = currentActiveQuest.IsProcedurallyGenerated();
			switch (currentActiveQuest.state)
			{
			case QuestState.Notstarted:
			case QuestState.RunningStartScript:
			case QuestState.RunningTasks:
			case QuestState.RunningCompleteScript:
				if (flag)
				{
					SelectTSM();
				}
				else
				{
					ShowQuestPanelSignal.Dispatch(currentActiveQuest.ID);
				}
				break;
			case QuestState.Harvestable:
				if (flag)
				{
					SelectTSM();
				}
				else
				{
					ShowQuestRewardSignal.Dispatch(currentActiveQuest.ID);
				}
				break;
			}
			abstractQuestWayFinderView.SetNextQuest();
		}

		private void SelectTSM()
		{
			base.GameContext.injectionBinder.GetInstance<TSMCharacterSelectedSignal>().Dispatch();
		}
	}
}
