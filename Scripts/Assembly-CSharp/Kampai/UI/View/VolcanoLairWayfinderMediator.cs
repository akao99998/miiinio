using Kampai.Game;
using strange.extensions.context.api;

namespace Kampai.UI.View
{
	public class VolcanoLairWayfinderMediator : AbstractWayFinderMediator
	{
		private MasterPlanComponentTaskUpdatedSignal taskUpdatedSignal;

		private SetMasterPlanWayfinderIconToCompleteSignal setCompleteIconSignal;

		[Inject]
		public VolcanoLairWayfinderView view { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IMasterPlanService planService { get; set; }

		[Inject]
		public CloseAllOtherMenuSignal closeAllOtherMenusSignal { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public ResetLairWayfinderIconSignal resetIconSignal { get; set; }

		[Inject]
		public HideFluxWayfinder hideWayfinder { get; set; }

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
			hideWayfinder.AddListener(ShowWayfinder);
			resetIconSignal.AddListener(view.ResetDefaultIcon);
			taskUpdatedSignal = gameContext.injectionBinder.GetInstance<MasterPlanComponentTaskUpdatedSignal>();
			setCompleteIconSignal = gameContext.injectionBinder.GetInstance<SetMasterPlanWayfinderIconToCompleteSignal>();
			taskUpdatedSignal.AddListener(view.TaskUpdated);
			setCompleteIconSignal.AddListener(view.SetBuildReadyIcon);
			planService.SetWayfinderState();
			view.SetOffset();
		}

		public override void OnRemove()
		{
			base.OnRemove();
			hideWayfinder.RemoveListener(ShowWayfinder);
			resetIconSignal.RemoveListener(view.ResetDefaultIcon);
			taskUpdatedSignal.RemoveListener(view.TaskUpdated);
			taskUpdatedSignal = null;
			setCompleteIconSignal.RemoveListener(view.SetBuildReadyIcon);
			setCompleteIconSignal = null;
		}

		protected override void GoToClicked()
		{
			if (!base.pickModel.PanningCameraBlocked)
			{
				closeAllOtherMenusSignal.Dispatch(null);
				gameContext.injectionBinder.GetInstance<DetermineLairUISignal>().Dispatch();
			}
		}

		private void ShowWayfinder(bool enabled)
		{
			if (playerService.GetFirstInstanceByDefinitionId<VillainLair>(3137).hasVisited)
			{
				view.SetForceHide(enabled);
			}
		}
	}
}
