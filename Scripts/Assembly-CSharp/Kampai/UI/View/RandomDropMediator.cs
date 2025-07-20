using System.Collections;
using Kampai.Common;
using Kampai.Main;
using UnityEngine;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class RandomDropMediator : Mediator
	{
		[Inject]
		public RandomDropView view { get; set; }

		[Inject]
		public ZoomPercentageSignal zoomSignal { get; set; }

		[Inject]
		public SpawnDooberSignal tweenSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		[Inject]
		public RequestZoomPercentageSignal requestSignal { get; set; }

		public override void OnRegister()
		{
			Init();
			soundFXSignal.Dispatch("Play_drop_harvest_01");
			view.timeToTweenSignal.AddListener(TweenTime);
			zoomSignal.AddListener(UpdateScale);
			view.button.ClickedSignal.AddListener(OnClick);
		}

		public override void OnRemove()
		{
			view.timeToTweenSignal.RemoveListener(TweenTime);
			zoomSignal.RemoveListener(UpdateScale);
			view.button.ClickedSignal.RemoveListener(OnClick);
		}

		private void Init()
		{
			view.Init();
			StartCoroutine(WaitAFrame());
		}

		private IEnumerator WaitAFrame()
		{
			yield return null;
			requestSignal.Dispatch();
		}

		private void UpdateScale(float percentage)
		{
			view.UpdateScale(percentage);
		}

		private void TweenTime(Vector3 startPosition, int itemDefId)
		{
			tweenSignal.Dispatch(startPosition, DestinationType.STORAGE, itemDefId, true);
			Object.Destroy(base.gameObject);
		}

		private void OnClick()
		{
			view.KillTweens();
			TweenTime(view.gameObject.transform.position, view.ItemDefinitionId);
		}
	}
}
