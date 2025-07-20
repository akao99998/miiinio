using System.Collections.Generic;
using Kampai.Main;
using Kampai.UI.View;
using strange.extensions.command.impl;

namespace Kampai.Game.View
{
	public class OpenSellBuildingModelCommand : Command
	{
		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public PopupMessageSignal popupMessageSignal { get; set; }

		public override void Execute()
		{
			MarketplaceDefinition marketplaceDefinition = definitionService.Get<MarketplaceDefinition>();
			if (playerService.GetQuantity(StaticItem.LEVEL_ID) < marketplaceDefinition.LevelGate)
			{
				string @string = localService.GetString("MarketplaceUnlock", marketplaceDefinition.LevelGate);
				popupMessageSignal.Dispatch(@string, PopupMessageType.NORMAL);
				return;
			}
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Load, "screen_StorageBuilding");
			iGUICommand.skrimScreen = "StorageSkrim";
			iGUICommand.darkSkrim = true;
			iGUICommand.Args.Add(GetStorageBuilding());
			iGUICommand.Args.Add(StorageBuildingModalTypes.SELL);
			guiService.Execute(iGUICommand);
		}

		private StorageBuilding GetStorageBuilding()
		{
			StorageBuilding result = null;
			using (IEnumerator<StorageBuilding> enumerator = playerService.GetByDefinitionId<StorageBuilding>(3018).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					StorageBuilding current = enumerator.Current;
					result = current;
				}
			}
			return result;
		}
	}
}
