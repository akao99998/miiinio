using Ea.Sharkbite.HttpPlugin.Http.Api;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Main;
using Kampai.Splash;
using Kampai.Util;
using Newtonsoft.Json;
using UnityEngine;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class UserSessionService : IUserSessionService
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("UserSessionService") as IKampaiLogger;

		private UserSession Session;

		private Signal loginCallback;

		[Inject]
		public UserRegisteredSignal userRegisteredSignal { get; set; }

		[Inject]
		public IInvokerService invoker { get; set; }

		[Inject]
		public SetupHockeyAppUserSignal setupHockeyAppUser { get; set; }

		[Inject]
		public ILocalPersistanceService LocalPersistService { get; set; }

		[Inject]
		public IDLCService dlcService { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public IDownloadService downloadService { get; set; }

		[Inject]
		public SetupSwrveSignal setupSwrveSignal { get; set; }

		[Inject]
		public SetupSupersonicSignal setupSupersonicSignal { get; set; }

		[Inject]
		public UpdateUserSignal updateUserSignal { get; set; }

		[Inject]
		public ISynergyService synergyService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public UserSessionGrantedSignal userSessionGrantedSignal { get; set; }

		public UserSession UserSession
		{
			get
			{
				return Session;
			}
			set
			{
				Session = value;
			}
		}

		public void LoginRequestCallback(IResponse response)
		{
			TimeProfiler.EndSection("login");
			if (response.Success)
			{
				string body = response.Body;
				UserSession userSession2 = (UserSession = JsonConvert.DeserializeObject<UserSession>(body));
				userSessionGrantedSignal.Dispatch();
				LocalPersistService.PutData("LoadMode", "remote");
				updateSynergyId(userSession2);
				string empty = string.Empty;
				if (response.Headers.ContainsKey("X-Kampai-Remote-IP-Address"))
				{
					empty = response.Headers["X-Kampai-Remote-IP-Address"];
					logger.Info("Client IP address reported as {0}", empty);
				}
				if (loginCallback != null)
				{
					loginCallback.Dispatch();
					return;
				}
				telemetryService.Send_Telemetry_EVT_USER_GAME_LOAD_FUNNEL("70 - User Login", playerService.SWRVEGroup, dlcService.GetDownloadQualityLevel());
				logger.Log(KampaiLogLevel.Info, "User's session ID: {0}", UserSession.SessionID);
				setupSwrveSignal.Dispatch(userSession2.UserID);
				setupSupersonicSignal.Dispatch();
				Signal<IResponse> signal = new Signal<IResponse>();
				signal.AddListener(CatchAuthenticationErrorResponse);
				downloadService.AddGlobalResponseListener(signal);
			}
			else
			{
				invoker.Add(delegate
				{
					logger.Fatal(FatalCode.GS_ERROR_LOGIN, "Response code {0}", response.Code);
				});
			}
		}

		public void RegisterRequestCallback(IResponse response)
		{
			TimeProfiler.EndSection("register");
			if (response.Success)
			{
				string body = response.Body;
				UserIdentity userIdentity = JsonConvert.DeserializeObject<UserIdentity>(body);
				setupHockeyAppUser.Dispatch(userIdentity.UserID);
				userRegisteredSignal.Dispatch(userIdentity);
				return;
			}
			logger.Log(KampaiLogLevel.Error, "RegisterUserCommand error with URL : {0}", response.Request.Uri);
			invoker.Add(delegate
			{
				logger.Fatal(FatalCode.GS_ERROR_LOGIN_3, "Response code {0}", response.Code);
			});
		}

		public void UserUpdateRequestCallback(string synergyID, IResponse response)
		{
			if (response.Success)
			{
				Session.SynergyID = synergyID;
				return;
			}
			logger.Log(KampaiLogLevel.Error, "Failed to update user {0} with synergy ID {1}", UserSession.UserID, synergyID);
		}

		public void setLoginCallback(Signal a)
		{
			loginCallback = a;
		}

		public void OpenURL(string url)
		{
			Application.OpenURL(url);
		}

		private void CatchAuthenticationErrorResponse(IResponse response)
		{
			if (response.Code != 401 || response.Body == null)
			{
				return;
			}
			try
			{
				ErrorResponse errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(response.Body);
				if (errorResponse.Error.Message != null && errorResponse.Error.Message.Equals("Invalid Session"))
				{
					invoker.Add(delegate
					{
						logger.Fatal(FatalCode.SESSION_INVALID);
					});
				}
			}
			catch (JsonSerializationException)
			{
				logger.Debug("UserSessionService:CatchAuthenticationErrorResponse - JsonSerializationException");
			}
			catch (JsonReaderException)
			{
				logger.Debug("UserSessionService:CatchAuthenticationErrorResponse - JsonReaderException");
			}
		}

		private void updateSynergyId(UserSession session)
		{
			string userID = synergyService.userID;
			string synergyID = session.SynergyID;
			if (string.IsNullOrEmpty(synergyID) && !string.IsNullOrEmpty(userID))
			{
				updateUserSignal.Dispatch(userID);
			}
			if (!string.IsNullOrEmpty(synergyID) && !synergyID.Equals(userID))
			{
				logger.Debug("SynergyIds don't match oops, changing them, old SynergyID = {0}  new Synergy ID = {1} ", NimbleBridge_SynergyIdManager.GetComponent().GetSynergyId(), synergyID);
				using (NimbleBridge_SynergyIdManager.GetComponent().Login(synergyID, session.UserID))
				{
				}
			}
		}
	}
}
