using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Main;
using Kampai.Util;
using strange.extensions.context.api;

namespace Kampai.UI.View
{
	public class DebrisModalMediator : UIStackMediator<DebrisModalView>
	{
		private DebrisBuilding debrisBuilding;

		private TransactionDefinition transactionDef;

		private int itemsRequired;

		private int itemDefId;

		private ToggleVignetteSignal toggleVignetteSignal;

		private IdleMinionSignal idleMinionSignal;

		[Inject]
		public CallMinionSignal CallMinionSignal { get; set; }

		[Inject]
		public IDefinitionService DefinitionService { get; set; }

		[Inject]
		public IPlayerService PlayerService { get; set; }

		[Inject]
		public SetStorageCapacitySignal setStorageSignal { get; set; }

		[Inject]
		public HideSkrimSignal HideSignal { get; set; }

		[Inject]
		public IGUIService GUIService { get; set; }

		[Inject]
		public ILocalizationService LocalizationService { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable GameContext { get; set; }

		[Inject]
		public OnDropItemOverDropAreaSignal OnDropItemOverDropAreaSignal { get; set; }

		[Inject]
		public OnDragItemOverDropAreaSignal OnDragItemOverDropAreaSignal { get; set; }

		[Inject]
		public RushDialogConfirmationSignal RushedSignal { get; set; }

		[Inject]
		public ShowNeedXMinionsSignal ShowNeedXMinionsSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			base.view.OnMenuClose.AddListener(OnMenuClose);
			RushedSignal.AddListener(OnPurchaseItem);
			OnDragItemOverDropAreaSignal.AddListener(OnDragItemOverDropArea);
			OnDropItemOverDropAreaSignal.AddListener(OnDropItemOverDropArea);
			toggleVignetteSignal = GameContext.injectionBinder.GetInstance<ToggleVignetteSignal>();
			toggleVignetteSignal.Dispatch(true, 0f);
			idleMinionSignal = GameContext.injectionBinder.GetInstance<IdleMinionSignal>();
			idleMinionSignal.AddListener(UpdateMinionsAvailable);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			base.view.OnMenuClose.RemoveListener(OnMenuClose);
			RushedSignal.RemoveListener(OnPurchaseItem);
			OnDragItemOverDropAreaSignal.RemoveListener(OnDragItemOverDropArea);
			OnDropItemOverDropAreaSignal.RemoveListener(OnDropItemOverDropArea);
			toggleVignetteSignal.Dispatch(false, null);
			idleMinionSignal.RemoveListener(UpdateMinionsAvailable);
		}

		private void UpdateMinionsAvailable()
		{
			int count = PlayerService.GetIdleMinions().Count;
			base.view.UpdateAvailableMinions(count);
		}

		private void OnDragItemOverDropArea(DragDropItemView dragDropItemView, bool success)
		{
			base.view.OnDragItemOverDropArea(dragDropItemView, success);
		}

		private void OnDropItemOverDropArea(DragDropItemView dragDropItemView, bool success)
		{
			base.view.OnDropItemOverDropArea(dragDropItemView, success);
			if (success)
			{
				if (base.view.MinionsAvailable < 1)
				{
					ShowNeedXMinionsSignal.Dispatch(1);
				}
				else
				{
					RunTransaction();
				}
			}
		}

		public override void Initialize(GUIArguments args)
		{
			DebrisBuilding debrisBuilding = args.Get<DebrisBuilding>();
			if (debrisBuilding != null)
			{
				this.debrisBuilding = debrisBuilding;
				transactionDef = DefinitionService.Get<TransactionDefinition>(this.debrisBuilding.GetTransactionID(DefinitionService));
				int count = PlayerService.GetIdleMinions().Count;
				QuantityItem quantityItem = transactionDef.Inputs[0];
				itemDefId = quantityItem.ID;
				itemsRequired = (int)quantityItem.Quantity;
				int quantityByDefinitionId = (int)PlayerService.GetQuantityByDefinitionId(itemDefId);
				ItemDefinition itemDefinition = DefinitionService.Get<ItemDefinition>(itemDefId);
				string image = itemDefinition.Image;
				string mask = itemDefinition.Mask;
				base.view.Init(count, quantityByDefinitionId, itemsRequired, image, mask, LocalizationService, this.debrisBuilding.Definition);
				base.Initialize(args);
			}
		}

		private void OnPurchaseItem()
		{
			RunTransaction();
		}

		private void RunTransaction()
		{
			PlayerService.StartTransaction(transactionDef, TransactionTarget.CLEAR_DEBRIS, TransactionCallback, new TransactionArg(debrisBuilding.ID));
		}

		private void PerformTransactionSuccessAction()
		{
			CallMinions();
			setStorageSignal.Dispatch();
		}

		protected override void Close()
		{
			soundFXSignal.Dispatch("Play_menu_disappear_01");
			base.view.Close();
		}

		private void CallMinions()
		{
			int minionSlotsOwned = debrisBuilding.MinionSlotsOwned;
			for (int i = 0; i < minionSlotsOwned; i++)
			{
				CallMinionSignal.Dispatch(debrisBuilding, base.view.gameObject);
			}
		}

		private void TransactionCallback(PendingCurrencyTransaction pct)
		{
			if (pct.Success)
			{
				PlayerService.FinishTransaction(pct.GetPendingTransaction(), TransactionTarget.CLEAR_DEBRIS, new TransactionArg(debrisBuilding.ID));
				PerformTransactionSuccessAction();
				Close();
			}
		}

		private void OnMenuClose()
		{
			HideSignal.Dispatch("DebrisSkrim");
			GUIService.Execute(GUIOperation.Unload, "screen_ClearDebris");
		}
	}
}
