using Kampai.Game;

namespace Kampai.UI.View
{
	public class StorageBuildingWayFinderMediator : AbstractWayFinderMediator
	{
		[Inject]
		public StorageBuildingWayFinderView StorageBuildingWayFinderView { get; set; }

		[Inject]
		public IMarketplaceService MarketplaceService { get; set; }

		public override IWayFinderView View
		{
			get
			{
				return StorageBuildingWayFinderView;
			}
		}

		public override void OnRegister()
		{
			base.OnRegister();
			if (MarketplaceService.AreThereSoldItems())
			{
				StorageBuildingWayFinderView.UpdateIcon(base.DefinitionService.Get<WayFinderDefinition>(1000008086).MarketplaceSoldIcon);
			}
		}
	}
}
