using Kampai.Game;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.UI.View
{
	public class DisplayItemPopupCommand : Command
	{
		[Inject]
		public int definitionID { get; set; }

		[Inject]
		public RectTransform imageTransform { get; set; }

		[Inject]
		public UIPopupType popupType { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject(UIElement.CAMERA)]
		public Camera uiCamera { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		public override void Execute()
		{
			Vector3[] array = new Vector3[4];
			imageTransform.GetWorldCorners(array);
			Vector3 position = default(Vector3);
			float num = array[0].y;
			Vector3[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				Vector3 vector = array2[i];
				position += vector;
				num = Mathf.Min(num, vector.y);
			}
			position /= 4f;
			if (popupType == UIPopupType.HELPTIP)
			{
				position.y = num;
			}
			Vector3 vector2 = uiCamera.WorldToViewportPoint(position);
			IGUICommand iGUICommand = null;
			switch (popupType)
			{
			case UIPopupType.GENERIC:
			case UIPopupType.GENERICGOTO:
				iGUICommand = guiService.BuildCommand(GUIOperation.LoadUntrackedInstance, "cmp_GenericTooltip");
				break;
			case UIPopupType.CRAFTING:
				iGUICommand = guiService.BuildCommand(GUIOperation.LoadUntrackedInstance, "cmp_CraftingTooltip");
				break;
			case UIPopupType.MINIONPARTYHUD:
				iGUICommand = guiService.BuildCommand(GUIOperation.LoadUntrackedInstance, "screen_PartyMeterFlyout");
				iGUICommand.skrimScreen = "GenericPopup";
				iGUICommand.darkSkrim = false;
				iGUICommand.genericPopupSkrim = true;
				break;
			case UIPopupType.MINIONPARTYCOUNTDOWN:
				iGUICommand = guiService.BuildCommand(GUIOperation.LoadUntrackedInstance, "HUD_PartyMeterCountDownTimer");
				break;
			case UIPopupType.HELPTIP:
				iGUICommand = guiService.BuildCommand(GUIOperation.LoadUntrackedInstance, "popup_HelpTip");
				break;
			}
			if (iGUICommand != null)
			{
				GUIArguments args = iGUICommand.Args;
				args.Add(vector2);
				args.Add(typeof(RectTransform), imageTransform);
				if (popupType == UIPopupType.GENERIC || popupType == UIPopupType.GENERICGOTO || popupType == UIPopupType.CRAFTING)
				{
					ItemDefinition value = definitionService.Get<ItemDefinition>(definitionID);
					args.Add(typeof(ItemDefinition), value);
					args.Add(typeof(bool), popupType == UIPopupType.GENERICGOTO);
				}
				else if (popupType == UIPopupType.HELPTIP)
				{
					HelpTipDefinition value2 = definitionService.Get<HelpTipDefinition>(definitionID);
					args.Add(typeof(HelpTipDefinition), value2);
				}
				guiService.Execute(iGUICommand);
			}
		}
	}
}
