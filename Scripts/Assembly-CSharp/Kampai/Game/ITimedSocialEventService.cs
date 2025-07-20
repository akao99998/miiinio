using System.Collections.Generic;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public interface ITimedSocialEventService
	{
		TimedSocialEventDefinition GetCurrentSocialEvent();

		TimedSocialEventDefinition GetSocialEvent(int id);

		void ClearCache();

		void GetSocialEventState(int eventID, Signal<SocialTeamResponse, ErrorResponse> resultSignal);

		SocialTeamResponse GetSocialEventStateCached(int eventID);

		void CreateSocialTeam(int eventID, Signal<SocialTeamResponse, ErrorResponse> resultSignal);

		void JoinSocialTeam(int eventID, long teamID, Signal<SocialTeamResponse, ErrorResponse> resultSignal);

		void LeaveSocialTeam(int eventID, long teamID, Signal<SocialTeamResponse, ErrorResponse> resultSignal);

		void InviteFriends(int eventID, long teamID, IdentityType identityType, IList<string> externalIDs, Signal<SocialTeamResponse, ErrorResponse> resultSignal);

		void RejectInvitation(int eventID, long teamID, Signal<SocialTeamResponse, ErrorResponse> resultSignal);

		void FillOrder(int eventID, long teamID, int orderID, Signal<SocialTeamResponse, ErrorResponse> resultSignal);

		void ClaimReward(int eventID, long teamID, Signal<SocialTeamResponse, ErrorResponse> resultSignal);

		void GetSocialTeams(int eventID, IList<long> teamIds, Signal<Dictionary<long, SocialTeam>> resultSignal);

		void setRewardCutscene(bool cutscene);

		bool isRewardCutscene();

		IList<int> GetPastEventsWithUnclaimedReward();

		TimedSocialEventDefinition GetNextSocialEvent();
	}
}
