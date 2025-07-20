using System;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game;
using Kampai.Util;
using strange.extensions.context.api;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class WayFinderPanelMediator : Mediator
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("WayFinderPanelMediator") as IKampaiLogger;

		private StartSaleSignal startSaleSignal;

		private EndSaleSignal endSaleSignal;

		[Inject]
		public WayFinderPanelView View { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public CreateWayFinderSignal CreateWayFinderSignal { get; set; }

		[Inject]
		public RemoveWayFinderSignal RemoveWayFinderSignal { get; set; }

		[Inject]
		public GetWayFinderSignal GetWayFinderSignal { get; set; }

		[Inject]
		public ShowAllWayFindersSignal ShowAllWayFindersSignal { get; set; }

		[Inject]
		public HideAllWayFindersSignal HideAllWayFindersSignal { get; set; }

		[Inject]
		public SetLimitTikiBarWayFindersSignal SetLimitTikiBarWayFindersSignal { get; set; }

		[Inject]
		public UpdateWayFinderPrioritySignal UpdateWayFinderPrioritySignal { get; set; }

		[Inject]
		public IPrestigeService PrestigeService { get; set; }

		[Inject]
		public ITikiBarService TikiBarService { get; set; }

		[Inject]
		public IPlayerService PlayerService { get; set; }

		[Inject]
		public IPositionService PositionService { get; set; }

		[Inject]
		public AddQuestToExistingWayFinderSignal AddQuestToExistingWayFinderSignal { get; set; }

		[Inject]
		public RemoveQuestFromExistingWayFinderSignal RemoveQuestFromExistingWayFinderSignal { get; set; }

		[Inject]
		public VillainLairModel lairModel { get; set; }

		[Inject]
		public PickControllerModel pickControllerModel { get; set; }

		public override void OnRegister()
		{
			View.Init(logger, TikiBarService, PlayerService, PrestigeService, PositionService, pickControllerModel);
			CreateWayFinderSignal.AddListener(CreateWayFinder);
			RemoveWayFinderSignal.AddListener(RemoveWayFinder);
			GetWayFinderSignal.AddListener(GetWayFinder);
			ShowAllWayFindersSignal.AddListener(ShowAllWayFinders);
			HideAllWayFindersSignal.AddListener(HideAllWayFinders);
			SetLimitTikiBarWayFindersSignal.AddListener(SetLimitTikiBarWayFinders);
			UpdateWayFinderPrioritySignal.AddListener(UpdateWayFinderPriority);
			AddQuestToExistingWayFinderSignal.AddListener(AddQuestToExistingWayFinder);
			RemoveQuestFromExistingWayFinderSignal.AddListener(RemoveQuestFromExistingWayFinder);
			startSaleSignal = gameContext.injectionBinder.GetInstance<StartSaleSignal>();
			startSaleSignal.AddListener(UpdateHUDSnap);
			endSaleSignal = gameContext.injectionBinder.GetInstance<EndSaleSignal>();
			endSaleSignal.AddListener(UpdateHUDSnap);
		}

		public override void OnRemove()
		{
			View.Cleanup();
			CreateWayFinderSignal.RemoveListener(CreateWayFinder);
			RemoveWayFinderSignal.RemoveListener(RemoveWayFinder);
			GetWayFinderSignal.RemoveListener(GetWayFinder);
			ShowAllWayFindersSignal.RemoveListener(ShowAllWayFinders);
			HideAllWayFindersSignal.RemoveListener(HideAllWayFinders);
			SetLimitTikiBarWayFindersSignal.RemoveListener(SetLimitTikiBarWayFinders);
			UpdateWayFinderPrioritySignal.RemoveListener(UpdateWayFinderPriority);
			AddQuestToExistingWayFinderSignal.RemoveListener(AddQuestToExistingWayFinder);
			RemoveQuestFromExistingWayFinderSignal.RemoveListener(RemoveQuestFromExistingWayFinder);
			startSaleSignal.RemoveListener(UpdateHUDSnap);
			endSaleSignal.RemoveListener(UpdateHUDSnap);
		}

		private void CreateWayFinder(WayFinderSettings settings)
		{
			View.CreateWayFinder(settings, settings.updatePriority);
		}

		private void RemoveWayFinder(int trackedId)
		{
			View.RemoveWayFinder(trackedId);
		}

		private void GetWayFinder(int trackedId, Action<int, IWayFinderView> callback)
		{
			callback(trackedId, View.GetWayFinder(trackedId));
		}

		private void ShowAllWayFinders()
		{
			if (lairModel.currentActiveLair == null)
			{
				View.ShowAllWayFinders();
			}
		}

		private void HideAllWayFinders()
		{
			if (lairModel.currentActiveLair == null)
			{
				View.HideAllWayFinders();
			}
		}

		private void SetLimitTikiBarWayFinders(bool limitWayFinders)
		{
			View.SetLimitTikiBarWayFinders(limitWayFinders);
		}

		private void UpdateWayFinderPriority()
		{
			View.UpdateWayFinderPriority();
		}

		private void AddQuestToExistingWayFinder(int questDefId, int trackedId)
		{
			View.AddQuestToExistingWayFinder(questDefId, trackedId);
		}

		private void RemoveQuestFromExistingWayFinder(int questDefId, int trackedId)
		{
			View.RemoveQuestFromExistingWayFinder(questDefId, trackedId);
		}

		private void UpdateHUDSnap(int saleId)
		{
			View.UpdateHUDSnap(PositionService);
		}
	}
}
