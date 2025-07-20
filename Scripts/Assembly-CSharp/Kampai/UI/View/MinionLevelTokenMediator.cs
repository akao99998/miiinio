using Elevation.Logging;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class MinionLevelTokenMediator : Mediator
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("MinionLevelTokenMediator") as IKampaiLogger;

		[Inject]
		public MinionLevelTokenView view { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public SpawnDooberSignal spawnDooberSignal { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public IPositionService positionService { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		public override void OnRegister()
		{
			VillainLairEntranceBuilding byInstanceId = playerService.GetByInstanceId<VillainLairEntranceBuilding>(374);
			WorldToGlassUIModal component = view.GetComponent<WorldToGlassUIModal>();
			component.Settings = new WorldToGlassUISettings(byInstanceId.ID);
			view.Init(positionService, gameContext, logger, playerService, localizationService);
			view.HarvestButton.ClickedSignal.AddListener(HarvestPendingToken);
			UpdateView();
		}

		public override void OnRemove()
		{
			view.HarvestButton.ClickedSignal.RemoveListener(HarvestPendingToken);
		}

		private void UpdateView()
		{
			uint quantity = playerService.GetQuantity(StaticItem.PENDING_MINION_LEVEL_TOKEN);
			view.SetTokenCount(quantity);
		}

		private void HarvestPendingToken()
		{
			if (playerService.GetQuantity(StaticItem.PENDING_MINION_LEVEL_TOKEN) != 0)
			{
				playerService.AlterQuantity(StaticItem.PENDING_MINION_LEVEL_TOKEN, -1);
			}
			playerService.AlterQuantity(StaticItem.MINION_LEVEL_TOKEN, 1);
			VillainLairEntranceBuilding byInstanceId = playerService.GetByInstanceId<VillainLairEntranceBuilding>(374);
			Vector3 type = new Vector3(byInstanceId.Location.x, 0f, byInstanceId.Location.y);
			spawnDooberSignal.Dispatch(type, DestinationType.MINION_LEVEL_TOKEN, 31, true);
			IGUICommand command = guiService.BuildCommand(GUIOperation.Unload, "cmp_MinionLevelToken");
			guiService.Execute(command);
		}
	}
}
