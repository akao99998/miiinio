using Elevation.Logging;
using Kampai.Game.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class PanAndOpenQuestCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("PanAndOpenQuestCommand") as IKampaiLogger;

		[Inject]
		public int questID { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		[Inject(GameElement.NAMED_CHARACTER_MANAGER)]
		public GameObject namedCharacterManager { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public CameraAutoMoveSignal autoMoveSignal { get; set; }

		[Inject]
		public IMasterPlanQuestService masterPlanQuestService { get; set; }

		public override void Execute()
		{
			Quest questByInstanceId = masterPlanQuestService.GetQuestByInstanceId(questID);
			if (questByInstanceId == null)
			{
				logger.Error("No quest found with id: {0}", questID);
				return;
			}
			Vector3 zero = Vector3.zero;
			BuildingManagerView component = buildingManager.GetComponent<BuildingManagerView>();
			BuildingObject buildingObject = component.GetBuildingObject(questByInstanceId.QuestIconTrackedInstanceId);
			if (buildingObject != null)
			{
				zero = buildingObject.transform.position;
			}
			else
			{
				NamedCharacterManagerView component2 = namedCharacterManager.GetComponent<NamedCharacterManagerView>();
				NamedCharacterObject namedCharacterObject = component2.Get(questByInstanceId.QuestIconTrackedInstanceId);
				if (!(namedCharacterObject != null))
				{
					logger.Warning("Unsupported type of object to pan to");
					return;
				}
				zero = namedCharacterObject.transform.position;
			}
			logger.Info("Pan and open quest id: {0} - {1}", questID, zero);
			ScreenPosition value = null;
			TaskableBuilding byInstanceId = playerService.GetByInstanceId<TaskableBuilding>(questByInstanceId.QuestIconTrackedInstanceId);
			if (byInstanceId != null)
			{
				value = byInstanceId.Definition.ScreenPosition;
			}
			autoMoveSignal.Dispatch(zero, new Boxed<ScreenPosition>(value), new CameraMovementSettings(CameraMovementSettings.Settings.Quest, byInstanceId, questByInstanceId), false);
		}
	}
}
