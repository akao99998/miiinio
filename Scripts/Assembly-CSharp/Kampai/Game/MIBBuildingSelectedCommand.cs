using Elevation.Logging;
using Kampai.Main;
using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.command.impl;
using strange.extensions.context.api;

namespace Kampai.Game
{
	public class MIBBuildingSelectedCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("MIBBuildingSelectedCommand") as IKampaiLogger;

		[Inject]
		public IHindsightService hindsightService { get; set; }

		[Inject]
		public PopupMessageSignal PopupMessageSignal { get; set; }

		[Inject]
		public ILocalizationService LocalizationService { get; set; }

		[Inject]
		public IPlayerService PlayerService { get; set; }

		[Inject(MainElement.CONTEXT)]
		public ICrossContextCapable mainContext { get; set; }

		[Inject]
		public IMIBService mibService { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playGlobalSoundFXSignal { get; set; }

		public override void Execute()
		{
			HindsightCampaign cachedContent = hindsightService.GetCachedContent(HindsightCampaign.Scope.message_in_a_bottle);
			if (cachedContent != null)
			{
				MIBBuilding firstInstanceByDefinitionId = PlayerService.GetFirstInstanceByDefinitionId<MIBBuilding>(3129);
				if (firstInstanceByDefinitionId != null && firstInstanceByDefinitionId.MIBState == MIBBuildingState.READY && !mibService.IsUserReturning())
				{
					playGlobalSoundFXSignal.Dispatch("Play_menu_popUp_01");
					mainContext.injectionBinder.GetInstance<DisplayHindsightContentSignal>().Dispatch(HindsightCampaign.Scope.message_in_a_bottle);
				}
			}
			else
			{
				PopupMessageSignal.Dispatch(LocalizationService.GetString("NoUpsightContentAvailable"), PopupMessageType.NORMAL);
			}
		}
	}
}
