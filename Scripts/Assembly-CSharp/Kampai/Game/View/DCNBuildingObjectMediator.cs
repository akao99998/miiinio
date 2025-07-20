using System.Collections;
using UnityEngine;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;

namespace Kampai.Game.View
{
	public class DCNBuildingObjectMediator : EventMediator
	{
		private Signal<bool> callbackSignal = new Signal<bool>();

		[Inject]
		public DCNBuildingObjectView view { get; set; }

		[Inject]
		public ShowDCNScreenSignal showDCNScreenSignal { get; set; }

		[Inject]
		public DCNMaybeShowContentSignal dcnMaybeShowContentSignal { get; set; }

		[Inject]
		public DCNShowFeaturedContentSignal dcnShowFeaturedContentSignal { get; set; }

		[Inject]
		public IDCNService dcnService { get; set; }

		[Inject]
		public QueueDCNConfirmationSignal queueConfirmationSignal { get; set; }

		public override void OnRegister()
		{
			showDCNScreenSignal.AddListener(ShowScreen);
			dcnShowFeaturedContentSignal.AddListener(ShowContent);
			callbackSignal.AddListener(OpenContentCallback);
			StartCoroutine(DCNViewIsReady());
		}

		public override void OnRemove()
		{
			showDCNScreenSignal.RemoveListener(ShowScreen);
			dcnShowFeaturedContentSignal.RemoveListener(ShowContent);
			callbackSignal.RemoveListener(OpenContentCallback);
		}

		private void ShowScreen(bool show)
		{
			if (show)
			{
				view.ShowScreen();
			}
			else
			{
				view.HideScreen();
			}
		}

		private IEnumerator DCNViewIsReady()
		{
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			dcnMaybeShowContentSignal.Dispatch();
		}

		private void ShowContent()
		{
			if (view.ScreenIsOpen())
			{
				queueConfirmationSignal.Dispatch(callbackSignal);
			}
		}

		private void OpenContentCallback(bool result)
		{
			if (result)
			{
				view.HideScreen();
			}
			dcnService.OpenFeaturedContent(result);
		}
	}
}
