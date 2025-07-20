using Elevation.Logging;
using Kampai.Common;
using Kampai.Game.Mignette;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class StartMignetteCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("StartMignetteCommand") as IKampaiLogger;

		[Inject]
		public int BuildingId { get; set; }

		[Inject]
		public DeselectAllMinionsSignal deselectAllMinionsSignal { get; set; }

		[Inject]
		public MignetteGameModel mignetteGameModel { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IQuestService questService { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public PickControllerModel pickControllerModel { get; set; }

		[Inject]
		public HideSkrimSignal hideSkrimSignal { get; set; }

		[Inject]
		public MignetteCollectionService collectionService { get; set; }

		public override void Execute()
		{
			if (mignetteGameModel.IsMignetteActive)
			{
				return;
			}
			hideSkrimSignal.Dispatch("MignetteSkrim");
			pickControllerModel.ForceDisabled = true;
			mignetteGameModel.IsMignetteActive = true;
			mignetteGameModel.BuildingId = BuildingId;
			mignetteGameModel.CurrentGameScore = 0;
			mignetteGameModel.TriggerCooldownOnComplete = true;
			deselectAllMinionsSignal.Dispatch();
			MignetteBuilding byInstanceId = playerService.GetByInstanceId<MignetteBuilding>(BuildingId);
			MignetteBuildingDefinition mignetteBuildingDefinition = byInstanceId.MignetteBuildingDefinition;
			GameObject obj = new GameObject(mignetteBuildingDefinition.ContextRootName);
			GameObjectUtil.AddComponent(obj, mignetteBuildingDefinition.ContextRootName, logger);
			if (mignetteBuildingDefinition.ShowMignetteHUD)
			{
				if (!string.IsNullOrEmpty(mignetteBuildingDefinition.CollectableImage))
				{
					mignetteGameModel.CollectableImage = UIUtils.LoadSpriteFromPath(mignetteBuildingDefinition.CollectableImage);
				}
				if (!string.IsNullOrEmpty(mignetteBuildingDefinition.CollectableImageMask))
				{
					mignetteGameModel.CollectableImageMask = UIUtils.LoadSpriteFromPath(mignetteBuildingDefinition.CollectableImageMask);
				}
				guiService.Execute(GUIOperation.Load, "MignetteHUD");
			}
			collectionService.pendingRewardTransaction = null;
			questService.UpdateAllQuestsWithQuestStepType(QuestStepType.Mignette, QuestTaskTransition.Start, byInstanceId);
		}
	}
}
