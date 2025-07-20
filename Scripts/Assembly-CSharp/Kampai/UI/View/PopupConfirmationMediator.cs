using Kampai.Game;
using Kampai.Main;
using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class PopupConfirmationMediator : UIStackMediator<PopupConfirmationView>
	{
		private bool result;

		private Signal<bool> CallbackSignal;

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public CloseConfirmationSignal closeConfirmationSignal { get; set; }

		[Inject]
		public HideSkrimSignal hideSkrim { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			closeConfirmationSignal.AddListener(Cancel);
			base.view.Decline.ClickedSignal.AddListener(Cancel);
			base.view.Accept.ClickedSignal.AddListener(Proceed);
			base.view.OnMenuClose.AddListener(OnMenuClose);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			closeConfirmationSignal.RemoveListener(Cancel);
			base.view.Decline.ClickedSignal.RemoveListener(Cancel);
			base.view.Accept.ClickedSignal.RemoveListener(Proceed);
			base.view.OnMenuClose.RemoveListener(OnMenuClose);
		}

		public override void Initialize(GUIArguments args)
		{
			PopupConfirmationSetting setting = args.Get<PopupConfirmationSetting>();
			Init(setting);
		}

		protected override void OnCloseAllMenu(GameObject exception)
		{
		}

		protected override void Close()
		{
			soundFXSignal.Dispatch("Play_main_menu_close_01");
			base.view.Close();
		}

		private void Cancel()
		{
			result = false;
			Close();
		}

		private void Proceed()
		{
			result = true;
			Close();
		}

		private void Init(PopupConfirmationSetting setting)
		{
			base.view.title.text = localService.GetString(setting.TitleKey);
			if (setting.DescriptionAlreadyTranslated)
			{
				base.view.description.text = setting.DescriptionKey;
			}
			else
			{
				base.view.description.text = localService.GetString(setting.DescriptionKey);
			}
			if (!string.IsNullOrEmpty(setting.LeftButtonText))
			{
				base.view.LeftButton.LocKey = setting.LeftButtonText;
			}
			if (!string.IsNullOrEmpty(setting.RightButtonText))
			{
				base.view.RightButton.LocKey = setting.RightButtonText;
			}
			string imagePath = setting.ImagePath;
			if (imagePath == null || string.IsNullOrEmpty(setting.ImagePath))
			{
				imagePath = "img_char_Min_FeedbackPositive01";
			}
			CallbackSignal = setting.ConfirmationCallback;
			base.closeAllOtherMenuSignal.Dispatch(base.gameObject);
			base.view.Init();
		}

		private void OnMenuClose()
		{
			guiService.Execute(GUIOperation.Unload, "popup_Confirmation");
			hideSkrim.Dispatch("ConfirmationSkrim");
			CallbackSignal.Dispatch(result);
			result = false;
		}
	}
}
