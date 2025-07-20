using Elevation.Logging;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using strange.extensions.context.api;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class ResourceIconMediator : Mediator
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("ResourceIconMediator") as IKampaiLogger;

		[Inject]
		public ResourceIconView View { get; set; }

		[Inject]
		public UITryHarvestSignal TryHarvestSignal { get; set; }

		[Inject]
		public HighlightHarvestButtonSignal HighlightHarvestSignal { get; set; }

		[Inject]
		public RemoveResourceIconSignal RemoveResourceIconSignal { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable GameContext { get; set; }

		[Inject]
		public IPlayerService PlayerService { get; set; }

		[Inject]
		public IDefinitionService DefinitionService { get; set; }

		[Inject]
		public IPositionService positionService { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		[Inject]
		public TryCollectLeisurePointsSignal tryCollectPoints { get; set; }

		public override void OnRegister()
		{
			View.Init(GameContext, logger, PlayerService, DefinitionService, positionService, localizationService);
			HighlightHarvestSignal.AddListener(HighlightHarvest);
			View.RemoveResourceIconSignal.AddListener(RemoveResourceIcon);
			View.ClickedSignal.AddListener(ButtonClicked);
		}

		public override void OnRemove()
		{
			HighlightHarvestSignal.RemoveListener(HighlightHarvest);
			View.RemoveResourceIconSignal.RemoveListener(RemoveResourceIcon);
			View.ClickedSignal.RemoveListener(ButtonClicked);
			View.Cleanup();
		}

		private void RemoveResourceIcon()
		{
			RemoveResourceIconSignal.Dispatch(new Tuple<int, int>(View.TrackedId, View.ItemDefID));
		}

		private void ButtonClicked()
		{
			if (View.leisureBuilding != null)
			{
				tryCollectPoints.Dispatch(View.leisureBuilding);
			}
			else
			{
				TryHarvestSignal.Dispatch(View.TrackedId, delegate
				{
				}, false);
			}
		}

		private void HighlightHarvest(bool isHighlighted)
		{
			View.HighlightHarvest(isHighlighted);
		}
	}
}
