using System;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using strange.extensions.context.api;

namespace Kampai.UI.View
{
	public class MoveBuildingMenuMediator : UIStackMediator<MoveBuildingMenuView>
	{
		[Flags]
		public enum Buttons
		{
			None = 0,
			Inventory = 1,
			Accept = 4,
			Close = 8,
			All = 0x10
		}

		public IKampaiLogger logger = LogManager.GetClassLogger("MoveBuildingMenuMediator") as IKampaiLogger;

		private Scaffolding currentScaffolding;

		[Inject]
		public PlayGlobalSoundFXSignal playSFXSignal { get; set; }

		[Inject]
		public UpdateMovementValidity updateSignal { get; set; }

		[Inject]
		public IPositionService positionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		[Inject]
		public DisableMoveToInventorySignal disableSignal { get; set; }

		[Inject]
		public PickControllerModel pickControllerModel { get; set; }

		[Inject]
		public UIModel model { get; set; }

		public override void OnRegister()
		{
			model.AllowMultiTouch = true;
			updateSignal.AddListener(UpdateValidity);
			disableSignal.AddListener(DisableInventory);
			currentScaffolding = gameContext.injectionBinder.GetInstance<Scaffolding>();
			base.view.Init(positionService, gameContext, logger, playerService, localizationService);
			base.view.InventoryButton.ClickedSignal.AddListener(OnInventory);
			base.view.AcceptButton.ClickedSignal.AddListener(OnAccept);
			base.view.CloseButton.ClickedSignal.AddListener(OnClose);
			UpdateCostPanel();
			base.OnRegister();
		}

		public override void OnRemove()
		{
			model.AllowMultiTouch = false;
			updateSignal.RemoveListener(UpdateValidity);
			base.view.InventoryButton.ClickedSignal.RemoveListener(OnInventory);
			base.view.AcceptButton.ClickedSignal.RemoveListener(OnAccept);
			base.view.CloseButton.ClickedSignal.RemoveListener(OnClose);
			disableSignal.RemoveListener(DisableInventory);
			base.OnRemove();
		}

		private void OnInventory()
		{
			playSFXSignal.Dispatch("Play_low_woosh_01");
			ToInventory();
		}

		private void OnAccept()
		{
			Confirm();
		}

		private void OnClose()
		{
			Cancel();
		}

		private void DisableInventory()
		{
			base.view.DisableInventory();
		}

		private void Cancel()
		{
			CancelBuildingMovementSignal instance = gameContext.injectionBinder.GetInstance<CancelBuildingMovementSignal>();
			instance.Dispatch(false);
		}

		private void Confirm()
		{
			ConfirmBuildingMovementSignal instance = gameContext.injectionBinder.GetInstance<ConfirmBuildingMovementSignal>();
			instance.Dispatch();
		}

		private void ToInventory()
		{
			InventoryBuildingMovementSignal instance = gameContext.injectionBinder.GetInstance<InventoryBuildingMovementSignal>();
			instance.Dispatch();
		}

		private void UpdateValidity(bool enable)
		{
			base.view.UpdateValidity(enable);
		}

		private void UpdateCostPanel()
		{
			int num = 0;
			if (pickControllerModel.SelectedBuilding.HasValue && pickControllerModel.SelectedBuilding != -1)
			{
				num = pickControllerModel.SelectedBuilding.Value;
				Building byInstanceId = playerService.GetByInstanceId<Building>(num);
				SetInventoryCount(byInstanceId.Definition.ID);
			}
			else if (currentScaffolding.Lifted && currentScaffolding.Definition != null)
			{
				SetInventoryCount(currentScaffolding.Definition.ID);
			}
		}

		private void SetInventoryCount(int buildingDefID)
		{
			int inventoryCountByDefinitionID = playerService.GetInventoryCountByDefinitionID(buildingDefID);
			base.view.SetInventoryCount(inventoryCountByDefinitionID);
		}

		protected override void Close()
		{
		}
	}
}
