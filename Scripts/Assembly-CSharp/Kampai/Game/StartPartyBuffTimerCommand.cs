using System;
using System.Collections.Generic;
using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class StartPartyBuffTimerCommand : Command
	{
		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public EndPartyBuffTimerWithCallbackSignal endPartyBuffTimerWithCallbackSignal { get; set; }

		[Inject]
		public PostStartPartyBuffTimerSignal postStartPartyBuffTimerSignal { get; set; }

		[Inject]
		public UnloadPartyAssetsSignal unloadPartyAssetsSignal { get; set; }

		[Inject(UIElement.CONTEXT)]
		public ICrossContextCapable uiContext { get; set; }

		[Inject]
		public IGuestOfHonorService guestService { get; set; }

		public override void Execute()
		{
			if (playerService.IsMinionPartyUnlocked())
			{
				MinionParty minionPartyInstance = playerService.GetMinionPartyInstance();
				if (minionPartyInstance.IsBuffHappening)
				{
					endPartyBuffTimerWithCallbackSignal.Dispatch(new Boxed<Action>(StartBuff));
				}
				else if (guestService.PartyShouldProduceBuff())
				{
					StartBuff();
				}
				else
				{
					CleanUpPartyWithoutBuff();
				}
			}
		}

		private void StartBuff()
		{
			MinionParty minionPartyInstance = playerService.GetMinionPartyInstance();
			int buffStartTime = timeService.CurrentTime();
			MinionPartyDefinition definition = minionPartyInstance.Definition;
			minionPartyInstance.BuffStartTime = buffStartTime;
			minionPartyInstance.NewBuffStartTime = 0;
			minionPartyInstance.IsBuffHappening = true;
			minionPartyInstance.PartyType = MinionPartyType.LUAU;
			minionPartyInstance.PartyStartTier = minionPartyInstance.DeterminePartyTier(playerService.GetQuantity(StaticItem.LEVEL_ID));
			PartyMeterTierDefinition partyMeterTierDefinition = definition.partyMeterDefinition.Tiers[minionPartyInstance.PartyStartTier];
			guestService.StartBuff(partyMeterTierDefinition.Duration);
			postStartPartyBuffTimerSignal.Dispatch();
		}

		private void CleanUpPartyWithoutBuff()
		{
			List<Minion> instancesByType = playerService.GetInstancesByType<Minion>();
			foreach (Minion item in instancesByType)
			{
				item.IsInMinionParty = false;
			}
			playerService.UpdateMinionPartyPointValues();
			uiContext.injectionBinder.GetInstance<DisplayDiscoGlobeSignal>().Dispatch(false);
			unloadPartyAssetsSignal.Dispatch();
		}
	}
}
