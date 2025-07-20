using System;
using Kampai.Common;
using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class StartMinionPartyIntroCommand : Command
	{
		[Inject]
		public TriggerPhilPartyStartSignal triggerPhilPartyStartSignal { get; set; }

		[Inject]
		public ShowHUDSignal showHudSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public CameraMoveToCustomPositionSignal customCameraPositionSignal { get; set; }

		[Inject(UIElement.CONTEXT)]
		public ICrossContextCapable uiContext { get; set; }

		[Inject]
		public ShowStoreSignal showStoreSignal { get; set; }

		[Inject]
		public TeleportMinionsToTownForPartySignal teleportMinionsToTownForPartySignal { get; set; }

		[Inject]
		public PrepareLeisureMinionsForPartySignal prepareLeisureMinionsForPartySignal { get; set; }

		[Inject]
		public PrepareTaskingMinionsForPartySignal prepareTaskingMinionsForPartySignal { get; set; }

		[Inject]
		public AddCharacterToPartyStageSignal addCharacterToPartyStageSignal { get; set; }

		[Inject]
		public KillDiscoGlobeSignal killDiscoGlobeSignal { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public IPartyService partyService { get; set; }

		[Inject]
		public PreLoadPartyAssetsSignal preLoadPartyAssetsSignal { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public IGuestOfHonorService guestService { get; set; }

		[Inject]
		public CheckMinionPartyLevelSignal checkMinionPartyLevelSignal { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		public override void Execute()
		{
			if (timeEventService.HasEventID(80000))
			{
				timeEventService.RushEvent(80000);
			}
			int quantity = (int)playerService.GetQuantity(StaticItem.XP_ID);
			checkMinionPartyLevelSignal.Dispatch(true);
			MinionParty minionPartyInstance = playerService.GetMinionPartyInstance();
			minionPartyInstance.IsPartyHappening = true;
			if (guestService.PartyShouldProduceBuff())
			{
				minionPartyInstance.NewBuffStartTime = timeService.CurrentTime() + minionPartyInstance.Definition.GetPartyDuration(true);
				minionPartyInstance.PartyStartTier = minionPartyInstance.DeterminePartyTier(playerService.GetQuantity(StaticItem.LEVEL_ID));
				guestService.UpdateAndStoreGuestOfHonorCooldowns();
			}
			prepareLeisureMinionsForPartySignal.Dispatch();
			prepareTaskingMinionsForPartySignal.Dispatch();
			addCharacterToPartyStageSignal.Dispatch();
			showHudSignal.Dispatch(false);
			uiContext.injectionBinder.GetInstance<HideAllWayFindersSignal>().Dispatch();
			showStoreSignal.Dispatch(false);
			killDiscoGlobeSignal.Dispatch();
			preLoadPartyAssetsSignal.Dispatch();
			customCameraPositionSignal.Dispatch(60000, new Boxed<Action>(OnPanComplete));
			SendTelemetry(minionPartyInstance, quantity);
		}

		private void OnPanComplete()
		{
			teleportMinionsToTownForPartySignal.Dispatch();
			triggerPhilPartyStartSignal.Dispatch();
		}

		private void SendTelemetry(MinionParty minionParty, int totalPartyPoints)
		{
			int quantity = (int)playerService.GetQuantity(StaticItem.LEVEL_ID);
			BuffDefinition recentBuffDefinition = guestService.GetRecentBuffDefinition(true);
			string guestOfHonor = ((guestService.CurrentGuestOfHonor == null) ? "Guest Of Honor is null" : guestService.CurrentGuestOfHonor.LocalizedKey);
			bool isInspiredParty = partyService.IsInspirationParty(quantity, minionParty.CurrentPartyIndex);
			telemetryService.Send_Telemetry_EVT_MINION_PARTY_STARTED(totalPartyPoints, (recentBuffDefinition == null) ? "Buff is null" : recentBuffDefinition.buffType.ToString(), guestOfHonor, isInspiredParty);
		}
	}
}
