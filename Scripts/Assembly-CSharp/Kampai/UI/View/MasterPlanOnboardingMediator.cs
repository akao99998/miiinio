using Kampai.Game;
using Kampai.Main;

namespace Kampai.UI.View
{
	public class MasterPlanOnboardingMediator : UIStackMediator<MasterPlanOnboardingView>
	{
		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public DisplayMasterPlanOnboardingSignal displayOnboardSignal { get; set; }

		[Inject]
		public DisplayMasterPlanIntroDialogSignal introDialogSignal { get; set; }

		[Inject]
		public IGhostComponentService ghostService { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		[Inject]
		public HideSkrimSignal hideSkrimSignal { get; set; }

		public override void OnRegister()
		{
			base.view.definitionService = definitionService;
			base.view.OnMenuClose.AddListener(OnMenuClose);
			base.view.actionButtonView.ClickedSignal.AddListener(Next);
		}

		public override void OnRemove()
		{
			base.view.OnMenuClose.RemoveListener(OnMenuClose);
			base.view.actionButtonView.ClickedSignal.RemoveListener(Next);
		}

		public override void Initialize(GUIArguments args)
		{
			base.view.definition = args.Get<MasterPlanOnboardDefinition>();
			base.view.Initialize();
			base.view.Init();
			soundFXSignal.Dispatch("Play_menu_popUp_01");
			base.view.Open();
		}

		protected override void Close()
		{
			hideSkrimSignal.Dispatch("MasterPlanOnboarding");
			base.view.Close();
		}

		private void OnMenuClose()
		{
			soundFXSignal.Dispatch("Play_menu_disappear_01");
			IGUICommand command = guiService.BuildCommand(GUIOperation.Unload, "screen_MasterPlanOnboarding");
			guiService.Execute(command);
			if (!base.view.IsLast)
			{
				displayOnboardSignal.Dispatch(base.view.definition.nextOnboardDefinitionId);
			}
		}

		private void Next()
		{
			if (!(base.view == null))
			{
				ghostService.RunEndGhostComponentFunctionFromDefinition(base.view.definition.ghostFunction.closeType);
				Close();
				if (base.view.IsLast)
				{
					introDialogSignal.Dispatch();
				}
			}
		}
	}
}
