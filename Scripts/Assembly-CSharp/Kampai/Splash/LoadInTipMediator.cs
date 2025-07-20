using System.Collections;
using Kampai.Main;
using UnityEngine;
using strange.extensions.mediation.impl;

namespace Kampai.Splash
{
	public class LoadInTipMediator : Mediator
	{
		private float waitSeconds;

		private IEnumerator cycleRoutine;

		[Inject]
		public ILoadInService loadInService { get; set; }

		[Inject]
		public LocalizationServiceInitializedSignal localizationServiceInitializedSignal { get; set; }

		[Inject]
		public LoadInTipView view { get; set; }

		public override void OnRegister()
		{
			localizationServiceInitializedSignal.AddListener(Init);
		}

		public override void OnRemove()
		{
			localizationServiceInitializedSignal.RemoveListener(Init);
			if (cycleRoutine != null)
			{
				StopCoroutine(cycleRoutine);
			}
		}

		private void Init()
		{
			UpdateTip();
		}

		private void UpdateTip()
		{
			TipToShow nextTip = loadInService.GetNextTip();
			if (nextTip.Text.Length == 0)
			{
				view.gameObject.SetActive(false);
				return;
			}
			waitSeconds = nextTip.Time;
			view.SetTip(nextTip.Text);
			cycleRoutine = CycleTip();
			StartCoroutine(cycleRoutine);
		}

		private IEnumerator CycleTip()
		{
			yield return new WaitForSeconds(waitSeconds);
			UpdateTip();
		}
	}
}
