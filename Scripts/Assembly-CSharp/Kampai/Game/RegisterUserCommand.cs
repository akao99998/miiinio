using Ea.Sharkbite.HttpPlugin.Http.Api;
using Kampai.Splash;
using strange.extensions.command.impl;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class RegisterUserCommand : Command
	{
		public const string REGISTER_ENDPOINT = "/rest/user/register";

		[Inject("game.server.host")]
		public string ServerUrl { get; set; }

		[Inject]
		public IDownloadService downloadService { get; set; }

		[Inject]
		public IUserSessionService userSessionService { get; set; }

		[Inject]
		public ISynergyService synergyService { get; set; }

		[Inject]
		public IRequestFactory requestFactory { get; set; }

		public override void Execute()
		{
			UserRegisterRequest userRegisterRequest = new UserRegisterRequest();
			userRegisterRequest.SynergyID = synergyService.userID;
			Signal<IResponse> signal = new Signal<IResponse>();
			signal.AddListener(userSessionService.RegisterRequestCallback);
			downloadService.Perform(requestFactory.Resource(ServerUrl + "/rest/user/register").WithContentType("application/json").WithMethod("POST")
				.WithEntity(userRegisterRequest)
				.WithResponseSignal(signal));
		}
	}
}
