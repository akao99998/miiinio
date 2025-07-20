using System;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Main;
using Kampai.Util;
using Newtonsoft.Json;
using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class GameCenterService : ISocialService, ISynergyService
	{
		private Signal<ISocialService> successSignal;

		private Signal<ISocialService> failureSignal;

		private GameCenterAuthToken token;

		private bool killSwitchFlag;

		public IKampaiLogger logger = LogManager.GetClassLogger("GameCenterService") as IKampaiLogger;

		[Inject(MainElement.MANAGER_PARENT)]
		public GameObject managers { get; set; }

		[Inject]
		public GameCenterAuthTokenCompleteSignal authCompleteSignal { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public IConfigurationsService configurationsService { get; set; }

		public string LoginSource { get; set; }

		public string userID
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
				return SocialServices.GAMECENTER;
			}
		}

		public bool isLoggedIn
		{
			get
			{
				return false;
			}
		}

		public bool isKillSwitchEnabled
		{
			get
			{
				return killSwitchFlag;
			}
		}

		public string accessToken
		{
			get
			{
				logger.Debug("accessToken = {0}", JsonConvert.SerializeObject(token));
				return JsonConvert.SerializeObject(token);
			}
		}

		public string userName
		{
			get
			{
				return string.Empty;
			}
		}

		public DateTime tokenExpiry
		{
			get
			{
				return default(DateTime);
			}
		}

		public string locKey
		{
			get
			{
				return "AccountTypeGameCenter";
			}
		}

		public void Init(Signal<ISocialService> successSignal, Signal<ISocialService> failureSignal)
		{
			logger.Debug("Game Center Login");
			updateKillSwitchFlag();
			this.successSignal = successSignal;
			this.failureSignal = failureSignal;
			if (isKillSwitchEnabled)
			{
				failureSignal.Dispatch(this);
			}
		}

		private void AuthenticationFail(string message)
		{
			RemoveAuthSubscribers();
			logger.Debug("Game Center Login Failed: " + message);
			failureSignal.Dispatch(this);
		}

		private void AuthenticationSuccess()
		{
		}

		private void GenerateIdentityFail(string message)
		{
			RemoveIdentitySubscribers();
			logger.Debug("Game Center Generate Identity Failed: " + message);
		}

		private void GenerateIdentitySuccess(Dictionary<string, string> identity)
		{
			logger.Debug("Game Center Generate Identity Success");
			GameObject gameObject = managers.FindChild("GameCenterAuthManager");
			if (gameObject != null)
			{
				authCompleteSignal.AddOnce(delegate
				{
					GameCenterAuthToken gameCenterAuthToken = new GameCenterAuthToken
					{
						publicKeyUrl = identity["publicKeyUrl"],
						signature = identity["signature"],
						salt = identity["salt"],
						timestamp = identity["timestamp"],
						bundleId = Native.BundleIdentifier,
						playerId = string.Empty
					};
					token = gameCenterAuthToken;
					successSignal.Dispatch(this);
				});
			}
		}

		private void RemoveAuthSubscribers()
		{
		}

		private void RemoveIdentitySubscribers()
		{
		}

		public void Login(Signal<ISocialService> successSignal, Signal<ISocialService> failureSignal, Action callback)
		{
			Init(successSignal, failureSignal);
		}

		public void Logout()
		{
		}

		public void updateKillSwitchFlag()
		{
			killSwitchFlag = configurationsService.isKillSwitchOn(KillSwitch.GAMECENTER);
			logger.Info("Game Center killswitch {0}", killSwitchFlag);
		}

		public void SendLoginTelemetry(string loginLocation)
		{
			telemetryService.Send_Telemetry_EVT_EBISU_LOGIN_GAMECENTER(loginLocation);
		}

		public void incrementAchievement(string achievementID, float percentComplete)
		{
		}

		public void ShowAchievements()
		{
		}
	}
}
