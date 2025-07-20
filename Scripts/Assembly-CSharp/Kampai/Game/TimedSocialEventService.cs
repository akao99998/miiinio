using System;
using System.Collections.Generic;
using Ea.Sharkbite.HttpPlugin.Http.Api;
using Elevation.Logging;
using Kampai.Splash;
using Kampai.Util;
using Newtonsoft.Json;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class TimedSocialEventService : ITimedSocialEventService
	{
		public const string SOCIAL_EVENT_TEAM_BY_USER_ENDPOINT = "/rest/tse/event/{0}/team/user/{1}";

		public const string SOCIAL_EVENT_INVITE_FRIENDS_ENDPOINT = "/rest/tse/event/{0}/team/{1}/user/{2}/invite";

		public const string SOCIAL_EVENT_REJECT_INVITE_ENDPOINT = "/rest/tse/event/{0}/team/{1}/user/{2}/reject";

		public const string SOCIAL_EVENT_JOIN_TEAM_ENDPOINT = "/rest/tse/event/{0}/team/{1}/user/{2}/join";

		public const string SOCIAL_EVENT_LEAVE_TEAM_ENDPOINT = "/rest/tse/event/{0}/team/{1}/user/{2}/leave";

		public const string SOCIAL_EVENT_FILL_ORDER_ENDPOINT = "/rest/tse/event/{0}/team/{1}/user/{2}/order";

		public const string SOCIAL_EVENT_CLAIM_REWARD_ENDPOINT = "/rest/tse/event/{0}/team/{1}/user/{2}/reward";

		public const string SOCIAL_EVENT_TEAMS_ENDPOINT = "/rest/tse/event/{0}/teams";

		public const int CLAIM_REWARD_LIMIT = 259200;

		public IKampaiLogger logger = LogManager.GetClassLogger("TimedSocialEventService") as IKampaiLogger;

		private Dictionary<int, SocialTeamResponse> socialEventCache;

		private bool rewardCutscene;

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public IUserSessionService userSessionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject("game.server.host")]
		public string ServerUrl { get; set; }

		[Inject]
		public IDownloadService downloadService { get; set; }

		[Inject]
		public IRequestFactory requestFactory { get; set; }

		public TimedSocialEventService()
		{
			socialEventCache = new Dictionary<int, SocialTeamResponse>();
		}

		public void ClearCache()
		{
			socialEventCache.Clear();
		}

		public TimedSocialEventDefinition GetCurrentSocialEvent()
		{
			IList<TimedSocialEventDefinition> all = definitionService.GetAll<TimedSocialEventDefinition>();
			int num = timeService.CurrentTime();
			foreach (TimedSocialEventDefinition item in all)
			{
				if (item.StartTime <= num && item.FinishTime >= num)
				{
					return item;
				}
			}
			return null;
		}

		public TimedSocialEventDefinition GetNextSocialEvent()
		{
			TimedSocialEventDefinition result = null;
			int num = int.MaxValue;
			IList<TimedSocialEventDefinition> all = definitionService.GetAll<TimedSocialEventDefinition>();
			int num2 = timeService.CurrentTime();
			foreach (TimedSocialEventDefinition item in all)
			{
				int startTime = item.StartTime;
				int num3 = startTime - num2;
				if (startTime > num2 && num3 < num)
				{
					num = num3;
					result = item;
				}
			}
			return result;
		}

		public TimedSocialEventDefinition GetSocialEvent(int id)
		{
			IList<TimedSocialEventDefinition> all = definitionService.GetAll<TimedSocialEventDefinition>();
			if (all == null)
			{
				logger.Warning("GetSocialEvent not found");
				return null;
			}
			foreach (TimedSocialEventDefinition item in all)
			{
				if (item.ID == id)
				{
					return item;
				}
			}
			logger.Warning("GetSocialEvent not found with id {0}", id);
			return null;
		}

		public void GetSocialEventState(int eventID, Signal<SocialTeamResponse, ErrorResponse> resultSignal)
		{
			UserSession userSession = userSessionService.UserSession;
			if (userSession == null)
			{
				logger.Error("User is not logged in. Can't get social team for event {0}", eventID);
				return;
			}
			string userID = userSession.UserID;
			Signal<IResponse> signal = new Signal<IResponse>();
			signal.AddListener(delegate(IResponse response)
			{
				OnGetTeamResponse(resultSignal, response);
			});
			downloadService.Perform(requestFactory.Resource(ServerUrl + string.Format("/rest/tse/event/{0}/team/user/{1}", eventID, userID)).WithHeaderParam("user_id", userSession.UserID).WithHeaderParam("session_key", userSession.SessionID)
				.WithResponseSignal(signal));
		}

		public SocialTeamResponse GetSocialEventStateCached(int eventID)
		{
			if (socialEventCache != null)
			{
				if (socialEventCache.ContainsKey(eventID))
				{
					return socialEventCache[eventID];
				}
				logger.Error("Social event not found in cache {0}", eventID);
			}
			return null;
		}

		public void CreateSocialTeam(int eventID, Signal<SocialTeamResponse, ErrorResponse> resultSignal)
		{
			UserSession userSession = userSessionService.UserSession;
			if (userSession == null)
			{
				logger.Error("User is not logged in. Can't get social team for event {0}", eventID);
				return;
			}
			string userID = userSession.UserID;
			Signal<IResponse> signal = new Signal<IResponse>();
			signal.AddListener(delegate(IResponse response)
			{
				OnGetTeamResponse(resultSignal, response);
			});
			downloadService.Perform(requestFactory.Resource(ServerUrl + string.Format("/rest/tse/event/{0}/team/user/{1}", eventID, userID)).WithHeaderParam("user_id", userSession.UserID).WithHeaderParam("session_key", userSession.SessionID)
				.WithContentType("application/json")
				.WithMethod("POST")
				.WithResponseSignal(signal));
		}

		public void JoinSocialTeam(int eventID, long teamID, Signal<SocialTeamResponse, ErrorResponse> resultSignal)
		{
			UserSession userSession = userSessionService.UserSession;
			if (userSession == null)
			{
				logger.Error("User is not logged in. Can't get social team for event {0}", eventID);
				return;
			}
			string userID = userSession.UserID;
			Signal<IResponse> signal = new Signal<IResponse>();
			signal.AddListener(delegate(IResponse response)
			{
				OnGetTeamResponse(resultSignal, response);
			});
			downloadService.Perform(requestFactory.Resource(ServerUrl + string.Format("/rest/tse/event/{0}/team/{1}/user/{2}/join", eventID, teamID, userID)).WithHeaderParam("user_id", userSession.UserID).WithHeaderParam("session_key", userSession.SessionID)
				.WithContentType("application/json")
				.WithMethod("POST")
				.WithResponseSignal(signal));
		}

		public void LeaveSocialTeam(int eventID, long teamID, Signal<SocialTeamResponse, ErrorResponse> resultSignal)
		{
			UserSession userSession = userSessionService.UserSession;
			if (userSession == null)
			{
				logger.Error("User is not logged in. Can't get social team for event {0}", eventID);
				return;
			}
			string userID = userSession.UserID;
			Signal<IResponse> signal = new Signal<IResponse>();
			signal.AddListener(delegate(IResponse response)
			{
				OnGetTeamResponse(resultSignal, response);
			});
			downloadService.Perform(requestFactory.Resource(ServerUrl + string.Format("/rest/tse/event/{0}/team/{1}/user/{2}/leave", eventID, teamID, userID)).WithHeaderParam("user_id", userSession.UserID).WithHeaderParam("session_key", userSession.SessionID)
				.WithContentType("application/json")
				.WithMethod("POST")
				.WithResponseSignal(signal));
		}

		public void InviteFriends(int eventID, long teamID, IdentityType identityType, IList<string> externalIDs, Signal<SocialTeamResponse, ErrorResponse> resultSignal)
		{
			UserSession userSession = userSessionService.UserSession;
			if (userSession == null)
			{
				logger.Error("User is not logged in. Can't get social team for event {0}", eventID);
				return;
			}
			string userID = userSession.UserID;
			Signal<IResponse> signal = new Signal<IResponse>();
			signal.AddListener(delegate(IResponse response)
			{
				OnGetTeamResponse(resultSignal, response);
			});
			InviteFriendsRequest inviteFriendsRequest = new InviteFriendsRequest();
			inviteFriendsRequest.IdentityType = identityType;
			inviteFriendsRequest.ExternalIds = externalIDs;
			downloadService.Perform(requestFactory.Resource(ServerUrl + string.Format("/rest/tse/event/{0}/team/{1}/user/{2}/invite", eventID, teamID, userID)).WithHeaderParam("user_id", userSession.UserID).WithHeaderParam("session_key", userSession.SessionID)
				.WithContentType("application/json")
				.WithMethod("POST")
				.WithEntity(inviteFriendsRequest)
				.WithResponseSignal(signal));
		}

		public void RejectInvitation(int eventID, long teamID, Signal<SocialTeamResponse, ErrorResponse> resultSignal)
		{
			UserSession userSession = userSessionService.UserSession;
			if (userSession == null)
			{
				logger.Error("User is not logged in. Can't get social team for event {0}", eventID);
				return;
			}
			string userID = userSession.UserID;
			Signal<IResponse> signal = new Signal<IResponse>();
			signal.AddListener(delegate(IResponse response)
			{
				OnGetTeamResponse(resultSignal, response);
			});
			downloadService.Perform(requestFactory.Resource(ServerUrl + string.Format("/rest/tse/event/{0}/team/{1}/user/{2}/reject", eventID, teamID, userID)).WithHeaderParam("user_id", userSession.UserID).WithHeaderParam("session_key", userSession.SessionID)
				.WithContentType("application/json")
				.WithMethod("POST")
				.WithResponseSignal(signal));
		}

		public void FillOrder(int eventID, long teamID, int orderID, Signal<SocialTeamResponse, ErrorResponse> resultSignal)
		{
			UserSession userSession = userSessionService.UserSession;
			if (userSession == null)
			{
				logger.Error("User is not logged in. Can't get social team for event {0}", eventID);
				return;
			}
			string userID = userSession.UserID;
			Signal<IResponse> signal = new Signal<IResponse>();
			signal.AddListener(delegate(IResponse response)
			{
				OnFillOrderResponse(resultSignal, response);
			});
			FillOrderRequest fillOrderRequest = new FillOrderRequest();
			fillOrderRequest.OrderID = orderID;
			downloadService.Perform(requestFactory.Resource(ServerUrl + string.Format("/rest/tse/event/{0}/team/{1}/user/{2}/order", eventID, teamID, userID)).WithHeaderParam("user_id", userSession.UserID).WithHeaderParam("session_key", userSession.SessionID)
				.WithContentType("application/json")
				.WithMethod("POST")
				.WithEntity(fillOrderRequest)
				.WithResponseSignal(signal));
		}

		public void ClaimReward(int eventID, long teamID, Signal<SocialTeamResponse, ErrorResponse> resultSignal)
		{
			UserSession userSession = userSessionService.UserSession;
			if (userSession == null)
			{
				logger.Error("User is not logged in. Can't get social team for event {0}", eventID);
				return;
			}
			string userID = userSession.UserID;
			Signal<IResponse> signal = new Signal<IResponse>();
			signal.AddListener(delegate(IResponse response)
			{
				OnGetTeamResponse(resultSignal, response);
			});
			downloadService.Perform(requestFactory.Resource(ServerUrl + string.Format("/rest/tse/event/{0}/team/{1}/user/{2}/reward", eventID, teamID, userID)).WithHeaderParam("user_id", userSession.UserID).WithHeaderParam("session_key", userSession.SessionID)
				.WithContentType("application/json")
				.WithMethod("POST")
				.WithResponseSignal(signal));
		}

		public void GetSocialTeams(int eventID, IList<long> teamIds, Signal<Dictionary<long, SocialTeam>> resultSignal)
		{
			UserSession userSession = userSessionService.UserSession;
			Signal<IResponse> signal = new Signal<IResponse>();
			signal.AddListener(delegate(IResponse response)
			{
				OnGetTeamsResponse(resultSignal, response);
			});
			GetTeamsRequest getTeamsRequest = new GetTeamsRequest();
			getTeamsRequest.TeamIDs = teamIds;
			downloadService.Perform(requestFactory.Resource(ServerUrl + string.Format("/rest/tse/event/{0}/teams", eventID)).WithHeaderParam("user_id", userSession.UserID).WithHeaderParam("session_key", userSession.SessionID)
				.WithContentType("application/json")
				.WithMethod("POST")
				.WithEntity(getTeamsRequest)
				.WithResponseSignal(signal));
		}

		private void OnGetTeamResponse(Signal<SocialTeamResponse, ErrorResponse> resultSignal, IResponse response)
		{
			if (response.Success)
			{
				string body = response.Body;
				SocialTeamResponse socialTeamResponse = JsonConvert.DeserializeObject<SocialTeamResponse>(body, new JsonConverter[1]
				{
					new SocialTeamConverter(definitionService)
				});
				socialEventCache[socialTeamResponse.EventId] = socialTeamResponse;
				UpdateClaimRewardForPastEvent(socialTeamResponse);
				if (resultSignal != null)
				{
					resultSignal.Dispatch(socialTeamResponse, null);
				}
			}
			else
			{
				logger.Warning("Failed to get social team", response.Code);
				if (resultSignal != null)
				{
					ErrorResponse errorResponse = GetErrorResponse(response);
					resultSignal.Dispatch(null, errorResponse);
				}
			}
		}

		private void OnFillOrderResponse(Signal<SocialTeamResponse, ErrorResponse> resultSignal, IResponse response)
		{
			string body = response.Body;
			if (response.Success)
			{
				SocialTeamResponse socialTeamResponse = JsonConvert.DeserializeObject<SocialTeamResponse>(body, new JsonConverter[1]
				{
					new SocialTeamConverter(definitionService)
				});
				socialEventCache[socialTeamResponse.EventId] = socialTeamResponse;
				if (resultSignal != null)
				{
					resultSignal.Dispatch(socialTeamResponse, null);
				}
			}
			else
			{
				logger.Warning("Failed to fill order in social event");
				if (resultSignal != null)
				{
					ErrorResponse errorResponse = GetErrorResponse(response);
					resultSignal.Dispatch(null, errorResponse);
				}
			}
		}

		private void OnGetTeamsResponse(Signal<Dictionary<long, SocialTeam>> resultSignal, IResponse response)
		{
			if (response.Success)
			{
				if (resultSignal != null)
				{
					string body = response.Body;
					Dictionary<long, SocialTeam> type = JsonConvert.DeserializeObject<Dictionary<long, SocialTeam>>(body, new JsonConverter[1]
					{
						new SocialTeamConverter(definitionService)
					});
					resultSignal.Dispatch(type);
				}
			}
			else
			{
				logger.Warning("Failed to get list of social teams", response.Code);
				if (resultSignal != null)
				{
					resultSignal.Dispatch(null);
				}
			}
		}

		private ErrorResponse GetErrorResponse(IResponse response)
		{
			string body = response.Body;
			ErrorResponse errorResponse = null;
			try
			{
				errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(body);
			}
			catch (Exception)
			{
				errorResponse = new ErrorResponse();
				ErrorResponseContent errorResponseContent = new ErrorResponseContent();
				errorResponseContent.ResponseCode = response.Code;
				errorResponseContent.Code = 0;
				errorResponseContent.Message = "unknown";
				errorResponse.Error = errorResponseContent;
			}
			return errorResponse;
		}

		public TimedSocialEventDefinition GetCurrentTimedSocialEventDefinition()
		{
			IList<TimedSocialEventDefinition> all = definitionService.GetAll<TimedSocialEventDefinition>();
			int num = timeService.CurrentTime();
			foreach (TimedSocialEventDefinition item in all)
			{
				int startTime = item.StartTime;
				int finishTime = item.FinishTime;
				if (num >= startTime && num < finishTime)
				{
					return item;
				}
			}
			return null;
		}

		public void setRewardCutscene(bool cutscene)
		{
			rewardCutscene = cutscene;
		}

		public bool isRewardCutscene()
		{
			return rewardCutscene;
		}

		public IList<int> GetPastEventsWithUnclaimedReward()
		{
			List<int> list = new List<int>();
			List<int> list2 = new List<int>();
			int num = timeService.CurrentTime();
			IList<TimedSocialEventDefinition> all = definitionService.GetAll<TimedSocialEventDefinition>();
			foreach (TimedSocialEventDefinition item in all)
			{
				int iD = item.ID;
				if (item.FinishTime >= num)
				{
					list2.Add(iD);
				}
				else
				{
					if (num - item.FinishTime >= 259200)
					{
						continue;
					}
					list2.Add(iD);
					switch (playerService.GetSocialClaimReward(iD))
					{
					case SocialClaimRewardItem.ClaimState.EVENT_COMPLETED_REWARD_NOT_CLAIMED:
						list.Add(iD);
						if (!socialEventCache.ContainsKey(iD))
						{
							GetSocialEventState(iD, null);
						}
						break;
					case SocialClaimRewardItem.ClaimState.UNKNOWN:
						GetSocialEventState(iD, null);
						break;
					}
				}
			}
			playerService.CleanupSocialClaimReward(list2);
			return list;
		}

		private void UpdateClaimRewardForPastEvent(SocialTeamResponse teamResponse)
		{
			TimedSocialEventDefinition timedSocialEventDefinition = definitionService.Get<TimedSocialEventDefinition>(teamResponse.EventId);
			int num = timeService.CurrentTime();
			if (timedSocialEventDefinition == null || timedSocialEventDefinition.FinishTime > num)
			{
				return;
			}
			if (teamResponse.UserEvent == null)
			{
				playerService.AddSocialClaimReward(teamResponse.EventId, SocialClaimRewardItem.ClaimState.EVENT_NOT_COMPLETED);
			}
			else if (teamResponse.UserEvent.RewardClaimed)
			{
				playerService.AddSocialClaimReward(teamResponse.EventId, SocialClaimRewardItem.ClaimState.REWARD_CLAIMED);
			}
			else if (teamResponse.Team != null && teamResponse.Team.OrderProgress != null)
			{
				if (teamResponse.Team.OrderProgress.Count == timedSocialEventDefinition.Orders.Count)
				{
					playerService.AddSocialClaimReward(teamResponse.EventId, SocialClaimRewardItem.ClaimState.EVENT_COMPLETED_REWARD_NOT_CLAIMED);
				}
				else
				{
					playerService.AddSocialClaimReward(teamResponse.EventId, SocialClaimRewardItem.ClaimState.EVENT_NOT_COMPLETED);
				}
			}
		}
	}
}
