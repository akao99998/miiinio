using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;
using strange.extensions.context.api;
using strange.extensions.injector.api;

namespace Kampai.UI
{
	public class DragFromStoreCommand : Command
	{
		private IKampaiLogger logger = LogManager.GetClassLogger("DragFromStoreCommand") as IKampaiLogger;

		[Inject]
		public Definition definition { get; set; }

		[Inject]
		public TransactionDefinition transactionDef { get; set; }

		[Inject]
		public Vector3 eventPosition { get; set; }

		[Inject]
		public bool isFromStore { get; set; }

		[Inject]
		public PickControllerModel pickControllerModel { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject(MainElement.CAMERA)]
		public Camera mainCamera { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public DecoGridModel decoGridModel { get; set; }

		[Inject]
		public DragAndDropPickSignal dragAndDropSignal { get; set; }

		public override void Execute()
		{
			ICrossContextInjectionBinder crossContextInjectionBinder = gameContext.injectionBinder;
			BuildingDefinition buildingDefinition = definition as BuildingDefinition;
			if (buildingDefinition == null)
			{
				return;
			}
			pickControllerModel.LastBuildingStorePosition = eventPosition;
			Vector3 positioningBasedOnBuildingType = GetPositioningBasedOnBuildingType(buildingDefinition);
			Scaffolding instance = crossContextInjectionBinder.GetInstance<Scaffolding>();
			instance.Definition = buildingDefinition;
			instance.Location = new Location(positioningBasedOnBuildingType);
			instance.Transaction = transactionDef;
			instance.Building = null;
			instance.Lifted = true;
			logger.Debug(transactionDef.ToString());
			bool type = false;
			ICollection<Instance> byDefinitionId = playerService.GetByDefinitionId<Instance>(buildingDefinition.ID);
			if (byDefinitionId.Count != 0)
			{
				foreach (Instance item in byDefinitionId)
				{
					Building building = item as Building;
					if (building != null && building.State == BuildingState.Inventory)
					{
						instance.Building = building;
						type = true;
						break;
					}
				}
			}
			DragOffsetType offsetType = DragOffsetType.NONE;
			if (buildingDefinition.FootprintID == 300000)
			{
				offsetType = DragOffsetType.ONE_X_ONE;
			}
			CreateDummyBuildingSignal instance2 = crossContextInjectionBinder.GetInstance<CreateDummyBuildingSignal>();
			positioningBasedOnBuildingType = new Vector3(positioningBasedOnBuildingType.x, 0f, positioningBasedOnBuildingType.z);
			instance2.Dispatch(buildingDefinition, positioningBasedOnBuildingType, type);
			SetPickController(-1, offsetType);
		}

		private Vector3 GetPositioningBasedOnBuildingType(BuildingDefinition definition)
		{
			ICrossContextInjectionBinder crossContextInjectionBinder = gameContext.injectionBinder;
			DecorationBuildingDefinition decorationBuildingDefinition = definition as DecorationBuildingDefinition;
			ConnectableBuildingDefinition connectableBuildingDefinition = definition as ConnectableBuildingDefinition;
			Vector3 result = default(Vector3);
			if (isFromStore)
			{
				return BuildingUtil.UIToWorldCoords(mainCamera, eventPosition);
			}
			if (connectableBuildingDefinition != null)
			{
				Vector3 newPieceLocation = decoGridModel.GetNewPieceLocation((int)eventPosition.x, (int)eventPosition.z, connectableBuildingDefinition.connectableType, crossContextInjectionBinder.GetInstance<Environment>());
				return eventPosition + newPieceLocation;
			}
			if (decorationBuildingDefinition != null)
			{
				Vector3 newPieceLocation2 = decoGridModel.GetNewPieceLocation((int)eventPosition.x, (int)eventPosition.z, 0, crossContextInjectionBinder.GetInstance<Environment>());
				return eventPosition + newPieceLocation2;
			}
			return result;
		}

		private void SetPickController(int? selectedBuildingID, DragOffsetType offsetType)
		{
			pickControllerModel.InvalidateMovement = false;
			pickControllerModel.StartHitObject = null;
			pickControllerModel.CurrentMode = PickControllerModel.Mode.DragAndDrop;
			pickControllerModel.SelectedBuilding = selectedBuildingID;
			dragAndDropSignal.Dispatch(isFromStore ? 1 : 3, eventPosition, offsetType);
		}
	}
}
