using System;
using Kampai.Common;
using Kampai.UI.View;
using strange.extensions.command.impl;

namespace Kampai.Game.View
{
	public class AndroidBackButtonCommand : Command
	{
		[Inject]
		public UIModel model { get; set; }

		[Inject]
		public NetworkModel networkModel { get; set; }

		[Inject]
		public MinimizeAppSignal minimizeSignal { get; set; }

		[Inject]
		public IZoomCameraModel zoomModel { get; set; }

		[Inject]
		public BuildingZoomSignal zoomSignal { get; set; }

		[Inject]
		public LevelUpBackButtonSignal levelUpBackButtonSignal { get; set; }

		public override void Execute()
		{
			if (model.DisableBack)
			{
				return;
			}
			if (model.LevelUpUIOpen)
			{
				levelUpBackButtonSignal.Dispatch();
			}
			else if (networkModel.isConnectionLost)
			{
				minimizeSignal.Dispatch();
			}
			else if (model.UIOpen)
			{
				Action action = model.RemoveTopUI();
				if (action != null)
				{
					action();
				}
			}
			else if (zoomModel.ZoomedIn)
			{
				zoomSignal.Dispatch(new BuildingZoomSettings(ZoomType.OUT, zoomModel.LastZoomBuildingType));
			}
			else
			{
				minimizeSignal.Dispatch();
			}
		}
	}
}
