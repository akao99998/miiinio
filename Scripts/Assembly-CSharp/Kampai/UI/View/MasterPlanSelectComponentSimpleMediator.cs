namespace Kampai.UI.View
{
	public class MasterPlanSelectComponentSimpleMediator : UIStackMediator<MasterPlanSelectComponentSimpleView>
	{
		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public CloseAllMessageDialogs closeAllMessageDialogs { get; set; }

		[Inject]
		public IGhostComponentService ghostService { get; set; }

		public override void OnRegister()
		{
			base.view.Init();
			closeAllMessageDialogs.AddListener(Close);
			base.view.OnMenuClose.AddListener(OnMenuClose);
			ghostService.DisplayAllSelectablePlanComponents();
			base.OnRegister();
		}

		public override void OnRemove()
		{
			closeAllMessageDialogs.RemoveListener(Close);
			base.view.OnMenuClose.RemoveListener(OnMenuClose);
			base.OnRemove();
		}

		protected override void Close()
		{
			ghostService.ClearGhostComponentBuildings();
			base.view.Close();
		}

		private void OnMenuClose()
		{
			guiService.Execute(GUIOperation.Unload, "screen_MasterplanSelectComponent");
		}
	}
}
