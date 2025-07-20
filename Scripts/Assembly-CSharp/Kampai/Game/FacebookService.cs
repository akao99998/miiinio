using System;
using System.Collections;
using System.Collections.Generic;
using Elevation.Logging;
using Facebook.Unity;
using Kampai.Common;
using Kampai.Util;
using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class FacebookService : ISocialService
	{
		private Signal<ISocialService> _initSuccessSignal;

		private Signal<ISocialService> _initFailSignal;

		private Signal<ISocialService> _loginSuccessSignal;

		private Signal<ISocialService> _loginFailureSignal;

		private Signal<List<string>> _inviteSignalSuccess;

		private Signal<List<string>> _inviteSignalFailure;

		private Signal<ISocialService> _getFriendsSignalSuccess;

		private Signal<ISocialService> _getFriendsSignalFailure;

		public IKampaiLogger logger = LogManager.GetClassLogger("FacebookService") as IKampaiLogger;

		private Signal<ISocialService> success = new Signal<ISocialService>();

		private Signal<ISocialService> failure = new Signal<ISocialService>();

		private bool killSwitchFlag;

		[Inject]
		public ILocalPersistanceService localPersistence { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public IConfigurationsService configurationsService { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		public Dictionary<string, FBUser> friends { get; set; }

		public Dictionary<string, Texture> userPictures { get; set; }

		public string LoginSource { get; set; }

		public bool isLoggedIn
		{
			get
			{
				return FB.IsLoggedIn;
			}
		}

		public bool isKillSwitchEnabled
		{
			get
			{
				return killSwitchFlag;
			}
		}

		public string userID
		{
			get
			{
				return (!isLoggedIn) ? string.Empty : AccessToken.CurrentAccessToken.UserId;
			}
		}

		public string userName
		{
			get
			{
				return string.Empty;
			}
		}

		public SocialServices type
		{
			get
			{
				return SocialServices.FACEBOOK;
			}
		}

		public string accessToken
		{
			get
			{
				return (!isLoggedIn) ? string.Empty : AccessToken.CurrentAccessToken.TokenString;
			}
		}

		public DateTime tokenExpiry
		{
			get
			{
				return (!isLoggedIn) ? DateTime.MinValue : AccessToken.CurrentAccessToken.ExpirationTime;
			}
		}

		public string locKey
		{
			get
			{
				return "AccountTypeFacebook";
			}
		}

		public void Login(Signal<ISocialService> successSignal, Signal<ISocialService> failureSignal, Action callback)
		{
			logger.Debug("Facebook: Login Source = {0}", LoginSource ?? "N/A");
			if (!isKillSwitchEnabled)
			{
				if (localPersistence.GetData("SocialInProgress") != "True")
				{
					localPersistence.PutData("SocialInProgress", "True");
					_loginSuccessSignal = successSignal;
					_loginFailureSignal = failureSignal;
					routineRunner.StartCoroutine(LogInWithReadPermissions("public_profile", "user_friends"));
				}
				else
				{
					logger.Warning("Facebook: Ignoring login attempt as another one is already in progress.");
				}
			}
			else
			{
				failureSignal.Dispatch(this);
			}
		}

		private IEnumerator LogInWithReadPermissions(params string[] permissions)
		{
			yield return null;
			FB.LogInWithReadPermissions(permissions, AuthCallback);
		}

		public void Init(Signal<ISocialService> successSignal, Signal<ISocialService> failureSignal)
		{
			logger.Debug("Facebook: Init Called");
			updateKillSwitchFlag();
			_initSuccessSignal = successSignal;
			_initFailSignal = failureSignal;
			friends = new Dictionary<string, FBUser>();
			userPictures = new Dictionary<string, Texture>();
			if (!FB.IsInitialized)
			{
				FB.Init(GameConstants.Facebook.APP_ID, true, true, true, false, true, null, "en_US", null, SetInit);
			}
			else
			{
				SetInit();
			}
			localPersistence.PutData("SocialInProgress", "False");
		}

		private void downloadFriendsSuccess(ISocialService service)
		{
			success.RemoveListener(downloadFriendsSuccess);
			failure.RemoveListener(downloadFriendsFailure);
		}

		private void downloadFriendsFailure(ISocialService service)
		{
			success.RemoveListener(downloadFriendsSuccess);
			failure.RemoveListener(downloadFriendsFailure);
		}

		public void FriendInvite(string message, string title, string data, int maxRecipients, Signal<List<string>> successSignal, Signal<List<string>> failureSignal)
		{
			SendRequest(message, title, data, null, maxRecipients, successSignal, failureSignal);
		}

		public void SendRequest(string message, string title, string data, IList<string> ids, Signal<List<string>> successSignal, Signal<List<string>> failureSignal)
		{
			SendRequest(message, title, data, ids, 100, successSignal, failureSignal);
		}

		public void SendRequestToAll(string message, string title, string data, Signal<List<string>> successSignal, Signal<List<string>> failureSignal)
		{
			SendRequest(message, title, data, (friends == null) ? null : friends.Keys, 100, successSignal, failureSignal);
		}

		public void SendRequest(string message, string title, string data, IEnumerable<string> ids, int maxRecipients, Signal<List<string>> successSignal, Signal<List<string>> failureSignal)
		{
			logger.Debug("Facebook: FriendInvite");
			if (!isKillSwitchEnabled && isLoggedIn)
			{
				_inviteSignalSuccess = successSignal;
				_inviteSignalFailure = failureSignal;
				FB.AppRequest(message, ids, null, null, maxRecipients, data, title, AppRequestCallback);
				return;
			}
			if (!isLoggedIn)
			{
				logger.Error("Facebook: FriendInvite failed. Please log in first.");
			}
			if (failureSignal != null)
			{
				failureSignal.Dispatch(null);
			}
		}

		private void AppRequestCallback(IAppRequestResult result)
		{
			string text = ((result == null) ? null : result.Error);
			List<string> list = ((result == null || result.To == null) ? null : new List<string>(result.To));
			if (result == null || result.Cancelled || !string.IsNullOrEmpty(text))
			{
				if (result == null)
				{
					logger.Error("Facebook: AppRequest with no result");
				}
				else
				{
					logger.Error("Facebook: AppRequest failure = {0}", (!string.IsNullOrEmpty(text)) ? text : "Cancelled");
				}
				if (_inviteSignalFailure != null)
				{
					_inviteSignalFailure.Dispatch(list);
				}
				return;
			}
			logger.Debug("Facebook: AppRequest result = {0}", result.RawResult);
			IDictionary<string, object> resultDictionary = result.ResultDictionary;
			if (resultDictionary == null || !resultDictionary.ContainsKey("request") || list == null || list.Count == 0)
			{
				logger.Error("Facebook: AppRequest cancelled due to bad response.");
				if (_inviteSignalFailure != null)
				{
					_inviteSignalFailure.Dispatch(list);
				}
				return;
			}
			logger.Debug("Facebook: AppRequest succeeded for = {0}", string.Join(",", list.ToArray()));
			if (_inviteSignalSuccess != null)
			{
				_inviteSignalSuccess.Dispatch(list);
			}
		}

		public void GetUserInfo()
		{
			FB.API("/me?fields=id,first_name", HttpMethod.GET, GetUserInfoCallback);
		}

		private void GetUserInfoCallback(IGraphResult result)
		{
			string text = ((result == null) ? null : result.Error);
			if (result == null || result.Cancelled || !string.IsNullOrEmpty(text))
			{
				if (result == null)
				{
					logger.Error("Facebook: GetUserInfo with no result");
					return;
				}
				logger.Error("Facebook: GetUserInfo failure = {0}", (!string.IsNullOrEmpty(text)) ? text : "Cancelled");
			}
			else
			{
				logger.Debug("Facebook: GetUserInfo result = {0}", result.RawResult);
			}
		}

		public void DownloadFriends(int friendLimit, Signal<ISocialService> success, Signal<ISocialService> failure)
		{
			logger.Debug("Facebook: DownloadFriends");
			if (!isKillSwitchEnabled)
			{
				_getFriendsSignalFailure = failure;
				_getFriendsSignalSuccess = success;
				FB.API(string.Format("me/friends?fields=name,id&limit={0}&access_token={1}", friendLimit, AccessToken.CurrentAccessToken.TokenString), HttpMethod.GET, DownloadFriendsCallback);
			}
			else
			{
				failure.Dispatch(this);
			}
		}

		private void DownloadFriendsCallback(IGraphResult result)
		{
			string text = ((result == null) ? null : result.Error);
			if (result == null || result.Cancelled || !string.IsNullOrEmpty(text))
			{
				if (result == null)
				{
					logger.Error("Facebook: DownloadFriends with no result");
				}
				else
				{
					logger.Error("Facebook: DownloadFriends failure = {0}", (!string.IsNullOrEmpty(text)) ? text : "Cancelled");
				}
				if (_getFriendsSignalFailure != null)
				{
					_getFriendsSignalFailure.Dispatch(this);
				}
				return;
			}
			logger.Debug("Facebook: DownloadFriends result = {0}", result.RawResult);
			IDictionary<string, object> resultDictionary = result.ResultDictionary;
			List<object> list = ((resultDictionary == null || !resultDictionary.ContainsKey("data")) ? null : (resultDictionary["data"] as List<object>));
			if (list == null)
			{
				logger.Error("Facebook: DownloadFriends result doesn't have any valid data.");
				if (_getFriendsSignalFailure != null)
				{
					_getFriendsSignalFailure.Dispatch(this);
				}
				return;
			}
			foreach (Dictionary<string, object> item in list)
			{
				string text2 = item["id"] as string;
				if (!friends.ContainsKey(text2))
				{
					friends.Add(text2, new FBUser(item["name"] as string, text2));
				}
			}
			if (_getFriendsSignalSuccess != null)
			{
				_getFriendsSignalSuccess.Dispatch(this);
			}
		}

		public FBUser GetFriend(string fbid)
		{
			if (friends != null && friends.ContainsKey(fbid))
			{
				return friends[fbid];
			}
			return null;
		}

		public IEnumerator DownloadUserPicture(string id, Signal<string> callback = null)
		{
			string url = string.Format("https://graph.facebook.com/{0}/picture?width=256&height=256", id);
			logger.Info("Facebook: Download user picture URL: {0}", url);
			WWW www = new WWW(url);
			yield return www;
			if (!string.IsNullOrEmpty(www.error) || www.texture == null)
			{
				logger.Warning("Facebook: Download picture failed with error {0}", www.error);
			}
			else
			{
				Texture texture = www.texture;
				if (texture.width > 8 && texture.height > 8)
				{
					userPictures[id] = texture;
					if (friends.ContainsKey(id))
					{
						friends[id].SetTexture(texture, Vector2.zero);
					}
				}
			}
			if (callback != null)
			{
				callback.Dispatch(id);
			}
		}

		public Texture GetUserPicture(string id)
		{
			return (userPictures == null || !userPictures.ContainsKey(id)) ? null : userPictures[id];
		}

		public void Logout()
		{
			logger.Debug("Facebook: Logout");
			FB.LogOut();
			friends.Clear();
		}

		private void SetInit()
		{
			logger.Debug("Facebook: Set Init Called");
			if (FB.IsInitialized)
			{
				FB.ActivateApp();
				_initSuccessSignal.Dispatch(this);
				logger.Info("Is Logged In Facebook: {0}", isLoggedIn.ToString());
				if (isLoggedIn)
				{
					AccessToken currentAccessToken = AccessToken.CurrentAccessToken;
					logger.Info("Facebook UserID: {0}", currentAccessToken.UserId);
					logger.Info("Access Token: {0}", currentAccessToken.TokenString);
					logger.Info("Access Expiry: {0} ", currentAccessToken.ExpirationTime.ToString());
					success.AddListener(downloadFriendsSuccess);
					failure.AddListener(downloadFriendsFailure);
					DownloadFriends(100, success, failure);
				}
			}
			else
			{
				_initFailSignal.Dispatch(this);
			}
		}

		private void AuthCallback(ILoginResult result)
		{
			localPersistence.PutData("SocialInProgress", "False");
			logger.Debug("Facebook: Auth result = {0}", result.RawResult);
			if (isLoggedIn)
			{
				AccessToken currentAccessToken = AccessToken.CurrentAccessToken;
				logger.Debug("********** FB **********");
				logger.Debug(currentAccessToken.UserId);
				logger.Debug(currentAccessToken.TokenString);
				_loginSuccessSignal.Dispatch(this);
				GetUserInfo();
				success.AddListener(downloadFriendsSuccess);
				failure.AddListener(downloadFriendsFailure);
				DownloadFriends(100, success, failure);
			}
			else
			{
				string text = ((result == null) ? null : result.Error);
				logger.Error("Facebook: Auth failure = {0}", (!string.IsNullOrEmpty(text) || !result.Cancelled) ? text : "Cancelled");
				FB.Init(GameConstants.Facebook.APP_ID, true, true, true, false, true, null, "en_US", null, SetInit);
				localPersistence.PutData("SocialInProgress", "False");
				_loginFailureSignal.Dispatch(this);
			}
		}

		public void updateKillSwitchFlag()
		{
			killSwitchFlag = configurationsService.isKillSwitchOn(KillSwitch.FACEBOOK);
		}

		public void SendLoginTelemetry(string loginLocation)
		{
			telemetryService.Send_Telemetry_EVT_EBISU_LOGIN_FACEBOOK(loginLocation, LoginSource);
		}

		public void incrementAchievement(string achievementID, float percentComplete)
		{
		}

		public void ShowAchievements()
		{
		}
	}
}
