using Elevation.Logging;
using Kampai.Game;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.UI.View
{
	public class CreatePartyMeterCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("CreatePartyMeterCommand") as IKampaiLogger;

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject(UIElement.HUD)]
		public GameObject hud { get; set; }

		public override void Execute()
		{
			logger.EventStart("CreatePartyMeterCommand.Execute");
			if (playerService.IsMinionPartyUnlocked())
			{
				GameObject gameObject = CreatePartyMeter();
				HUDView component = hud.GetComponent<HUDView>();
				RectTransform partyMeterPanel = component.PartyMeterPanel;
				if (partyMeterPanel != null)
				{
					gameObject.transform.SetParent(partyMeterPanel, false);
				}
			}
			logger.EventStart("CreatePartyMeterCommand.Execute");
		}

		private GameObject CreatePartyMeter()
		{
			GameObject gameObject = KampaiResources.Load<GameObject>("cmp_PartyMeterTimer");
			if (gameObject == null)
			{
				logger.Error("Invalid GUISettings.Path: {0}", "cmp_PartyMeterTimer");
				return null;
			}
			GameObject gameObject2 = Object.Instantiate(gameObject);
			if (gameObject2 == null)
			{
				logger.Error("Unable to create instance of {0}", "cmp_PartyMeterTimer");
				return null;
			}
			gameObject2.name = "cmp_PartyMeterTimer";
			return gameObject2;
		}
	}
}
