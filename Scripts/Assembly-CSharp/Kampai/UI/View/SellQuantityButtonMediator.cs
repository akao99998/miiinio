using System.Collections;
using Kampai.Common;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class SellQuantityButtonMediator : Mediator
	{
		private float buttonHeldTime;

		private int prevValue;

		[Inject]
		public SellQuantityButtonView view { get; set; }

		[Inject]
		public AppPauseSignal pauseSignal { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			view.OnPointerDownSignal.AddListener(OnPointerDown);
			view.OnPointerUpSignal.AddListener(OnPointerUp);
			pauseSignal.AddListener(OnPause);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			view.OnPointerDownSignal.RemoveListener(OnPointerDown);
			view.OnPointerUpSignal.RemoveListener(OnPointerUp);
			pauseSignal.RemoveListener(OnPause);
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			StartCoroutine(InitWait());
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			StopCoroutine(Wait());
			StopCoroutine(InitWait());
		}

		private IEnumerator InitWait()
		{
			if (view.IsPriceButton)
			{
				StartCoroutine(WaitToPlaySound(view.PRICE_INIT_WAIT_TIME));
				yield return new WaitForSeconds(view.PRICE_INIT_WAIT_TIME);
				buttonHeldTime = 0f;
				prevValue = view.MinValue;
			}
			else
			{
				StartCoroutine(WaitToPlaySound(view.COUNT_WAIT_TIME));
			}
			view.heldDownSignal.Dispatch(1);
			if (view.IsHeldDown)
			{
				StartCoroutine(Wait());
			}
		}

		private IEnumerator Wait()
		{
			Button btn = view.GetComponentInParent<Button>();
			if (btn == null || !btn.interactable)
			{
				yield break;
			}
			if (view.IsPriceButton)
			{
				yield return null;
				if (view.IsHeldDown)
				{
					buttonHeldTime += Time.deltaTime;
					int value = Mathf.FloorToInt(Mathf.Lerp(view.MinValue, view.MaxValue, buttonHeldTime / view.PRICE_MAX_WAIT_TIME));
					if (value != prevValue)
					{
						view.heldDownSignal.Dispatch(value - prevValue);
						prevValue = value;
					}
					StartCoroutine(Wait());
				}
			}
			else
			{
				yield return new WaitForSeconds(view.COUNT_WAIT_TIME);
				if (view.IsHeldDown)
				{
					view.heldDownSignal.Dispatch(1);
					StartCoroutine(Wait());
				}
			}
		}

		private IEnumerator WaitToPlaySound(float timeSpacing)
		{
			Button btn = view.GetComponentInParent<Button>();
			if (btn == null || !btn.interactable)
			{
				yield break;
			}
			do
			{
				if (view.IsHeldDown)
				{
					view.playSFXSignal.Dispatch("Play_button_click_01");
				}
				yield return new WaitForSeconds(timeSpacing);
			}
			while (view.IsHeldDown && btn.interactable);
		}

		private void OnPause()
		{
			if (view.IsHeldDown)
			{
				StopCoroutine(WaitToPlaySound(0f));
			}
			view.IsHeldDown = false;
		}
	}
}
