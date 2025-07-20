using System;
using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class EndPartyBuffTimerWithCallbackCommand : Command
	{
		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public DisplayDiscoGlobeSignal displayDiscoGlobeSignal { get; set; }

		[Inject]
		public Boxed<Action> onCompleteEvent { get; set; }

		[Inject]
		public IGuestOfHonorService guestService { get; set; }

		public override void Execute()
		{
			MinionParty minionPartyInstance = playerService.GetMinionPartyInstance();
			int timePassedSinceBuffStarts = timeService.CurrentTime() - minionPartyInstance.BuffStartTime;
			int buffStartTime = minionPartyInstance.BuffStartTime;
			minionPartyInstance.BuffStartTime = 0;
			minionPartyInstance.NewBuffStartTime = 0;
			minionPartyInstance.IsBuffHappening = false;
			guestService.StopBuff(timePassedSinceBuffStarts, buffStartTime);
			playerService.UpdateMinionPartyPointValues();
			if (onCompleteEvent.Value != null)
			{
				onCompleteEvent.Value();
			}
			else
			{
				displayDiscoGlobeSignal.Dispatch(false);
			}
		}
	}
}
