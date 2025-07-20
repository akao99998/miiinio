using Kampai.UI.View;
using UnityEngine;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.Game.View
{
	public class OpenOrderBoardCommand : Command
	{
		[Inject]
		public OrderBoard orderBoard { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public CloseAllOtherMenuSignal closeSignal { get; set; }

		[Inject]
		public ShowHUDReminderSignal showHUDReminderSignal { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject(GameElement.BUILDING_MANAGER)]
		public GameObject buildingManager { get; set; }

		public override void Execute()
		{
			OrderBoard byInstanceId = playerService.GetByInstanceId<OrderBoard>(309);
			if (byInstanceId == null || byInstanceId.State != BuildingState.Inaccessible)
			{
				OpenGUI();
			}
		}

		private void OpenGUI()
		{
			gameContext.injectionBinder.GetInstance<BuildingZoomSignal>().Dispatch(new BuildingZoomSettings(ZoomType.IN, BuildingZoomType.ORDERBOARD, ZoomComplete));
		}

		public void ZoomComplete()
		{
			showHUDReminderSignal.Dispatch(false);
			closeSignal.Dispatch(null);
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Load, "screen_OrderBoard");
			BuildingManagerView component = buildingManager.GetComponent<BuildingManagerView>();
			BuildingObject buildingObject = component.GetBuildingObject(orderBoard.ID);
			if (!(buildingObject == null))
			{
				OrderBoardBuildingTicketsView component2 = buildingObject.GetComponent<OrderBoardBuildingTicketsView>();
				iGUICommand.Args.Add(component2);
				iGUICommand.Args.Add(orderBoard);
				iGUICommand.skrimScreen = "OrderBoardSkrim";
				iGUICommand.darkSkrim = true;
				guiService.Execute(iGUICommand);
			}
		}
	}
}
