using System;
using Elevation.Logging;
using Kampai.Game;
using Kampai.Main;
using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Common
{
	public class ShowSocialPartyFBConnectCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("ShowSocialPartyFBConnectCommand") as IKampaiLogger;

		[Inject]
		public Action<bool> action { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal globalSFX { get; set; }

		[Inject(SocialServices.FACEBOOK)]
		public ISocialService facebookService { get; set; }

		[Inject]
		public ICoppaService coppaService { get; set; }

		public override void Execute()
		{
			logger.Info("Facebook killswitch enabled = {0}", facebookService.isKillSwitchEnabled);
			if (facebookService.isKillSwitchEnabled || coppaService.Restricted())
			{
				action(false);
				return;
			}
			globalSFX.Dispatch("Play_menu_popUp_01");
			IGUICommand iGUICommand = null;
			iGUICommand = guiService.BuildCommand(GUIOperation.Load, "popup_SocialParty_FBConnect");
			iGUICommand.Args.Add(typeof(Action<bool>), action);
			iGUICommand.skrimScreen = "StageSkrimFB";
			iGUICommand.singleSkrimClose = true;
			iGUICommand.darkSkrim = true;
			guiService.Execute(iGUICommand);
		}
	}
}
