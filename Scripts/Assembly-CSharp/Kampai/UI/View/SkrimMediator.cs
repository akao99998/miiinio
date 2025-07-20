using System;
using System.Collections;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;

namespace Kampai.UI.View
{
	public class SkrimMediator : KampaiMediator, KampaiDisposable
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("SkrimMediator") as IKampaiLogger;

		private bool triedToCloseTheNextFrame;

		private float fadeDuration;

		private bool useFade;

		private GameObject partyScreenVFX;

		private SkrimCallback externalCallback;

		[Inject]
		public SkrimView view { get; set; }

		[Inject]
		public UIModel model { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public HideSkrimSignal hideSkrim { get; set; }

		[Inject]
		public EnableSkrimButtonSignal enableSkrimSignal { get; set; }

		[Inject]
		public HideItemPopupSignal hideGenericPopupsSignal { get; set; }

		[Inject]
		public PickControllerModel PickControllerModel { get; set; }

		[Inject]
		public OnClickSkrimSignal onClickSkrimSignal { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public IInvokerService invoker { get; set; }

		[Inject]
		public MoveSkrimTopLayerSignal moveSkrimTopLayerSignal { get; set; }

		public override void OnRegister()
		{
			PickControllerModel.IncreaseSkrimCounter();
			enableSkrimSignal.AddListener(EnableSkrimButton);
			moveSkrimTopLayerSignal.AddListener(OnMoveSkirmToTopLevelCallback);
		}

		public override void OnRemove()
		{
			view.ClickButton.ClickedSignal.RemoveListener(Close);
			hideSkrim.RemoveListener(HideSkrim);
			enableSkrimSignal.RemoveListener(EnableSkrimButton);
			moveSkrimTopLayerSignal.RemoveListener(OnMoveSkirmToTopLevelCallback);
		}

		public override void Initialize(GUIArguments args)
		{
			float alpha = view.DarkSkrimImage.color.a;
			bool flag = args.Get<bool>();
			SkrimBehavior skrimBehavior = args.Get<SkrimBehavior>();
			fadeDuration = 0.13f;
			if (skrimBehavior == SkrimBehavior.partyEffectsAndFade)
			{
				useFade = true;
				if (partyScreenVFX == null)
				{
					GameObject original = KampaiResources.Load("cmp_StartPartySkrimEffects") as GameObject;
					GameObject gameObject = UnityEngine.Object.Instantiate(original);
					gameObject.transform.SetParent(base.gameObject.transform, false);
					gameObject.transform.SetSiblingIndex(view.DarkSkrimImage.transform.GetSiblingIndex() + 1);
					partyScreenVFX = gameObject;
				}
			}
			float num = Mathf.Clamp(args.Get<float>(), 0f, 1f);
			if (num > 0f)
			{
				alpha = num;
			}
			hideSkrim.AddListener(HideSkrim);
			enableSkrimSignal.AddListener(EnableSkrimButton);
			view.Init(alpha, useFade);
			SkrimCallback skrimCallback = args.Get<SkrimCallback>();
			if (skrimCallback != null)
			{
				externalCallback = skrimCallback;
				view.ClickButton.ClickedSignal.AddListener(CallbackDelegate);
			}
			else
			{
				view.ClickButton.ClickedSignal.AddListener(Close);
			}
			view.ClickButton.gameObject.SetActive(true);
			view.SetDarkSkrimActive(flag, fadeDuration);
		}

		private void CallbackDelegate()
		{
			PickControllerModel.DecreaseSkrimCounter();
			externalCallback.Callback.Dispatch(this);
		}

		private void OnMoveSkirmToTopLevelCallback(string skrimName)
		{
			if (guiLabel.Equals(skrimName))
			{
				view.transform.SetAsFirstSibling();
			}
		}

		private void EnableSkrimButton(bool enable)
		{
			view.EnableSkrimButton(enable);
		}

		private void Close()
		{
			if (model.PopupAnimationIsPlaying)
			{
				return;
			}
			if (view.genericPopupSkrim)
			{
				hideGenericPopupsSignal.Dispatch();
				HideSkrim(guiLabel);
			}
			else if (model.UIOpen && !model.DisableBack)
			{
				onClickSkrimSignal.Dispatch();
				Action action = model.RemoveTopUI();
				if (!Input.GetKeyDown(KeyCode.Escape))
				{
					if (view.singleSkrimClose)
					{
						action();
						return;
					}
					while (action != null)
					{
						action();
						action = model.RemoveTopUI();
					}
				}
				else if (action != null)
				{
					action();
				}
				else
				{
					HideSkrim(guiLabel);
				}
			}
			else if (!model.DisableBack)
			{
				Action action2 = model.RemoveTopUI();
				if (action2 != null)
				{
					action2();
				}
				HideSkrim(guiLabel);
			}
			else if (!triedToCloseTheNextFrame)
			{
				logger.Debug("Postponing skrim close with empty ui stack");
				triedToCloseTheNextFrame = true;
				invoker.Add(Close);
			}
			else
			{
				logger.Debug("Closing skrim with empty ui stack");
				HideSkrim(guiLabel);
			}
		}

		private void HideSkrim(string skrimName)
		{
			if (guiLabel == null || guiLabel.Equals(skrimName))
			{
				if (useFade)
				{
					view.FadeDarkSkrim(0f, fadeDuration);
				}
				StartCoroutine(DelayFinishHidingSkrim(fadeDuration));
			}
		}

		private IEnumerator DelayFinishHidingSkrim(float duration)
		{
			yield return new WaitForSeconds(duration);
			FinishHidingSkrim();
		}

		private void FinishHidingSkrim()
		{
			gameContext.injectionBinder.GetInstance<ShowHiddenBuildingsSignal>().Dispatch();
			IGUICommand command = guiService.BuildCommand(GUIOperation.Unload, "Skrim", guiLabel);
			guiService.Execute(command);
			PickControllerModel.DecreaseSkrimCounter();
		}

		public void KDispose()
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
