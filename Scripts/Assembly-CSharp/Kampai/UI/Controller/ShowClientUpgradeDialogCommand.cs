using System;
using Elevation.Logging;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.UI.Controller
{
	public class ShowClientUpgradeDialogCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("ShowClientUpgradeDialogCommand") as IKampaiLogger;

		[Inject]
		public IGUIService guiService { get; set; }

		public override void Execute()
		{
			string storeUrl = "market://details?id=com.ea.gp.minions";
			ShowUnityDialog(storeUrl);
		}

		private void ShowNativeDialog()
		{
			EventHandler<NativeAlertManager.NativeAlertEventArgs> onClick = null;
			onClick = delegate(object sender, NativeAlertManager.NativeAlertEventArgs eventArgs)
			{
				string buttonText = eventArgs.ButtonText;
				if (buttonText == "OK")
				{
					logger.Info("Going to store...");
					NativeAlertManager.AlertClicked -= onClick;
					Application.OpenURL("market://details?id=com.ea.gp.minions");
				}
			};
			NativeAlertManager.AlertClicked += onClick;
			NativeAlertManager.ShowAlert("Client Upgrade", "A game update is available.  Press OK to download it from the app store.", "OK", "Cancel");
		}

		private void ShowUnityDialog(string storeUrl = "")
		{
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Load, "popup_NudgeUpgrade");
			iGUICommand.skrimScreen = "ClientUpgradeSkrim";
			iGUICommand.darkSkrim = true;
			iGUICommand.Args.Add(storeUrl);
			guiService.Execute(iGUICommand);
		}
	}
}
