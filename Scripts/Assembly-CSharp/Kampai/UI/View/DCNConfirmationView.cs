using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	[RequireComponent(typeof(Animator))]
	public class DCNConfirmationView : PopupMenuView
	{
		public ButtonView confirmButton;

		public ButtonView declineButton;

		public ToggleButtonView toggleButton;

		private Signal<bool> CallbackSignal;

		private ILocalPersistanceService localPersistanceService;

		internal bool opened;

		public void Init(Signal<bool> callback, ILocalPersistanceService localPersistanceService)
		{
			base.Init();
			this.localPersistanceService = localPersistanceService;
			CallbackSignal = callback;
			base.Open();
		}

		internal void SetupSignals()
		{
			confirmButton.ClickedSignal.AddListener(OnConfirmButtonClick);
			declineButton.ClickedSignal.AddListener(OnDeclineButtonClick);
		}

		internal void RemoveSignals()
		{
			confirmButton.ClickedSignal.RemoveListener(OnConfirmButtonClick);
			declineButton.ClickedSignal.RemoveListener(OnDeclineButtonClick);
		}

		private void OnConfirmButtonClick()
		{
			if (toggleButton.IsOn)
			{
				localPersistanceService.PutDataInt("DCNStoreDoNotShow", 1);
			}
			opened = true;
			CallbackSignal.Dispatch(opened);
			Close();
		}

		private void OnDeclineButtonClick()
		{
			CallbackSignal.Dispatch(opened);
			Close();
		}
	}
}
