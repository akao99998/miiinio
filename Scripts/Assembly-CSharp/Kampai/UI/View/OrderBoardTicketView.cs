using System.Collections;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class OrderBoardTicketView : KampaiView
	{
		public OrderBoardTicketTimerView TicketMeter;

		public ButtonView TicketButton;

		public GameObject CheckMark;

		public GameObject NormalPanel;

		public GameObject PrestigePanel;

		public KampaiImage CharacterImage;

		public GameObject FirstTimePrestigeBadge;

		public Text StarText;

		public Text CurrencyText;

		public int Index;

		public GameObject FunPointIcon;

		public GameObject XpIcon;

		public GameObject BuffActivatedBackground;

		public GameObject BuffActivatedCap;

		private bool isThrobing;

		private Animator ticketAnimator;

		private Animator rootAnimator;

		private IEnumerator hideTicketMeter;

		internal OrderBoardTicket ticketInstance { get; set; }

		internal bool IsSelected { get; set; }

		internal string Title { get; set; }

		internal GetBuffStateSignal getBuffStateSignal { get; set; }

		internal void Init(ILocalizationService localizationService)
		{
			TicketMeter.Init(localizationService);
			IsSelected = false;
			isThrobing = false;
			ticketAnimator = TicketButton.GetComponent<Animator>();
			rootAnimator = GetComponent<Animator>();
			SetRootAnimation(true);
		}

		internal void OnRemove()
		{
			if (hideTicketMeter != null)
			{
				StopCoroutine(hideTicketMeter);
			}
		}

		internal void SetTicketState(bool showTicketButton)
		{
			SetTicketMeterState(showTicketButton);
			TicketButton.gameObject.SetActive(showTicketButton);
			if (showTicketButton)
			{
				SetTicketSelected(false);
			}
		}

		private void SetTicketMeterState(bool showTicketButton)
		{
			if (base.gameObject.activeInHierarchy)
			{
				if (showTicketButton)
				{
					TicketMeter.RushButton.ResetAnim();
					hideTicketMeter = WaitAFrame();
					StartCoroutine(hideTicketMeter);
				}
				else
				{
					TicketMeter.gameObject.SetActive(true);
				}
			}
		}

		private IEnumerator WaitAFrame()
		{
			yield return new WaitForEndOfFrame();
			if (TicketMeter != null)
			{
				TicketMeter.gameObject.SetActive(false);
			}
		}

		internal void SetCharacterImage(Sprite Image, Sprite mask, bool firstTimePrestige)
		{
			CharacterImage.sprite = Image;
			CharacterImage.maskSprite = mask;
			FirstTimePrestigeBadge.SetActive(firstTimePrestige);
		}

		internal void SetTicketSelected(bool isSelected)
		{
			if (IsSelected != isSelected)
			{
				IsSelected = isSelected;
				ticketAnimator.SetBool("Normal", !isSelected);
				ticketAnimator.SetBool("Highlighted", isSelected);
			}
		}

		internal void SetTicketCheckmark(bool show)
		{
			if (show)
			{
				CheckMark.SetActive(true);
			}
			else
			{
				CheckMark.SetActive(false);
			}
		}

		internal void SetTicketClick(bool enabled)
		{
			TicketButton.GetComponent<KampaiButton>().enabled = enabled;
		}

		internal void StartTimer(int index, int duration)
		{
			SetTicketState(false);
			TicketMeter.StartTimer(index, duration);
		}

		internal void SetRootAnimation(bool isOpen)
		{
			rootAnimator.Play((!isOpen) ? "Close" : "Open");
		}

		internal void SetTicketInstance(OrderBoardTicket ti)
		{
			ticketInstance = ti;
			UpdateReward();
		}

		internal void UpdateReward()
		{
			getBuffStateSignal.Dispatch(BuffType.CURRENCY, SetReward);
		}

		private void SetReward(float multiplier)
		{
			int quantity = (int)ticketInstance.TransactionInst.Outputs[1].Quantity;
			int number = Mathf.CeilToInt((float)ticketInstance.TransactionInst.Outputs[0].Quantity * multiplier);
			StarText.text = quantity.ToString();
			CurrencyText.text = UIUtils.FormatLargeNumber(number);
			bool active = (int)(multiplier * 100f) != 100;
			BuffActivatedBackground.SetActive(active);
			BuffActivatedCap.SetActive(active);
		}

		internal bool IsCounting()
		{
			return !TicketButton.gameObject.activeSelf;
		}

		internal void HighlightTicket(bool highlight)
		{
			if (isThrobing != highlight)
			{
				isThrobing = highlight;
				if (highlight)
				{
					TicketButton.GetComponent<Animator>().enabled = false;
					Vector3 originalScale = Vector3.one;
					TweenUtil.Throb(TicketButton.transform, 0.85f, 0.5f, out originalScale);
				}
				else
				{
					TicketButton.GetComponent<Animator>().enabled = true;
					Go.killAllTweensWithTarget(TicketButton.transform);
					TicketButton.transform.localScale = Vector3.one;
				}
			}
		}
	}
}
