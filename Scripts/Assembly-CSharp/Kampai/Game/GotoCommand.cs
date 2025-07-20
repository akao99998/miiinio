using System.Collections.Generic;
using Elevation.Logging;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class GotoCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("GotoCommand") as IKampaiLogger;

		[Inject]
		public GotoArgument gotoArgument { get; set; }

		[Inject]
		public OpenStoreHighlightItemSignal openStoreSignal { get; set; }

		[Inject]
		public CameraAutoMoveToBuildingSignal moveToBuildingSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public BuildMenuOpenedSignal buildMenuOpened { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		public override void Execute()
		{
			if (gotoArgument.BuildingId > 0)
			{
				Building byInstanceId = playerService.GetByInstanceId<Building>(gotoArgument.BuildingId);
				moveToBuildingSignal.Dispatch(byInstanceId, new PanInstructions(byInstanceId));
			}
			else if (gotoArgument.BuildingDefId > 0)
			{
				GotoBuildingDefinition(gotoArgument.BuildingDefId, gotoArgument.ForceStore);
			}
			else if (gotoArgument.ItemId > 0)
			{
				int buildingDefintionIDFromItemDefintionID = definitionService.GetBuildingDefintionIDFromItemDefintionID(gotoArgument.ItemId);
				if (buildingDefintionIDFromItemDefintionID > 0)
				{
					GotoBuildingDefinition(buildingDefintionIDFromItemDefintionID, gotoArgument.ForceStore);
					return;
				}
				logger.Warning("No building for item {0}", gotoArgument.ItemId);
			}
			else
			{
				logger.Warning("Nothing to goto");
			}
		}

		private void GotoBuildingDefinition(int definition, bool forceStore)
		{
			if (forceStore)
			{
				buildMenuOpened.Dispatch();
				openStoreSignal.Dispatch(definition, true);
				return;
			}
			IList<Building> buildingsWithoutState = playerService.GetBuildingsWithoutState(BuildingState.Inventory);
			if (buildingsWithoutState.Count > 0)
			{
				Building building = buildingsWithoutState[Random.Range(0, buildingsWithoutState.Count - 1)];
				gameContext.injectionBinder.GetInstance<PanAndOpenModalSignal>().Dispatch(building.ID, false);
			}
			else
			{
				buildMenuOpened.Dispatch();
				openStoreSignal.Dispatch(definition, true);
			}
		}
	}
}
