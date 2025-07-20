using System.Collections;
using System.Collections.Generic;
using Kampai.Main;
using Kampai.Util;
using Kampai.Util.Audio;
using UnityEngine;

namespace Kampai.UI.View
{
	public class PartyOnboardingMediator : KampaiMediator
	{
		private Dictionary<string, float> loopingEventParameters = new Dictionary<string, float>(1);

		private CustomFMOD_StudioEventEmitter emitter;

		private bool isClicked;

		private GoTween throbTween;

		private KampaiRaycaster tempRaycaster;

		private Coroutine autoCloseCoroutine;

		[Inject]
		public PartyOnboardingView view { get; set; }

		[Inject(UIElement.HUD)]
		public GameObject hud { get; set; }

		[Inject(MainElement.UI_DOOBER_CANVAS)]
		public GameObject dooberCanvas { get; set; }

		[Inject]
		public HideSkrimSignal hideSkrimSignal { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public StartLeisurePartyPointsFinishedSignal unlockSignal { get; set; }

		[Inject]
		public StartLoopingAudioSignal startLoopingAudioSignal { get; set; }

		public override void Initialize(GUIArguments args)
		{
			isClicked = false;
			Vector3 originalScale;
			throbTween = TweenUtil.Throb(view.transform, 0.8f, 0.7f, out originalScale);
			autoCloseCoroutine = StartCoroutine(autoClick());
		}

		public override void OnRegister()
		{
			base.OnRegister();
			view.transform.SetParent(dooberCanvas.transform, false);
			tempRaycaster = dooberCanvas.gameObject.AddComponent<KampaiRaycaster>();
			view.button.ClickedSignal.AddListener(onClicked);
			emitter = GetAudioEmitter.Get(view.gameObject, "LocalAudio");
			SetAudioLoop(true);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			view.button.ClickedSignal.RemoveListener(onClicked);
		}

		public void onClicked()
		{
			Object.Destroy(tempRaycaster);
			if (autoCloseCoroutine != null)
			{
				StopCoroutine(autoCloseCoroutine);
			}
			isClicked = true;
			throbTween.pause();
			view.trailParticles.gameObject.SetActive(true);
			Transform transform = hud.transform.Find("PointsPanel");
			Vector3 destination = new Vector3(transform.position.x, transform.position.y, transform.position.z);
			hideSkrimSignal.Dispatch("PartyOnboardSkrim");
			tweenToDestination(view.gameObject, destination);
			SetAudioLoop(false);
		}

		private void SetAudioLoop(bool isLooping)
		{
			loopingEventParameters["endLoop"] = ((!isLooping) ? 1 : 0);
			startLoopingAudioSignal.Dispatch(emitter, "Play_partyMeter_icon_01", loopingEventParameters);
		}

		private IEnumerator autoClick()
		{
			yield return new WaitForSeconds(4f);
			if (!isClicked)
			{
				onClicked();
			}
		}

		private void tweenToDestination(GameObject go, Vector3 destination)
		{
			Go.to(go.transform, 2f, new GoTweenConfig().setEaseType(GoEaseType.QuartIn).scale(0.2f).position(destination)
				.onComplete(delegate
				{
					guiService.Execute(GUIOperation.Unload, "PartyOnboarding");
					unlockSignal.Dispatch();
				}));
		}
	}
}
