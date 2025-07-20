using System.Collections;
using Kampai.Game;
using Kampai.Main;
using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class PartyMeterMediator : Mediator
	{
		private MinionParty minionParty;

		private IEnumerator fillMeterCoroutine;

		private PostStartPartyBuffTimerSignal postStartPartyBuffTimerSignal;

		[Inject]
		public PartyMeterView view { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IGuestOfHonorService guestService { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		public override void OnRegister()
		{
			postStartPartyBuffTimerSignal = gameContext.injectionBinder.GetInstance<PostStartPartyBuffTimerSignal>();
			postStartPartyBuffTimerSignal.AddListener(OnBuffStarted);
			Init();
		}

		public override void OnRemove()
		{
			RemoveCoroutine(fillMeterCoroutine);
			postStartPartyBuffTimerSignal.RemoveListener(OnBuffStarted);
			postStartPartyBuffTimerSignal = null;
		}

		private void Init()
		{
			minionParty = playerService.GetMinionPartyInstance();
			if (minionParty.IsBuffHappening && !minionParty.IsPartyHappening)
			{
				OnBuffStarted();
			}
		}

		private void OnBuffStarted()
		{
			if (guestService.GetBuffRemainingTime(minionParty) > 0)
			{
				ShowTheCooldownbar();
			}
		}

		private void ShowTheCooldownbar()
		{
			if (guestService.PartyShouldProduceBuff())
			{
				DisplayCooldownMeter(true);
				fillMeterCoroutine = AnimMeter();
				StartCoroutine(fillMeterCoroutine);
			}
		}

		private IEnumerator AnimMeter()
		{
			for (float buffRemaining = guestService.GetBuffRemainingTime(minionParty); buffRemaining >= 0f; buffRemaining = guestService.GetBuffRemainingTime(minionParty))
			{
				UpdateCountDownText(buffRemaining);
				yield return new WaitForEndOfFrame();
			}
			fillMeterCoroutine = null;
			DisplayCooldownMeter(false);
		}

		private void UpdateCountDownText(float timeRemaining)
		{
			view.UpdateCountDownText(string.Format("{0}", UIUtils.FormatTime(timeRemaining, localizationService)));
		}

		private void DisplayCooldownMeter(bool display)
		{
			view.DisplayCooldownMeter(display);
			if (display)
			{
				float num = guestService.GetBuffRemainingTime(minionParty);
				if (num >= 0f)
				{
					UpdateCountDownText(num);
				}
				else
				{
					view.DisplayCooldownMeter(false);
				}
				view.UpdateBuffText(localizationService.GetString("partyBuffMultiplier", guestService.GetCurrentBuffMultipler()));
				BuffDefinition recentBuffDefinition = guestService.GetRecentBuffDefinition();
				view.UpdateBuffIcon(recentBuffDefinition.buffSimpleMask);
			}
		}

		private void RemoveCoroutine(IEnumerator routine)
		{
			if (routine != null)
			{
				StopCoroutine(routine);
				routine = null;
			}
		}
	}
}
