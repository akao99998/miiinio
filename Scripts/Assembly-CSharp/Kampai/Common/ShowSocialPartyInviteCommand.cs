using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Game;
using Kampai.Main;
using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.command.impl;
using strange.extensions.signal.impl;

namespace Kampai.Common
{
	public class ShowSocialPartyInviteCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("ShowSocialPartyInviteCommand") as IKampaiLogger;

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public ITimedSocialEventService timedSocialEventService { get; set; }

		[Inject(SocialServices.FACEBOOK)]
		public ISocialService facebookService { get; set; }

		[Inject]
		public NetworkConnectionLostSignal networkConnectionLostSignal { get; set; }

		[Inject]
		public SocialPartyFillOrderSetupUISignal socialPartyFillOrderSetupUISignal { get; set; }

		public override void Execute()
		{
			if (facebookService.isLoggedIn)
			{
				Signal<List<string>> signal = new Signal<List<string>>();
				Signal<List<string>> signal2 = new Signal<List<string>>();
				signal.AddListener(OnFBInviteSuccess);
				signal2.AddListener(OnFBInviteFailure);
				((FacebookService)facebookService).FriendInvite(localService.GetString("socialpartyjointeamdescription"), localService.GetString("socialpartyjointeamtitle"), string.Empty, 4, signal, signal2);
			}
		}

		private void OnFBInviteFailure(IList<string> to)
		{
			logger.Debug("OnFBInviteFailure");
		}

		private void OnFBInviteSuccess(IList<string> to)
		{
			logger.Debug("OnFBInviteSuccess");
			if (to == null)
			{
				logger.Error("No List to send to");
				return;
			}
			int eventId = timedSocialEventService.GetSocialEventStateCached(timedSocialEventService.GetCurrentSocialEvent().ID).EventId;
			long iD = timedSocialEventService.GetSocialEventStateCached(timedSocialEventService.GetCurrentSocialEvent().ID).Team.ID;
			Signal<SocialTeamResponse, ErrorResponse> signal = new Signal<SocialTeamResponse, ErrorResponse>();
			signal.AddListener(OnInviteSuccess);
			timedSocialEventService.InviteFriends(eventId, iD, IdentityType.facebook, to, signal);
		}

		public void OnInviteSuccess(SocialTeamResponse response, ErrorResponse error)
		{
			if (error != null)
			{
				networkConnectionLostSignal.Dispatch();
				return;
			}
			if (response == null || response.Team == null)
			{
				logger.Warning("OnInviteSuccess has no team in the response");
			}
			socialPartyFillOrderSetupUISignal.Dispatch(response.Team);
		}
	}
}
