using Kampai.Game.View;
using Kampai.Main;
using Kampai.UI.View;
using Kampai.Util.Audio;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	internal sealed class MarketplaceItemSoldCommand : Command
	{
		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IMarketplaceService marketplaceService { get; set; }

		[Inject]
		public UpdateSaleSlotSignal updateSaleSlot { get; set; }

		[Inject]
		public MarketplaceUpdateSoldItemsSignal updateSoldItemsSignal { get; set; }

		[Inject]
		public int saleItemId { get; set; }

		[Inject]
		public PlayLocalAudioSignal playLocalAudioSignal { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		public override void Execute()
		{
			MarketplaceSaleItem byInstanceId = playerService.GetByInstanceId<MarketplaceSaleItem>(saleItemId);
			if (byInstanceId != null)
			{
				byInstanceId.state = MarketplaceSaleItem.State.SOLD;
				MarketplaceSaleSlot slotByItem = marketplaceService.GetSlotByItem(byInstanceId);
				updateSaleSlot.Dispatch(slotByItem.ID);
				StorageBuilding firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<StorageBuilding>(3018);
				BuildingManagerView component = buildingManager.GetComponent<BuildingManagerView>();
				BuildingObject buildingObject = component.GetBuildingObject(firstInstanceByDefinitionId.ID);
				CustomFMOD_StudioEventEmitter type = GetAudioEmitter.Get(buildingObject.gameObject, "LocalAudio");
				playLocalAudioSignal.Dispatch(type, "Play_marketplace_bagDrop_01", null);
				updateSoldItemsSignal.Dispatch(true);
			}
		}
	}
}
