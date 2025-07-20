using System;
using Kampai.Game;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.UI.View
{
	public class ShowFacebookConnectPopupCommand : Command
	{
		[Inject]
		public IGUIService guiService { get; set; }

		[Inject(SocialServices.FACEBOOK)]
		public ISocialService facebookService { get; set; }

		[Inject]
		public SocialLoginSignal socialLoginSignal { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		public override void Execute()
		{
			if (!facebookService.isLoggedIn)
			{
				SocialSettingsDefinition socialSettingsDefinition = definitionService.Get<SocialSettingsDefinition>(1000009022);
				if (socialSettingsDefinition != null && socialSettingsDefinition.ShowFacebookConnectPopup)
				{
					IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Load, "popup_SocialParty_FBConnect");
					iGUICommand.skrimScreen = "StageSkrimFB";
					iGUICommand.darkSkrim = true;
					iGUICommand.singleSkrimClose = true;
					guiService.Execute(iGUICommand);
				}
				else
				{
					socialLoginSignal.Dispatch(facebookService, new Boxed<Action>(null));
				}
			}
		}
	}
}
