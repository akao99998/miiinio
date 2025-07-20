using Elevation.Logging;
using Kampai.Game;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.UI.View
{
	public class StartUpSellImpressionCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("StartUpSellImpressionCommand") as IKampaiLogger;

		[Inject]
		public int salePackDefID { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public OpenUpSellModalSignal openUpSellModalSignal { get; set; }

		public override void Execute()
		{
			SalePackDefinition definition;
			if (!definitionService.TryGet<SalePackDefinition>(salePackDefID, out definition))
			{
				logger.Error("The impression's salePackDefinition is null. returning");
			}
			else
			{
				openUpSellModalSignal.Dispatch(definition, "Impression", false);
			}
		}
	}
}
