using Elevation.Logging;
using Kampai.UI;
using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.command.impl;
using strange.extensions.signal.impl;

namespace Kampai.Game.View
{
	public class DisplayPlayerTrainingCommand : Command
	{
		private IKampaiLogger logger = LogManager.GetClassLogger("DisplayPlayerTrainingCommand") as IKampaiLogger;

		[Inject]
		public int playerTrainingDefinitionId { get; set; }

		[Inject]
		public bool openedFromSettingsMenu { get; set; }

		[Inject]
		public Signal<bool> callback { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerTrainingService playerTrainingService { get; set; }

		public override void Execute()
		{
			if (playerTrainingDefinitionId <= 0)
			{
				callback.Dispatch(false);
				return;
			}
			PlayerTrainingDefinition playerTrainingDefinition = definitionService.Get<PlayerTrainingDefinition>(playerTrainingDefinitionId);
			if (playerTrainingDefinition == null)
			{
				logger.Warning("Invalid player training definition id " + playerTrainingDefinitionId);
				callback.Dispatch(false);
				return;
			}
			if (!openedFromSettingsMenu)
			{
				if (playerTrainingDefinition.disableAutomaticDisplay)
				{
					callback.Dispatch(false);
					return;
				}
				if (playerTrainingService.HasSeen(playerTrainingDefinitionId, PlayerTrainingVisiblityType.GAME))
				{
					callback.Dispatch(false);
					return;
				}
			}
			logger.Info("Showing player training definition id: {0}", playerTrainingDefinitionId);
			playerTrainingService.MarkSeen(playerTrainingDefinitionId, PlayerTrainingVisiblityType.GAME);
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Queue, "popup_PlayerTraining");
			GUIArguments args = iGUICommand.Args;
			iGUICommand.skrimScreen = "PlayerTrainingSkrim";
			iGUICommand.darkSkrim = true;
			iGUICommand.disableSkrimButton = true;
			args.Add(playerTrainingDefinitionId);
			args.Add(openedFromSettingsMenu);
			args.Add(callback);
			guiService.Execute(iGUICommand);
		}
	}
}
