using System;
using System.Collections.Generic;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using strange.extensions.context.api;

namespace Kampai.UI.View
{
	public class MasterPlanSelectComponentMediator : UIStackMediator<MasterPlanSelectComponentView>
	{
		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public RefreshFromIndexSignal refreshFromIndex { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public MasterPlanSelectComponentSignal selectComponentSignal { get; set; }

		[Inject]
		public HideFluxWayfinder hideFluxWayfinderSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal sfxSignal { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public MoveSkrimTopLayerSignal moveSkrimTopLayerSignal { get; set; }

		[Inject]
		public HideSkrimSignal hideSkrimSignal { get; set; }

		[Inject]
		public IGhostComponentService ghostService { get; set; }

		[Inject]
		public IMasterPlanService masterPlanService { get; set; }

		[Inject]
		public VillainLairModel lairModel { get; set; }

		public override void Initialize(GUIArguments args)
		{
			Tuple<int, int> tuple = args.Get<Tuple<int, int>>();
			int item = tuple.Item1;
			int item2 = tuple.Item2;
			int selectedIndex;
			if (GetActiveComponent(out selectedIndex) == null)
			{
				selectedIndex = -1;
			}
			hideFluxWayfinderSignal.Dispatch(true);
			base.view.Init(item, item2, playerService, definitionService, guiService, ghostService, masterPlanService);
			moveSkrimTopLayerSignal.Dispatch("MasterPlan");
		}

		private MasterPlanComponent GetActiveComponent(out int selectedIndex)
		{
			selectedIndex = 0;
			IList<MasterPlanComponent> instancesByType = playerService.GetInstancesByType<MasterPlanComponent>();
			for (int i = 0; i < instancesByType.Count; i++)
			{
				MasterPlanComponent masterPlanComponent = instancesByType[i];
				if (masterPlanComponent.State != 0 && masterPlanComponent.State != MasterPlanComponentState.Scaffolding && masterPlanComponent.State != MasterPlanComponentState.Complete)
				{
					selectedIndex = i;
					return masterPlanComponent;
				}
			}
			return null;
		}

		public override void OnRegister()
		{
			base.OnRegister();
			base.view.updateSubViewSignal.AddListener(SignalViewUpdate);
			base.view.nextButtonView.ClickedSignal.AddListener(base.view.NextComponent);
			base.view.previousButtonView.ClickedSignal.AddListener(base.view.PreviousComponent);
			base.view.actionButtonView.ClickedSignal.AddListener(ComponentSelected);
			base.view.OnMenuClose.AddListener(OnMenuClose);
			base.view.PanWithinLairSignal.AddListener(PanCameraWithinLair);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			base.view.updateSubViewSignal.RemoveListener(SignalViewUpdate);
			base.view.nextButtonView.ClickedSignal.RemoveListener(base.view.NextComponent);
			base.view.previousButtonView.ClickedSignal.RemoveListener(base.view.PreviousComponent);
			base.view.actionButtonView.ClickedSignal.RemoveListener(ComponentSelected);
			base.view.OnMenuClose.RemoveListener(OnMenuClose);
			base.view.PanWithinLairSignal.RemoveListener(PanCameraWithinLair);
		}

		private void SignalViewUpdate(Type type, int index)
		{
			refreshFromIndex.Dispatch(type, index);
		}

		private void ComponentSelected()
		{
			if (base.view.selectedIndex >= 0)
			{
				selectComponentSignal.Dispatch(base.view.planDefinition, base.view.selectedIndex, false);
				Close();
			}
		}

		private void OnMenuClose()
		{
			hideFluxWayfinderSignal.Dispatch(false);
			base.view.PanToMainLairView();
			hideSkrimSignal.Dispatch("MasterPlan");
			sfxSignal.Dispatch("Play_menu_disappear_01");
			guiService.Execute(GUIOperation.Unload, "screen_MasterPlanComponentSelection");
		}

		protected override void Close()
		{
			base.view.ReleaseViews();
			base.view.PanToMainLairView();
			base.view.Close();
		}

		private void PanCameraWithinLair(int cameraPos, Boxed<Action> callback)
		{
			if (lairModel.currentActiveLair != null)
			{
				gameContext.injectionBinder.GetInstance<CameraMoveToCustomPositionSignal>().Dispatch(cameraPos, callback);
			}
		}
	}
}
