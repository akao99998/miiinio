using Kampai.Game;

namespace Kampai.UI.View
{
	public class BobPointsAtStuffWayFinderMediator : AbstractWayFinderMediator
	{
		[Inject]
		public BobPointsAtStuffWayFinderView BobPointsAtStuffWayFinderView { get; set; }

		[Inject]
		public ILandExpansionService LandExpansionService { get; set; }

		[Inject]
		public ILandExpansionConfigService LandExpansionConfigService { get; set; }

		public override IWayFinderView View
		{
			get
			{
				return BobPointsAtStuffWayFinderView;
			}
		}

		public override void OnRegister()
		{
			base.OnRegister();
			BobPointsAtStuffWayFinderView.Init(LandExpansionConfigService, base.DefinitionService);
		}

		protected override void GoToClicked()
		{
			if (base.pickModel.PanningCameraBlocked || base.lairModel.goingToLair)
			{
				return;
			}
			if (View.IsTargetObjectVisible())
			{
				if (base.PlayerService.HasTargetExpansion())
				{
					LandExpansionBuilding landExpansionBuilding = LandExpansionService.GetBuildingsByExpansionID(base.PlayerService.GetTargetExpansion())[0];
					base.GameContext.injectionBinder.GetInstance<SelectLandExpansionSignal>().Dispatch(landExpansionBuilding.ID);
				}
			}
			else
			{
				PanToInstance();
			}
		}
	}
}
