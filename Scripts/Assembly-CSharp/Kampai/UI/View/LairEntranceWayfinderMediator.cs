using Kampai.Common;
using Kampai.Game;
using strange.extensions.context.api;

namespace Kampai.UI.View
{
	public class LairEntranceWayfinderMediator : AbstractWayFinderMediator
	{
		private MasterPlanComponentTaskUpdatedSignal taskUpdatedSignal;

		private SetMasterPlanWayfinderIconToCompleteSignal setCompleteIconSignal;

		[Inject]
		public LairEntranceWayfinderView view { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IMasterPlanService planService { get; set; }

		[Inject]
		public StopEntranceWayfinderPulseSignal stopPulseSignal { get; set; }

		[Inject]
		public ResetLairWayfinderIconSignal resetIconSignal { get; set; }

		[Inject]
		public PickControllerModel pickControllerModel { get; set; }

		[Inject]
		public MoveBuildMenuSignal moveBuildMenuSignal { get; set; }

		public override IWayFinderView View
		{
			get
			{
				return view;
			}
		}

		public override void OnRegister()
		{
			base.OnRegister();
			taskUpdatedSignal = gameContext.injectionBinder.GetInstance<MasterPlanComponentTaskUpdatedSignal>();
			setCompleteIconSignal = gameContext.injectionBinder.GetInstance<SetMasterPlanWayfinderIconToCompleteSignal>();
			taskUpdatedSignal.AddListener(view.TaskUpdated);
			setCompleteIconSignal.AddListener(view.SetBuildReadyIcon);
			resetIconSignal.AddListener(view.ResetDefaultIcon);
			stopPulseSignal.AddListener(view.StopPulse);
			VillainLair firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<VillainLair>(3137);
			planService.SetWayfinderState();
			if (firstInstanceByDefinitionId != null && !firstInstanceByDefinitionId.hasVisited)
			{
				view.StartPulse();
			}
		}

		public override void OnRemove()
		{
			base.OnRemove();
			taskUpdatedSignal.RemoveListener(view.TaskUpdated);
			taskUpdatedSignal = null;
			setCompleteIconSignal.RemoveListener(view.SetBuildReadyIcon);
			setCompleteIconSignal = null;
			resetIconSignal.RemoveListener(view.ResetDefaultIcon);
			stopPulseSignal.RemoveListener(view.StopPulse);
		}

		protected override void GoToClicked()
		{
			if (!pickControllerModel.SelectedBuilding.HasValue)
			{
				moveBuildMenuSignal.Dispatch(false);
				base.GoToClicked();
			}
		}
	}
}
