using System.Collections;
using Elevation.Logging;
using Kampai.Game;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class SkipPartyButtonMediator : Mediator
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("SkipPartyButtonMediator") as IKampaiLogger;

		private IEnumerator skipButtonCooldownMeterCoroutine;

		private MinionParty party;

		[Inject]
		public SkipPartyButtonView view { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public IGuestOfHonorService guestService { get; set; }

		[Inject]
		public ShowMinionPartySkipButtonSignal showButtonSignal { get; set; }

		[Inject]
		public RevealLevelUpUISignal revealLevelUpSignal { get; set; }

		[Inject]
		public SetGrindCurrencySignal setGrindCurrencySignal { get; set; }

		[Inject]
		public SetPremiumCurrencySignal setPremiumCurrencySignal { get; set; }

		[Inject]
		public SetStorageCapacitySignal setStorageSignal { get; set; }

		public override void OnRegister()
		{
			view.SkipButton.ClickedSignal.AddListener(SkipPartyButtonClick);
			showButtonSignal.AddListener(DisplaySkipButton);
			Init();
		}

		public override void OnRemove()
		{
			RemoveCoroutine(skipButtonCooldownMeterCoroutine);
			view.SkipButton.ClickedSignal.RemoveListener(SkipPartyButtonClick);
			showButtonSignal.RemoveListener(DisplaySkipButton);
		}

		private void Init()
		{
			party = playerService.GetMinionPartyInstance();
			view.ShowSkipPartyButtonView(false);
		}

		private void DisplaySkipButton(bool isEnabled)
		{
			view.ShowSkipPartyButtonView(isEnabled);
			if (isEnabled)
			{
				party.PartyPreSkip = false;
				StartSkipCooldownMeter();
			}
		}

		private void SkipPartyButtonClick()
		{
			logger.Debug("Dispatching the EndMinionPartySignal from the skip party button");
			party.PartyPreSkip = true;
			gameContext.injectionBinder.GetInstance<EndMinionPartySignal>().Dispatch(true);
			setGrindCurrencySignal.Dispatch();
			setPremiumCurrencySignal.Dispatch();
			setStorageSignal.Dispatch();
			revealLevelUpSignal.Dispatch();
			StopSkipCooldownTime();
			DisplaySkipButton(false);
		}

		private void StartSkipCooldownMeter()
		{
			MinionPartyDefinition definition = party.Definition;
			float num = definition.GetPartyDuration(guestService.PartyShouldProduceBuff());
			num += 3.34f;
			skipButtonCooldownMeterCoroutine = AnimSkipCooldown(num);
			StartCoroutine(skipButtonCooldownMeterCoroutine);
		}

		private IEnumerator AnimSkipCooldown(float duration)
		{
			float totalPartyTime = duration;
			while (duration >= 0f)
			{
				view.UpdateSkipMeterTime(duration, totalPartyTime);
				yield return new WaitForEndOfFrame();
				duration -= Time.deltaTime;
			}
			skipButtonCooldownMeterCoroutine = null;
			DisplaySkipButton(false);
		}

		public void StopSkipCooldownTime()
		{
			if (skipButtonCooldownMeterCoroutine != null)
			{
				StopCoroutine(skipButtonCooldownMeterCoroutine);
			}
			skipButtonCooldownMeterCoroutine = null;
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
