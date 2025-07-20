using System;
using Elevation.Logging;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class SocialLoginCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("SocialLoginCommand") as IKampaiLogger;

		[Inject]
		public ISocialService socialService { get; set; }

		[Inject]
		public Boxed<Action> callback { get; set; }

		[Inject]
		public SocialLoginSuccessSignal loginSuccess { get; set; }

		[Inject]
		public SocialLoginFailureSignal loginFailure { get; set; }

		public override void Execute()
		{
			logger.Debug("Social Login Command Called With {0}", socialService.type.ToString());
			if (!socialService.isLoggedIn)
			{
				socialService.Login(loginSuccess, loginFailure, callback.Value);
			}
			else
			{
				logger.Debug("Already logged on, you must log out first");
			}
		}
	}
}
