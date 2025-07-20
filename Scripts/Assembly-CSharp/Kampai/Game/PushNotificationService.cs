using System;
using Elevation.Logging;
using Kampai.Util;

namespace Kampai.Game
{
	public class PushNotificationService : IPushNotificationService
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("PushNotificationService") as IKampaiLogger;

		private static bool pushTngInitialized;

		public void Start(string userAlias, int birthdateYear, int birthdateMonth)
		{
			if (!pushTngInitialized)
			{
				double dateOfBirthSecondsUnixEpoch = GetDateOfBirthSecondsUnixEpoch(birthdateYear, birthdateMonth);
				bool flag = !GameConstants.StaticConfig.PUSHTNG_USE_PROD_APNS_CERT;
				logger.Debug("[PN] PushNotificationService: call NimbleBridge_PushTNG.Start: userAlias: {0}, dateOfBirth Unix epoch: {1}(year-month: {2}-{3}), iOSSanbox: {4}", userAlias, dateOfBirthSecondsUnixEpoch, birthdateYear, birthdateMonth, flag);
				NimbleBridge_PushTNG.GetComponent().Start(userAlias, dateOfBirthSecondsUnixEpoch, flag);
				pushTngInitialized = true;
			}
			else
			{
				logger.Debug("[PN] PushNotificationService: skip NimbleBridge_PushTNG.Start because it is already initialized.");
			}
		}

		private double GetDateOfBirthSecondsUnixEpoch(int birthdateYear, int birthdateMonth)
		{
			DateTime dateTime = new DateTime(birthdateYear, birthdateMonth, 1);
			int num = (int)(dateTime - new DateTime(1970, 1, 1)).TotalSeconds;
			return num;
		}
	}
}
