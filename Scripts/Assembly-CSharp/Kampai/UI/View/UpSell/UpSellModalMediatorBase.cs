using Kampai.Common;
using Kampai.Common.Service.Telemetry;
using Kampai.Game;
using Kampai.Main;
using UnityEngine.UI;

namespace Kampai.UI.View.UpSell
{
	public class UpSellModalMediatorBase<T> : UIStackMediator<T>, IDefinitionsHotSwapHandler where T : UpSellModalView
	{
		private PackDefinition m_packDefinition;

		private string m_prefabName;

		private bool m_disableSkrimButton;

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public ICurrencyService currencyService { get; set; }

		[Inject]
		public PurchaseSalePackSignal purchaseSignal { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public OpenUpSellModalSignal openUpSellModalSignal { get; set; }

		[Inject]
		public IUpsellService upsellService { get; set; }

		[Inject]
		public EndSaleSignal endSaleSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playSFXSignal { get; set; }

		public override void Initialize(GUIArguments args)
		{
			m_packDefinition = args.Get<PackDefinition>();
			T val = base.view;
			val.Init(m_packDefinition, upsellService, playerService, currencyService, definitionService, localizationService, timeEventService);
			m_prefabName = args.Get<string>();
			m_disableSkrimButton = args.Get<bool>();
			if (m_disableSkrimButton)
			{
				base.view.backGroundButton.gameObject.SetActive(false);
			}
			T val2 = base.view;
			val2.Open();
			playSFXSignal.Dispatch("Play_menu_popUp_01");
			telemetryService.Send_Telemetry_EVT_UPSELL(m_packDefinition.SKU, UpsellStatus.Viewed);
		}

		public override void OnRegister()
		{
			base.OnRegister();
			endSaleSignal.AddListener(SaleEnded);
			base.view.OnMenuClose.AddListener(OnMenuClose);
			base.view.purchaseCurrencyButton.ClickedSignal.AddListener(OnPurchaseButtonClicked);
			base.view.backGroundButton.ClickedSignal.AddListener(Close);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			endSaleSignal.RemoveListener(SaleEnded);
			base.view.OnMenuClose.RemoveListener(OnMenuClose);
			base.view.purchaseCurrencyButton.ClickedSignal.RemoveListener(OnPurchaseButtonClicked);
			base.view.backGroundButton.ClickedSignal.RemoveListener(Close);
		}

		protected override void Close()
		{
			playSFXSignal.Dispatch("Play_menu_disappear_01");
			T val = base.view;
			val.Release();
			T val2 = base.view;
			val2.Close();
		}

		private void OnMenuClose()
		{
			guiService.Execute(GUIOperation.Unload, m_prefabName);
		}

		private void SaleEnded(int saleInstance)
		{
			base.view.purchaseCurrencyButton.GetComponent<Button>().enabled = false;
			Close();
		}

		private void OnPurchaseButtonClicked()
		{
			Close();
			purchaseSignal.Dispatch(m_packDefinition.ID);
			telemetryService.Send_Telemetry_EVT_UPSELL(m_packDefinition.SKU, UpsellStatus.Clicked);
		}

		public virtual void OnDefinitionsHotSwap(IDefinitionService definitionService)
		{
			if (m_packDefinition != null)
			{
				m_packDefinition = definitionService.Get<PackDefinition>(m_packDefinition.ID);
				SalePackDefinition salePackDefinition = m_packDefinition as SalePackDefinition;
				if (salePackDefinition != null)
				{
					Sale saleInstanceFromID = upsellService.GetSaleInstanceFromID(playerService.GetInstancesByType<Sale>(), salePackDefinition.ID);
					saleInstanceFromID.OnDefinitionHotSwap(salePackDefinition);
				}
				Close();
				openUpSellModalSignal.Dispatch(m_packDefinition, m_prefabName, false);
			}
		}
	}
}
