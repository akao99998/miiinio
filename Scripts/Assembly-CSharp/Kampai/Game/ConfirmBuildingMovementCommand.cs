using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game.Transaction;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;
using strange.extensions.context.api;
using strange.extensions.injector.api;

namespace Kampai.Game
{
	public class ConfirmBuildingMovementCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("ConfirmBuildingMovementCommand") as IKampaiLogger;

		[Inject]
		public PickControllerModel pickControllerModel { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IBuildingUtilities buildingUtil { get; set; }

		[Inject]
		public PlaceBuildingSignal placeBuildingSignal { get; set; }

		[Inject]
		public DeselectBuildingSignal deselectBuildingSignal { get; set; }

		[Inject]
		public DisableCameraBehaviourSignal disableCameraSignal { get; set; }

		[Inject]
		public EnableCameraBehaviourSignal enableCameraSignal { get; set; }

		[Inject]
		public DebugUpdateGridSignal gridSignal { get; set; }

		[Inject]
		public CancelBuildingMovementSignal cancelBuildingMovementSignal { get; set; }

		[Inject]
		public PurchaseNewBuildingSignal purchaseNewBuildingSignal { get; set; }

		[Inject]
		public BuildingChangeStateSignal buildingChangeStateSignal { get; set; }

		[Inject]
		public CreateInventoryBuildingSignal createInventoryBuildingSignal { get; set; }

		[Inject]
		public ILocalPersistanceService localPersistService { get; set; }

		[Inject]
		public Scaffolding currentScaffolding { get; set; }

		[Inject]
		public SetPremiumCurrencySignal setPremiumCurrencySignal { get; set; }

		[Inject]
		public SetGrindCurrencySignal setGrindCurrencySignal { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal globalAudioSignal { get; set; }

		[Inject]
		public SpawnDooberSignal tweenSignal { get; set; }

		[Inject]
		public IQuestService questService { get; set; }

		[Inject(UIElement.CONTEXT)]
		public ICrossContextCapable uiContext { get; set; }

		public override void Execute()
		{
			if (pickControllerModel.SelectedBuilding == -1)
			{
				currentScaffolding.Lifted = false;
				if (buildingUtil.ValidateScaffoldingPlacement(currentScaffolding.Definition, currentScaffolding.Location))
				{
					if (currentScaffolding.Building != null)
					{
						currentScaffolding.Building.Location = currentScaffolding.Location;
						buildingChangeStateSignal.Dispatch(currentScaffolding.Building.ID, BuildingState.Idle);
						createInventoryBuildingSignal.Dispatch(currentScaffolding.Building, currentScaffolding.Location);
						questService.UpdateAllQuestsWithQuestStepType(QuestStepType.Construction);
						PlaceCurrentScaffolding();
					}
					else
					{
						PurchaseBuilding();
					}
					globalAudioSignal.Dispatch("Play_building_place_01");
				}
				else
				{
					cancelBuildingMovementSignal.Dispatch(true);
				}
			}
			else
			{
				MoveBuilding();
			}
		}

		private void PurchaseBuilding()
		{
			TransactionArg transactionArg = new TransactionArg();
			transactionArg.Add(currentScaffolding.Location);
			transactionArg.Source = "ItemPurchase";
			TransactionDefinition transactionDefinition = definitionService.Get<TransactionDefinition>(currentScaffolding.Transaction.ID);
			TransactionDefinition transactionDefinition2 = transactionDefinition.CopyTransaction();
			foreach (QuantityItem output in transactionDefinition2.Outputs)
			{
				BuildingDefinition buildingDefinition = definitionService.Get<BuildingDefinition>(output.ID);
				if (buildingDefinition == null)
				{
					continue;
				}
				ICollection<Building> byDefinitionId = playerService.GetByDefinitionId<Building>(buildingDefinition.ID);
				int num = byDefinitionId.Count * buildingDefinition.IncrementalCost;
				if (num <= 0)
				{
					continue;
				}
				foreach (QuantityItem input in transactionDefinition2.Inputs)
				{
					if (input.ID == 0 || input.ID == 1)
					{
						input.Quantity += (uint)num;
					}
				}
			}
			playerService.RunEntireTransaction(transactionDefinition2, TransactionTarget.NO_VISUAL, TransactionCallback, transactionArg);
		}

		private void TransactionCallback(PendingCurrencyTransaction pct)
		{
			if (pct.Success)
			{
				Building building = null;
				foreach (Instance output in pct.GetOutputs())
				{
					if (output.Definition.ID == currentScaffolding.Definition.ID)
					{
						building = (Building)output;
						break;
					}
				}
				if (building != null)
				{
					building.Location = currentScaffolding.Location;
					purchaseNewBuildingSignal.Dispatch(building);
					setPremiumCurrencySignal.Dispatch();
					setGrindCurrencySignal.Dispatch();
					if (building.State != BuildingState.Idle && building.State != BuildingState.Construction)
					{
						buildingChangeStateSignal.Dispatch(building.ID, BuildingState.Inactive);
					}
					AwardXP(building);
					currentScaffolding.Building = building;
					PlaceCurrentScaffolding();
				}
				else
				{
					logger.Log(KampaiLogLevel.Error, "Attempting to place a null building.");
				}
			}
			else if (pct.ParentSuccess)
			{
				PurchaseBuilding();
			}
			else
			{
				cancelBuildingMovementSignal.Dispatch(false);
			}
		}

		private void AwardXP(Building newBuilding)
		{
			DecorationBuildingDefinition decorationBuildingDefinition = newBuilding.Definition as DecorationBuildingDefinition;
			if (decorationBuildingDefinition != null)
			{
				Vector3 type = new Vector3(newBuilding.Location.x, 0f, newBuilding.Location.y);
				playerService.CreateAndRunCustomTransaction(2, decorationBuildingDefinition.XPReward, TransactionTarget.REWARD_BUILDING, new TransactionArg(string.Format("Place {0}", decorationBuildingDefinition.LocalizedKey)));
				tweenSignal.Dispatch(type, DestinationType.XP, 2, true);
			}
		}

		private void PlaceCurrentScaffolding()
		{
			pickControllerModel.SelectedBuilding = null;
			placeBuildingSignal.Dispatch(currentScaffolding.Building.ID, currentScaffolding.Location);
			deselectBuildingSignal.Dispatch(-1);
			gridSignal.Dispatch();
			ICrossContextInjectionBinder crossContextInjectionBinder = uiContext.injectionBinder;
			UIModel instance = crossContextInjectionBinder.GetInstance<UIModel>();
			BuildingDefinition definition = currentScaffolding.Definition;
			DecorationBuildingDefinition decorationBuildingDefinition = definition as DecorationBuildingDefinition;
			int iD = definition.ID;
			bool flag = playerService.CheckIfBuildingIsCapped(iD);
			bool flag2 = decorationBuildingDefinition != null && decorationBuildingDefinition.AutoPlace && !flag;
			if (definition.Type == BuildingType.BuildingTypeIdentifier.CONNECTABLE || flag2)
			{
				crossContextInjectionBinder.GetInstance<DragFromStoreSignal>().Dispatch(currentScaffolding.Definition, currentScaffolding.Transaction, currentScaffolding.Location.ToVector3(), false);
				instance.GoToInEffect = false;
				return;
			}
			bool flag3 = decorationBuildingDefinition != null && !decorationBuildingDefinition.AutoPlace && !flag && !instance.GoToInEffect;
			if (definition.Type != BuildingType.BuildingTypeIdentifier.CONNECTABLE && flag3)
			{
				crossContextInjectionBinder.GetInstance<OpenStoreHighlightItemSignal>().Dispatch(iD, true);
			}
			instance.GoToInEffect = false;
		}

		private void MoveBuilding()
		{
			if (!pickControllerModel.SelectedBuilding.HasValue)
			{
				return;
			}
			BuildingManagerView component = buildingManager.GetComponent<BuildingManagerView>();
			int value = pickControllerModel.SelectedBuilding.Value;
			BuildingObject buildingObject = component.GetBuildingObject(value);
			if (!(buildingObject == null))
			{
				Building byInstanceId = playerService.GetByInstanceId<Building>(value);
				Location location = new Location(buildingObject.transform.position);
				if (buildingUtil.ValidateLocation(byInstanceId, location))
				{
					placeBuildingSignal.Dispatch(value, location);
					globalAudioSignal.Dispatch("Play_building_place_01");
					deselectBuildingSignal.Dispatch(value);
					disableCameraSignal.Dispatch(8);
					enableCameraSignal.Dispatch(1);
					gridSignal.Dispatch();
					LocalPersistMoveBuildingAction();
				}
			}
		}

		private void LocalPersistMoveBuildingAction()
		{
			if (!localPersistService.HasKey("didyouknow_MoveBuilding"))
			{
				localPersistService.PutDataInt("didyouknow_MoveBuilding", 1);
			}
		}
	}
}
