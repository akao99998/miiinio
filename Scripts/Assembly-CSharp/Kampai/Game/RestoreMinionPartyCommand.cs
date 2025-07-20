using System.Collections;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class RestoreMinionPartyCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("RestoreMinionPartyCommand") as IKampaiLogger;

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public IGuestOfHonorService guestService { get; set; }

		[Inject]
		public DisplayDiscoGlobeSignal displayDiscoGlobeSignal { get; set; }

		[Inject]
		public PreLoadPartyAssetsSignal preloadPartyAssetsSignal { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public LoadPartyAssetsSignal loadPartyAssetsSignal { get; set; }

		public override void Execute()
		{
			MinionParty minionPartyInstance = playerService.GetMinionPartyInstance();
			minionPartyInstance.ResolveBuffStartTime();
			minionPartyInstance.IsPartyHappening = false;
			int buffStartTime = minionPartyInstance.BuffStartTime;
			if (buffStartTime == 0)
			{
				return;
			}
			int currentBuffDuration = guestService.GetCurrentBuffDuration();
			minionPartyInstance.NewBuffStartTime = 0;
			if (buffStartTime + currentBuffDuration > timeService.CurrentTime())
			{
				minionPartyInstance.IsBuffHappening = true;
				List<int> lastGuestsOfHonorPrestigeIDs = minionPartyInstance.lastGuestsOfHonorPrestigeIDs;
				if (lastGuestsOfHonorPrestigeIDs.Count == 1)
				{
					guestService.SelectGuestOfHonor(lastGuestsOfHonorPrestigeIDs[0]);
				}
				else if (lastGuestsOfHonorPrestigeIDs.Count == 2)
				{
					guestService.SelectGuestOfHonor(lastGuestsOfHonorPrestigeIDs[0], lastGuestsOfHonorPrestigeIDs[1]);
				}
				else
				{
					logger.Error("No stored guests of honor");
				}
				PartyMeterTierDefinition partyMeterTierDefinition = minionPartyInstance.Definition.partyMeterDefinition.Tiers[minionPartyInstance.PartyStartTier];
				guestService.StartBuff(partyMeterTierDefinition.Duration);
				preloadPartyAssetsSignal.Dispatch();
				loadPartyAssetsSignal.Dispatch();
				DisplayDiscoBall(true);
			}
			else
			{
				HandleBuffOver(minionPartyInstance, currentBuffDuration);
			}
		}

		private void HandleBuffOver(MinionParty minionParty, int buffDuration)
		{
			guestService.StopBuff(buffDuration, minionParty.BuffStartTime);
			minionParty.IsBuffHappening = false;
			minionParty.BuffStartTime = 0;
			DisplayDiscoBall(false);
		}

		private void DisplayDiscoBall(bool display)
		{
			routineRunner.StartCoroutine(DisplayDiscoBallDelayed(display));
		}

		private IEnumerator DisplayDiscoBallDelayed(bool display)
		{
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			displayDiscoGlobeSignal.Dispatch(display);
		}
	}
}
