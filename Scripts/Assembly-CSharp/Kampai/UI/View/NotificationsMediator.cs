using Kampai.Main;
using UnityEngine;

namespace Kampai.UI.View
{
	public class NotificationsMediator : UIStackMediator<NotificationsView>
	{
		private bool autoClose;

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playSFXSignal { get; set; }

		[Inject]
		public HideSkrimSignal hideSkrimSignal { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		public override void OnRegister()
		{
			playSFXSignal.Dispatch("Play_menu_popUp_01");
			base.OnRegister();
			base.view.OnMenuClose.AddListener(Close);
			base.view.confirmButton.ClickedSignal.AddListener(Close);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			base.view.OnMenuClose.RemoveListener(Close);
			base.view.confirmButton.ClickedSignal.RemoveListener(Close);
		}

		public override void Initialize(GUIArguments args)
		{
			string message = args.Get<string>();
			autoClose = args.Get<bool>();
			base.view.Init(localService, message);
		}

		protected override void Close()
		{
			hideSkrimSignal.Dispatch("NotificationsSkrim");
			guiService.Execute(GUIOperation.Unload, "popup_Notification");
		}

		protected override void OnCloseAllMenu(GameObject exception)
		{
			if (autoClose)
			{
				base.OnCloseAllMenu(exception);
			}
		}
	}
}
