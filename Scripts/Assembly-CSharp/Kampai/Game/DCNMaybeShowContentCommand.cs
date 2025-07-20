using Elevation.Logging;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class DCNMaybeShowContentCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("DCNMaybeShowContentCommand") as IKampaiLogger;

		[Inject]
		public DCNFeaturedSignal dcnFeaturedSignal { get; set; }

		[Inject]
		public IDCNService dcnService { get; set; }

		[Inject]
		public ShowDCNScreenSignal showDCNScreenSignal { get; set; }

		[Inject]
		public IConfigurationsService configurationService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		public override void Execute()
		{
			DCNBuildingDefinition dCNBuildingDefinition = definitionService.Get<DCNBuildingDefinition>(3128);
			uint quantity = playerService.GetQuantity(StaticItem.LEVEL_ID);
			if (quantity >= dCNBuildingDefinition.UnlockLevel)
			{
				if (configurationService.isKillSwitchOn(KillSwitch.DCN))
				{
					logger.Info("DCN disabled by killswitch");
				}
				else if (dcnService.HasFeaturedContent())
				{
					showDCNScreenSignal.Dispatch(true);
				}
				else
				{
					dcnFeaturedSignal.Dispatch();
				}
			}
		}
	}
}
