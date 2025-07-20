using System;
using Elevation.Logging;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using Kampai.Common;
using Kampai.Util;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class GooglePlayService : ISocialService, ISynergyService
	{
		private Signal<ISocialService> successSignal;

		private Signal<ISocialService> failureSignal;

		public IKampaiLogger logger = LogManager.GetClassLogger("GooglePlayService") as IKampaiLogger;

		private bool killSwitchFlag;

		private string serverAuthCode;

		private bool attemptToAuthenticate;

		private Action callback;

		[Inject]
		public ILocalPersistanceService localPersistence { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public IUserSessionService userSessionService { get; set; }

		[Inject]
		public IInvokerService invoker { get; set; }

		[Inject]
		public IConfigurationsService configurationsService { get; set; }

		[Inject]
		public GooglePlayServerAuthCodeReceivedSignal googlePlayServerAuthCodeReceivedSignal { get; set; }

		public string LoginSource { get; set; }

		public string ServerAuthCode
		{
			get
			{
				return serverAuthCode;
			}
		}

		public string userID
		{
			get
			{
				return Social.localUser.id;
			}
		}

		public string userName
		{
			get
			{
				return Social.localUser.userName;
			}
		}

		public bool isLoggedIn
		{
			get
			{
				return Social.localUser.authenticated;
			}
		}

		public string accessToken
		{
			get
			{
				if (Social.localUser.authenticated)
				{
					return serverAuthCode;
				}
				return string.Empty;
			}
		}

		public bool isKillSwitchEnabled
		{
			get
			{
				return killSwitchFlag;
			}
		}

		public DateTime tokenExpiry
		{
			get
			{
				return default(DateTime);
			}
		}

		public SocialServices type
		{
			get
			{
				return SocialServices.GOOGLEPLAY;
			}
		}

		public string locKey
		{
			get
			{
				return "AccountTypeGoogle";
			}
		}

		public void RequestServerAuthCode()
		{
			if (!Social.localUser.authenticated)
			{
				logger.Error("Server auth code can be requested when player is authenticated");
				googlePlayServerAuthCodeReceivedSignal.Dispatch(false, this);
			}
			else
			{
				PlayGamesPlatform.Instance.GetServerAuthCode(OnServerAuthCodeRequest);
			}
		}

		public void ResetServerAuthCode()
		{
			serverAuthCode = null;
		}

		public void Init(Signal<ISocialService> successSignal, Signal<ISocialService> failureSignal)
		{
			updateKillSwitchFlag();
			PlayGamesPlatform.DebugLogEnabled = Debug.isDebugBuild;
			PlayGamesPlatform.Activate();
			this.successSignal = successSignal;
			this.failureSignal = failureSignal;
			logger.Debug("GOOGLE PLAY INIT START");
			localPersistence.PutData("SocialInProgress", "False");
			if (!Social.localUser.authenticated)
			{
				logger.Debug("GOOGLE PLAY USER NOT SIGNED IN");
				int dataInt = localPersistence.GetDataInt("GoogleFailCount");
				if (dataInt >= 1)
				{
					logger.Debug("GOOGLE PLAY MAX ATTEMPTS");
					return;
				}
				logger.Debug("GOOGLE PLAY USER NOT SIGNED IN - BELOW MAX ATTEMPTS");
				if (isKillSwitchEnabled)
				{
					failureSignal.Dispatch(this);
				}
				else
				{
					Authenticate();
				}
			}
			else
			{
				logger.Debug("GOOGLE PLAY USER ALREADY LOGGED IN");
				attemptToAuthenticate = true;
				UserSession userSession = userSessionService.UserSession;
				if (serverAuthCode == null && userSession != null && (userSession.SocialIdentities == null || userSession.SocialIdentities.Count == 0))
				{
					AuthSuccess();
				}
				else
				{
					successSignal.Dispatch(this);
				}
			}
		}

		public void Login(Signal<ISocialService> successSignal, Signal<ISocialService> failureSignal, Action callback)
		{
			if (isKillSwitchEnabled)
			{
				failureSignal.Dispatch(this);
				return;
			}
			this.successSignal = successSignal;
			this.failureSignal = failureSignal;
			this.callback = callback;
			Authenticate();
		}

		private void Authenticate()
		{
			serverAuthCode = null;
			attemptToAuthenticate = true;
			localPersistence.PutData("SocialInProgress", "True");
			Social.localUser.Authenticate(OnAuthenticate);
		}

		private void OnAuthenticate(bool success)
		{
			if (success)
			{
				LoginSource = "Authentication";
				AuthSuccess();
			}
			else
			{
				AuthFailure("Social.localUser.Authenticate failed");
			}
		}

		public void AuthSuccess()
		{
			if (!attemptToAuthenticate)
			{
				return;
			}
			if (string.IsNullOrEmpty(serverAuthCode))
			{
				PlayGamesPlatform.Instance.GetServerAuthCode(OnServerAuthCodeRequestOnLogin);
				return;
			}
			attemptToAuthenticate = false;
			localPersistence.PutData("SocialInProgress", "False");
			logger.Debug("GOOGLE PLAY AUTH SUCCESS");
			ILocalUser localUser = Social.localUser;
			logger.Debug("GP PLAYER ID: {0}", localUser.id);
			logger.Debug("GP NAME: {0}", localUser.userName);
			logger.Debug("GP Server auth code: {0}", serverAuthCode);
			localPersistence.PutDataInt("GoogleFailCount", 0);
			successSignal.Dispatch(this);
			if (callback != null)
			{
				callback();
			}
		}

		private void OnServerAuthCodeRequestOnLogin(CommonStatusCodes status, string serverAuthCode)
		{
			if ((status == CommonStatusCodes.Success || status == CommonStatusCodes.SuccessCached) && !string.IsNullOrEmpty(serverAuthCode))
			{
				logger.Debug("OnServerAuthCodeRequestOnLogin: success, status: {0}, serverAuthCode {1}", status, serverAuthCode);
				this.serverAuthCode = serverAuthCode;
				AuthSuccess();
			}
			else
			{
				logger.Debug("OnServerAuthCodeRequestOnLogin: failure, status: {0}", status);
				Logout();
				string error = string.Format("Couldn't fetch Google Play server auth code, status: {0}, serverAuthCode: {1}", status, serverAuthCode ?? "null");
				AuthFailure(error);
			}
		}

		private void OnServerAuthCodeRequest(CommonStatusCodes status, string serverAuthCode)
		{
			if ((status == CommonStatusCodes.Success || status == CommonStatusCodes.SuccessCached) && !string.IsNullOrEmpty(serverAuthCode))
			{
				logger.Debug("OnServerAuthCodeRequest: success, status: {0}, serverAuthCode {1}", status, serverAuthCode);
				this.serverAuthCode = serverAuthCode;
			}
			else
			{
				logger.Debug("Couldn't fetch Google Play server auth code, status: {0}, serverAuthCode: {1}", status, serverAuthCode ?? "null");
				this.serverAuthCode = null;
			}
			bool type = this.serverAuthCode != null;
			googlePlayServerAuthCodeReceivedSignal.Dispatch(type, this);
		}

		public void AuthFailure(string error)
		{
			if (attemptToAuthenticate)
			{
				attemptToAuthenticate = false;
				localPersistence.PutData("SocialInProgress", "False");
				logger.Debug("Fail msg: {0}", error);
				logger.Debug("GOOGLE PLAY AUTH FAILURE");
				int dataInt = localPersistence.GetDataInt("GoogleFailCount");
				localPersistence.PutDataInt("GoogleFailCount", ++dataInt);
				failureSignal.Dispatch(this);
			}
		}

		public void Logout()
		{
			attemptToAuthenticate = false;
			((PlayGamesPlatform)Social.Active).SignOut();
		}

		public void SendLoginTelemetry(string loginLocation)
		{
			telemetryService.Send_Telemetry_EVT_EBISU_LOGIN_GOOGLEPLAY(loginLocation);
		}

		public void updateKillSwitchFlag()
		{
			killSwitchFlag = configurationsService.isKillSwitchOn(KillSwitch.GOOGLEPLAY);
		}

		public void incrementAchievement(string achievementID, float percentComplete)
		{
			Action<bool> action = delegate(bool success)
			{
				logger.Debug("GooglePlayService.incrementAchievement(): achievement: {0} update to percent: {1} success: {2}", achievementID, (int)percentComplete, success);
			};
			PlayGamesPlatform.Instance.IncrementAchievement(achievementID, (int)percentComplete, action);
		}

		public void ShowAchievements()
		{
			Social.ShowAchievementsUI();
		}
	}
}
