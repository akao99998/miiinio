using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Game;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.mediation.impl;

namespace Kampai.UI.View
{
	public class SalePackHUDPanelMediator : EventMediator
	{
		private IList<Sale> playerSales;

		private IList<SalepackHUDView> playerSaleItems;

		private StartSaleSignal startSaleSignal;

		private EndSaleSignal endSaleSignal;

		public IKampaiLogger logger = LogManager.GetClassLogger("SalePackHUDPanelMediator") as IKampaiLogger;

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public RemoveSalePackSignal removeSalePackSignal { get; set; }

		[Inject]
		public StartUpSellImpressionSignal startUpSellImpressionSignal { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public SalePackHUDPanelView view { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			view.Init();
			removeSalePackSignal.AddListener(RemoveSalePack);
			startSaleSignal = gameContext.injectionBinder.GetInstance<StartSaleSignal>();
			endSaleSignal = gameContext.injectionBinder.GetInstance<EndSaleSignal>();
			startSaleSignal.AddListener(AddSalePack);
			endSaleSignal.AddListener(RemoveSalePack);
			LoadItems();
		}

		public override void OnRemove()
		{
			removeSalePackSignal.RemoveListener(RemoveSalePack);
			startSaleSignal.RemoveListener(AddSalePack);
			endSaleSignal.RemoveListener(RemoveSalePack);
		}

		internal void LoadItems()
		{
			if (playerSaleItems != null)
			{
				for (int num = playerSaleItems.Count - 1; num >= 0; num--)
				{
					playerSaleItems[num].Close();
					Object.Destroy(playerSaleItems[num].gameObject);
					playerSaleItems.Remove(playerSaleItems[num]);
				}
			}
			playerSales = playerService.GetInstancesByType<Sale>();
			foreach (Sale playerSale in playerSales)
			{
				if (playerSale.Started && !playerSale.Purchased && !playerSale.Finished)
				{
					AddSalePack(playerSale);
				}
			}
			UpdateScrollState();
		}

		public void AddSalePack(int instanceID)
		{
			Sale byInstanceId = playerService.GetByInstanceId<Sale>(instanceID);
			if (byInstanceId != null)
			{
				AddSalePack(byInstanceId, false);
			}
		}

		public void AddSalePack(Sale salePackItem, bool tryImpression = true)
		{
			if (playerSaleItems == null)
			{
				playerSaleItems = new List<SalepackHUDView>();
			}
			if (salePackItem.Definition.Type == SalePackType.Upsell)
			{
				SalepackHUDView salepackHUDView = BuildSaleItem(salePackItem);
				salepackHUDView.transform.SetParent(view.scrollList.content, false);
				salepackHUDView.transform.localScale = new Vector3(1f, 1f, 1f);
				if (tryImpression && salePackItem.Definition.Impressions != 0 && salePackItem.Impressions < salePackItem.Definition.Impressions)
				{
					SalePackDefinition definition = salePackItem.Definition;
					timeEventService.AddEvent(definition.ID, salePackItem.UTCLastImpressionTime, definition.ImpressionInterval, startUpSellImpressionSignal);
				}
				playerSaleItems.Add(salepackHUDView);
				UpdateScrollState();
			}
		}

		public void RemoveSalePack(int instanceID)
		{
			if (playerSaleItems == null)
			{
				return;
			}
			for (int num = playerSaleItems.Count - 1; num >= 0; num--)
			{
				if (playerSaleItems[num].SalePackItem.ID == instanceID)
				{
					if (playerSales != null && playerSales.Contains(playerSaleItems[num].SalePackItem))
					{
						playerSales.Remove(playerSaleItems[num].SalePackItem);
					}
					Object.Destroy(playerSaleItems[num].gameObject);
					playerSaleItems.Remove(playerSaleItems[num]);
				}
			}
			UpdateScrollState();
		}

		public SalepackHUDView BuildSaleItem(Sale item)
		{
			if (item.Definition == null)
			{
				logger.Fatal(FatalCode.EX_NULL_ARG);
			}
			string path = "HUD_Upsell";
			GameObject gameObject = KampaiResources.Load(path) as GameObject;
			if (gameObject == null)
			{
				logger.Error("Unable to load SalePack HUD prefab.");
				return null;
			}
			GameObject gameObject2 = Object.Instantiate(gameObject);
			SalepackHUDView component = gameObject2.GetComponent<SalepackHUDView>();
			component.SalePackItem = item;
			component.SetupIcon(item.Definition);
			return component;
		}

		private void UpdateScrollState()
		{
			if (playerSaleItems == null || playerSaleItems.Count == 1)
			{
				view.scrollList.enabled = false;
			}
			else
			{
				view.scrollList.enabled = true;
			}
		}
	}
}
