using Elevation.Logging;
using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class CheckUpgradeCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("CheckUpgradeCommand") as IKampaiLogger;

		[Inject]
		public IConfigurationsService configService { get; set; }

		[Inject]
		public IUserSessionService userSessionService { get; set; }

		[Inject]
		public ShowClientUpgradeDialogSignal showClientUpgradeDialogSignal { get; set; }

		[Inject]
		public ShowForcedClientUpgradeScreenSignal showForcedClientUpgradeScreenSignal { get; set; }

		[Inject]
		public IClientVersion clientVersionService { get; set; }

		public override void Execute()
		{
			ConfigurationDefinition configurations = configService.GetConfigurations();
			string clientVersion = clientVersionService.GetClientVersion();
			logger.Info("CheckUpgradeCommand for '{0}'", clientVersion);
			if (GameConstants.StaticConfig.DEBUG_ENABLED && clientVersion.Equals("0"))
			{
				return;
			}
			if (clientVersion == null)
			{
				logger.Info("CheckUpgradeCommand: Client version is null");
			}
			if (configurations.isAllowed)
			{
				logger.Log(KampaiLogLevel.Info, "CheckUpgradeCommand: This client version is allowed... carry on");
			}
			else if (configurations.isNudgeAllowed)
			{
				logger.Info("CheckUpgradeCommand: Client version is a nudge upgrade version...");
				int num = configurations.nudgeUpgradePercentage;
				if (num == 0)
				{
					num = 100;
				}
				UserSession userSession = userSessionService.UserSession;
				if (userSession != null)
				{
					string s = userSession.UserID.Substring(userSession.UserID.Length - 2, 2);
					int result = 0;
					if (!int.TryParse(s, out result))
					{
						result = 0;
					}
					if (result <= num)
					{
						logger.Log(KampaiLogLevel.Info, "CheckUpgradeCommand: Going to Nudge player to update");
						showClientUpgradeDialogSignal.Dispatch();
					}
				}
			}
			else
			{
				logger.Log(KampaiLogLevel.Info, "CheckUpgradeCommand: Going to force client to upgrade");
				showForcedClientUpgradeScreenSignal.Dispatch();
			}
		}
	}
}
