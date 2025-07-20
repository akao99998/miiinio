using System;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class SetupPushNotificationsCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("SetupPushNotificationsCommand") as IKampaiLogger;

		[Inject]
		public IPushNotificationService pushNotificationService { get; set; }

		[Inject]
		public IUserSessionService userSessionService { get; set; }

		[Inject]
		public ICoppaService coppaService { get; set; }

		public override void Execute()
		{
			logger.Debug("[PN] SetupPushNotificationsCommand: prepare args for PN service");
			UserSession userSession = userSessionService.UserSession;
			if (userSession == null)
			{
				logger.Error("[PN] User session is null");
				return;
			}
			string userID = userSession.UserID;
			if (string.IsNullOrEmpty(userID))
			{
				logger.Error("[PN] User alias is invalid");
				return;
			}
			DateTime birthdate;
			if (!coppaService.GetBirthdate(out birthdate))
			{
				logger.Info("[PN] Coppa birthdate is unknown at the moment");
				return;
			}
			logger.Debug("[PN] Start push notification service.");
			pushNotificationService.Start(userID, birthdate.Year, birthdate.Month);
		}
	}
}
