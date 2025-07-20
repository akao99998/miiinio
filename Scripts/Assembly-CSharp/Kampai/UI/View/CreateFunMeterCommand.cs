using Elevation.Logging;
using Kampai.Game;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.UI.View
{
	public class CreateFunMeterCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("CreateFunMeterCommand") as IKampaiLogger;

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject(UIElement.HUD)]
		public GameObject hud { get; set; }

		[Inject]
		public FinishCreateFunMeterSignal finishCreateFunMeterSignal { get; set; }

		public override void Execute()
		{
			logger.EventStart("CreateFunMeterCommand.Execute");
			if (playerService.IsMinionPartyUnlocked())
			{
				GameObject gameObject = CreateNewXPBar();
				HUDView component = hud.GetComponent<HUDView>();
				RectTransform pointsPanel = component.PointsPanel;
				if (pointsPanel != null)
				{
					gameObject.transform.SetParent(pointsPanel, false);
					finishCreateFunMeterSignal.Dispatch();
				}
			}
			logger.EventStart("CreateFunMeterCommand.Execute");
		}

		private GameObject CreateNewXPBar()
		{
			GameObject gameObject = KampaiResources.Load<GameObject>("XP_FunMeter");
			if (gameObject == null)
			{
				logger.Error("Invalid GUISettings.Path: {0}", "XP_FunMeter");
				return null;
			}
			GameObject gameObject2 = Object.Instantiate(gameObject);
			if (gameObject2 == null)
			{
				logger.Error("Unable to create instance of {0}", "XP_FunMeter");
				return null;
			}
			gameObject2.name = "XP_FunMeter";
			return gameObject2;
		}
	}
}
