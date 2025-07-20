using Ea.Sharkbite.HttpPlugin.Http.Api;
using Elevation.Logging;
using Kampai.Splash;
using Kampai.Util;
using strange.extensions.command.impl;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class LoginUserCommand : Command
	{
		public const string LOGIN_ENDPOINT = "/rest/user/session";

		public IKampaiLogger logger = LogManager.GetClassLogger("LoginUserCommand") as IKampaiLogger;

		[Inject]
		public ILocalPersistanceService LocalPersistService { get; set; }

		[Inject]
		public IEncryptionService encryptionService { get; set; }

		[Inject]
		public RegisterUserSignal RegisterUserSignal { get; set; }

		[Inject("game.server.host")]
		public string ServerUrl { get; set; }

		[Inject]
		public IUserSessionService UserSessionService { get; set; }

		[Inject]
		public IDownloadService downloadService { get; set; }

		[Inject]
		public IRequestFactory requestFactory { get; set; }

		public override void Execute()
		{
			logger.EventStart("LoginUserCommand.Execute");
			string text = LocalPersistService.GetData("UserID");
			string text2 = LocalPersistService.GetData("AnonymousSecret");
			string plainText = string.Empty;
			if (encryptionService.TryDecrypt(text2, "Kampai!", out plainText))
			{
				text2 = plainText;
			}
			string text3 = LocalPersistService.GetData("AnonymousID");
			if (encryptionService.TryDecrypt(text3, "Kampai!", out plainText))
			{
				text3 = plainText;
			}
			if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(text2) || string.IsNullOrEmpty(text3))
			{
				TimeProfiler.StartSection("register");
				logger.Log(KampaiLogLevel.Info, true, "Registering new anonymous user");
				RegisterUserSignal.Dispatch();
			}
			else
			{
				TimeProfiler.StartSection("login");
				UserLoginRequest userLoginRequest = new UserLoginRequest();
				userLoginRequest.UserID = text;
				userLoginRequest.AnonymousSecret = text2;
				userLoginRequest.IdentityID = text3;
				Signal<IResponse> signal = new Signal<IResponse>();
				signal.AddListener(UserSessionService.LoginRequestCallback);
				string text4 = ServerUrl + "/rest/user/session";
				downloadService.Perform(requestFactory.Resource(text4).WithContentType("application/json").WithMethod("POST")
					.WithEntity(userLoginRequest)
					.WithResponseSignal(signal));
				logger.Debug("LoginUserCommand: Using url {0}", text4);
			}
			logger.EventStop("LoginUserCommand.Execute");
		}
	}
}
