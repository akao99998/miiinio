using Elevation.Logging;
using Kampai.Common;
using Kampai.Common.Service.Telemetry;
using Kampai.Game;
using Kampai.Util;
using UnityEngine;

namespace Kampai.UI.View.UpSell
{
	public class UpSellMessagingModalMediator : UIStackMediator<UpSellMessagingModalView>
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("UpSellMessagingModalMediator") as IKampaiLogger;

		private SalePackDefinition m_salePackDefinition;

		private string m_prefabName;

		private int m_salePackInstanceID;

		private bool needImpression = true;

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public HideSkrimSignal hideSignal { get; set; }

		[Inject]
		public ITelemetryService telemetryService { get; set; }

		[Inject]
		public PurchaseSalePackSignal purchaseSalePackSignal { get; set; }

		[Inject]
		public FinishPurchasingSalePackSignal finishPurchaseSignal { get; set; }

		[Inject]
		public CloseAllOtherMenuSignal closeOtherMenuSignal { get; set; }

		[Inject]
		public UpdateSaleBadgeSignal updateSaleBadgeSignal { get; set; }

		[Inject]
		public MoveBuildMenuSignal moveBuildMenuSignal { get; set; }

		[Inject]
		public ShowMTXStoreSignal showMTXStoreSignal { get; set; }

		[Inject]
		public ProcessUpSellImpressionSignal processUpSellImpressionSignal { get; set; }

		public override void Initialize(GUIArguments args)
		{
			m_salePackDefinition = args.Get<PackDefinition>() as SalePackDefinition;
			base.view.Init(m_salePackDefinition);
			m_prefabName = args.Get<string>();
			base.view.Open();
			closeOtherMenuSignal.Dispatch(base.gameObject);
			telemetryService.Send_Telemetry_EVT_UPSELL(m_salePackDefinition.SKU, UpsellStatus.Viewed);
			UpdateStoreBadge();
		}

		private void UpdateStoreBadge()
		{
			Sale firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<Sale>(m_salePackDefinition.ID);
			if (firstInstanceByDefinitionId != null)
			{
				m_salePackInstanceID = firstInstanceByDefinitionId.ID;
				if (firstInstanceByDefinitionId.Started && !firstInstanceByDefinitionId.Viewed)
				{
					firstInstanceByDefinitionId.Viewed = true;
					updateSaleBadgeSignal.Dispatch();
				}
			}
		}

		public override void OnRegister()
		{
			base.OnRegister();
			base.view.OnMenuClose.AddListener(OnMenuClose);
			base.view.GoToButton.ClickedSignal.AddListener(OnGotoButtonClicked);
			base.view.backGroundButton.ClickedSignal.AddListener(Close);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			base.view.OnMenuClose.RemoveListener(OnMenuClose);
			base.view.GoToButton.ClickedSignal.RemoveListener(OnGotoButtonClicked);
			base.view.backGroundButton.ClickedSignal.RemoveListener(Close);
		}

		protected override void Close()
		{
			if (needImpression)
			{
				processUpSellImpressionSignal.Dispatch(m_salePackInstanceID);
			}
			base.view.Close();
		}

		private void OnMenuClose()
		{
			hideSignal.Dispatch("UpSellModalSkrim");
			guiService.Execute(GUIOperation.Unload, m_prefabName);
		}

		private void OnGotoButtonClicked()
		{
			int iD = m_salePackDefinition.ID;
			if (m_salePackDefinition.TransactionDefinition != null && m_salePackDefinition.TransactionDefinition.Outputs != null)
			{
				logger.Info("Processing Transaction...");
				purchaseSalePackSignal.Dispatch(iD);
			}
			else
			{
				finishPurchaseSignal.Dispatch(iD);
			}
			switch (m_salePackDefinition.MessageLinkType)
			{
			case SalePackMessageLinkType.HTML:
				logger.Info("Launching Sale Message Link {0}", m_salePackDefinition.MessageUrl);
				Application.OpenURL(m_salePackDefinition.MessageUrl);
				break;
			case SalePackMessageLinkType.Store:
				showMTXStoreSignal.Dispatch(new Tuple<int, int>(800003, 0));
				break;
			case SalePackMessageLinkType.BuildMenu:
				moveBuildMenuSignal.Dispatch(true);
				break;
			default:
				logger.Error("Unknown SalePackMessageLinkType {0}", m_salePackDefinition.MessageLinkType);
				break;
			case SalePackMessageLinkType.None:
				break;
			}
			needImpression = false;
			Close();
			telemetryService.Send_Telemetry_EVT_UPSELL(m_salePackDefinition.SKU, UpsellStatus.Clicked);
		}
	}
}
