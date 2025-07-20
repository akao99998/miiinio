using Kampai.Common;
using Kampai.Game.Mignette;
using Kampai.UI.View;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class StopMignetteCommand : Command
	{
		[Inject]
		public ShowHUDSignal showHUDSignal { get; set; }

		[Inject]
		public ShowStoreSignal showStoreSignal { get; set; }

		[Inject]
		public PickControllerModel pickControllerModel { get; set; }

		[Inject]
		public MignetteGameModel mignetteGameModel { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public DestroyMignetteContextSignal destroyMignetteContextSignal { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		public override void Execute()
		{
			showHUDSignal.Dispatch(true);
			showStoreSignal.Dispatch(true);
			pickControllerModel.ForceDisabled = false;
			int buildingId = mignetteGameModel.BuildingId;
			MignetteBuilding byInstanceId = playerService.GetByInstanceId<MignetteBuilding>(buildingId);
			MignetteBuildingDefinition mignetteBuildingDefinition = byInstanceId.MignetteBuildingDefinition;
			if (mignetteBuildingDefinition.ShowMignetteHUD)
			{
				guiService.Execute(GUIOperation.Unload, "MignetteHUD");
			}
			destroyMignetteContextSignal.Dispatch();
			mignetteGameModel.IsMignetteActive = false;
			string localizedKey = mignetteBuildingDefinition.LocalizedKey;
			int currentGameScore = mignetteGameModel.CurrentGameScore;
			float elapsedTime = mignetteGameModel.ElapsedTime;
			telemetryService.Send_Telemetry_EVT_MINI_GAME_PLAYED(localizedKey, currentGameScore, elapsedTime, (int)mignetteBuildingDefinition.XPRewardFactor);
		}
	}
}
