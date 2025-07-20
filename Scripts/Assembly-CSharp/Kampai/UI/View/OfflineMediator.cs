using System.Collections;
using Kampai.Common;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class OfflineMediator : Mediator
	{
		private Button button;

		[Inject]
		public OfflineView view { get; set; }

		[Inject]
		public ILocalizationService locService { get; set; }

		[Inject]
		public NetworkModel networkModel { get; set; }

		[Inject]
		public ShowOfflinePopupSignal showOfflinePopupSignal { get; set; }

		[Inject]
		public ResumeNetworkOperationSignal resumeNetworkOperationSignal { get; set; }

		[Inject]
		public NetworkLostOpenSignal openSignal { get; set; }

		[Inject]
		public NetworkLostCloseSignal closeSignal { get; set; }

		public override void OnRegister()
		{
			view.retryButton.ClickedSignal.AddListener(OnRetry);
			view.title.text = locService.GetString("OfflineTitle");
			view.description.text = locService.GetString("OfflineDescription");
			view.retryButtonText.text = locService.GetString("OfflineRetry");
			view.OnMenuClose.AddListener(OnMenuClose);
			view.Init();
			view.Open();
			button = view.retryButton.GetComponent<Button>();
			openSignal.Dispatch();
		}

		public override void OnRemove()
		{
			view.retryButton.ClickedSignal.RemoveListener(OnRetry);
			view.OnMenuClose.RemoveListener(OnMenuClose);
			closeSignal.Dispatch();
		}

		private void OnRetry()
		{
			button.interactable = false;
			StartCoroutine(WaitForRetry());
			networkModel.isConnectionLost = !NetworkUtil.IsConnected();
			if (!networkModel.isConnectionLost)
			{
				Close();
				resumeNetworkOperationSignal.Dispatch();
			}
		}

		private IEnumerator WaitForRetry()
		{
			yield return new WaitForSeconds(2f);
			if (view.retryButton != null)
			{
				button.interactable = true;
			}
		}

		private void OnMenuClose()
		{
			if (!networkModel.isConnectionLost)
			{
				showOfflinePopupSignal.Dispatch(false);
			}
			else
			{
				view.Open();
			}
		}

		private void Close()
		{
			view.Close();
		}
	}
}
