using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.UI.View
{
	public class OpenUpSellModalCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("OpenUpSellModalCommand") as IKampaiLogger;

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public PackDefinition packDefinition { get; set; }

		[Inject]
		public string source { get; set; }

		[Inject]
		public bool disableSkrimButton { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public CloseAllOtherMenuSignal closeSignal { get; set; }

		public override void Execute()
		{
			if (packDefinition == null)
			{
				logger.Error("Pack Definition is null returning");
				return;
			}
			SendTelemetry();
			closeSignal.Dispatch(null);
			LoadGameObject();
		}

		private void SendTelemetry()
		{
			SalePackDefinition salePackDefinition = packDefinition as SalePackDefinition;
			telemetryService.Send_Telemetry_EVT_IGE_STORE_VISIT(source, (salePackDefinition != null) ? salePackDefinition.Type.ToString() : "StoreBundle");
		}

		public GameObject LoadGameObject()
		{
			return LoadGameObject(GUIOperation.Load);
		}

		public GameObject LoadGameObject(GUIOperation guiOperation)
		{
			string text = SetByType();
			IGUICommand iGUICommand = guiService.BuildCommand(guiOperation, text);
			GUIArguments args = iGUICommand.Args;
			args.Add(disableSkrimButton);
			args.Add(typeof(PackDefinition), packDefinition);
			args.Add(text);
			return guiService.Execute(iGUICommand);
		}

		public string SetByType()
		{
			string text = "screen_UpsellBundles";
			switch (packDefinition.Layout)
			{
			case SalePackLayout.FreeOne:
			case SalePackLayout.PayOne:
				return string.Format("screen_Upsell{0}Pack", 1);
			case SalePackLayout.FreeTwo:
			case SalePackLayout.PayTwo:
				return string.Format("screen_Upsell{0}Pack", 2);
			case SalePackLayout.FreeThree:
			case SalePackLayout.PayThree:
				return string.Format("screen_Upsell{0}Pack", 3);
			case SalePackLayout.FreeFour:
			case SalePackLayout.PayFour:
			case SalePackLayout.Starter:
				return string.Format("screen_Upsell{0}Pack", 4);
			case SalePackLayout.Custom:
				return packDefinition.LayoutPrefab;
			default:
			{
				int outputCount = GetOutputCount(packDefinition);
				return string.Format("screen_Upsell{0}Pack", outputCount);
			}
			}
		}

		public int GetOutputCount(PackDefinition packDefinition)
		{
			if (packDefinition == null || packDefinition.TransactionDefinition == null || packDefinition.TransactionDefinition.Outputs == null)
			{
				return 0;
			}
			int num = 0;
			IList<QuantityItem> outputs = packDefinition.TransactionDefinition.Outputs;
			for (int i = 0; i < outputs.Count; i++)
			{
				QuantityItem quantityItem = outputs[i];
				if (quantityItem != null)
				{
					Definition definition = definitionService.Get(quantityItem.ID);
					if (definition != null && definition.ID != 2 && !(definition is UnlockDefinition))
					{
						num++;
					}
				}
			}
			return num;
		}
	}
}
