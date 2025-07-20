using System;
using System.Collections;
using System.Collections.Generic;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;

namespace Kampai.UI.View.UpSell
{
	public class UpSellModalView : PopupMenuView
	{
		public LocalizeView title;

		public RuntimeAnimatorController freeCollectButton;

		public LocalizeView dealDescription;

		[Header("Timer Items")]
		public LocalizeView timerTitle;

		public LocalizeView offerTime;

		public Color giftTitleColor = new Color32(103, 170, 200, byte.MaxValue);

		[Header("Banner Items")]
		public GameObject bannerPanel;

		public LocalizeView percentOffBannerText;

		public UpsellPriceMarkdownView priceMarkdownView;

		[Header("Currency Icons")]
		public KampaiImage inputItemIcon;

		public DoubleConfirmButtonView purchaseCurrencyButton;

		public LocalizeView purchaseButtonCost;

		[Header("Item Prefabs")]
		public GameObject itemViewPrefab;

		[Header("Item Locations")]
		public Transform[] itemTransforms;

		[Header("Skrim")]
		public ButtonView backGroundButton;

		protected PackDefinition packDefinition;

		protected SalePackDefinition salePackDefinition;

		protected ICurrencyService currencyService;

		protected IDefinitionService definitionService;

		protected ILocalizationService localizationService;

		protected IPlayerService playerService;

		protected ITimeEventService timeEventService;

		private IList<QuantityItem> outputs;

		private IList<QuantityItem> inputs;

		private IUpsellService upsellService;

		private IEnumerator m_updateSaleTime;

		public IList<UpSellItemView> views { get; private set; }

		internal void Init(PackDefinition packDefinition, IUpsellService upsellService, IPlayerService playerService, ICurrencyService currencyService, IDefinitionService defService, ILocalizationService locService, ITimeEventService timeEventService)
		{
			base.Init();
			this.currencyService = currencyService;
			definitionService = defService;
			localizationService = locService;
			this.playerService = playerService;
			this.timeEventService = timeEventService;
			this.upsellService = upsellService;
			this.packDefinition = packDefinition;
			salePackDefinition = packDefinition as SalePackDefinition;
			if (packDefinition == null)
			{
				logger.Error("Sale Pack Definition is null for Upsell");
				Close(true);
				return;
			}
			if (packDefinition != null && packDefinition.TransactionDefinition != null)
			{
				outputs = packDefinition.TransactionDefinition.Outputs;
				inputs = packDefinition.TransactionDefinition.Inputs;
			}
			LoadSaleInfo();
			SetupPackItems();
		}

		protected void SetupTopBanner(bool isEnabled)
		{
			if (!isEnabled || percentOffBannerText == null || packDefinition.PercentagePer100 == 0)
			{
				if (!(bannerPanel == null))
				{
					bannerPanel.SetActive(false);
				}
				return;
			}
			percentOffBannerText.gameObject.SetActive(true);
			percentOffBannerText.text = string.Format("{0}%", packDefinition.PercentagePer100);
			if (!(bannerPanel == null))
			{
				bannerPanel.SetActive(true);
			}
		}

		protected bool SetMarkDownPriceBanner()
		{
			if (priceMarkdownView == null || priceMarkdownView.gameObject == null)
			{
				return false;
			}
			if (packDefinition.TransactionType == UpsellTransactionType.Cash && packDefinition.PercentagePer100 != 0)
			{
				priceMarkdownView.gameObject.SetActive(true);
				priceMarkdownView.Init(packDefinition, definitionService, currencyService);
				return true;
			}
			priceMarkdownView.gameObject.SetActive(false);
			return false;
		}

		protected void SetupBanner()
		{
			if (SetMarkDownPriceBanner())
			{
				SetupTopBanner(false);
			}
			else
			{
				SetupTopBanner(true);
			}
		}

		protected virtual void SetupPackItems()
		{
			ToggleDealDescription();
			UpdateCostText();
			if (itemTransforms != null && itemTransforms.Length > 0)
			{
				CreateItems(itemTransforms);
				SetupBanner();
			}
		}

		internal virtual void Release()
		{
			if (views != null)
			{
				for (int i = 0; i < views.Count; i++)
				{
					ReleaseItem(views[i]);
				}
				views = null;
			}
		}

		protected void LoadSaleInfo()
		{
			if (!(title == null) && !string.IsNullOrEmpty(packDefinition.LocalizedKey))
			{
				title.LocKey = packDefinition.LocalizedKey;
			}
		}

		public void ToggleDealDescription()
		{
			if (!(dealDescription == null) && !(dealDescription.gameObject == null))
			{
				if (string.IsNullOrEmpty(packDefinition.Description))
				{
					dealDescription.gameObject.SetActive(false);
				}
				else
				{
					dealDescription.LocKey = packDefinition.Description;
				}
			}
		}

		protected UpSellItemView CreateItemView(GameObject prefab, Transform parent)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
			if (gameObject == null)
			{
				logger.Error("Could not create UpSellItemView from prefab");
				return null;
			}
			UpSellItemView component = gameObject.GetComponent<UpSellItemView>();
			if (component == null)
			{
				logger.Error("Could not get UpSellItemView from prefab");
				return null;
			}
			component.transform.SetParent(parent);
			SetUpItemTransform(component.transform as RectTransform);
			return component;
		}

		private void SetUpItemTransform(RectTransform rect)
		{
			if (!(rect == null))
			{
				rect.anchorMin = Vector2.zero;
				rect.anchorMax = Vector2.one;
				rect.sizeDelta = Vector2.zero;
				rect.localScale = Vector3.one;
				rect.localPosition = Vector3.zero;
			}
		}

		protected void ReleaseItem(UpSellItemView itemView)
		{
			if (!(itemView == null))
			{
				itemView.Release();
				UnityEngine.Object.Destroy(itemView);
			}
		}

		protected void CreateItems(params Transform[] itemParents)
		{
			if (outputs != null && itemParents != null && outputs.Count != 0 && itemParents.Length != 0)
			{
				views = new UpSellItemView[itemParents.Length];
				for (int i = 0; i < itemParents.Length; i++)
				{
					UpSellItemView itemView = CreateItemView(itemViewPrefab, itemParents[i]);
					SetupItemView(i, itemView);
				}
			}
		}

		protected void SetupItemView(int i, UpSellItemView itemView)
		{
			if (!(itemView == null))
			{
				views[i] = itemView;
				itemView.Item = upsellService.GetItemDef(i, outputs);
				if (itemView.Item != null)
				{
					bool audible = PackUtil.IsAudible(itemView.Item.ID, packDefinition);
					bool isExclusive = PackUtil.IsItemExclusive(itemView.Item.ID, packDefinition);
					itemView.SetAudible(audible);
					itemView.SetIsExclusive(isExclusive);
					itemView.ShowMtxID(packDefinition.CurrencyImageID);
					Rect rect = (itemTransforms[0].transform as RectTransform).rect;
					Rect rect2 = (itemView.transform.parent.transform as RectTransform).rect;
					itemView.AdditionalUIScale = Mathf.Min(rect2.width / rect.width, rect2.height / rect.height);
				}
			}
		}

		protected void UpdateCostText()
		{
			bool isCash = false;
			string text = SetPrice(out isCash);
			if (!string.IsNullOrEmpty(text))
			{
				if (!isCash || string.Compare(text, localizationService.GetString("StoreBuy"), StringComparison.Ordinal) == 0)
				{
					purchaseButtonCost.text = text;
				}
				else
				{
					purchaseButtonCost.text = string.Format(purchaseButtonCost.text, text);
				}
			}
		}

		protected void SetFreeItem()
		{
			upsellService.SetupFreeItemButton(purchaseButtonCost, purchaseCurrencyButton, "Collect", freeCollectButton);
			ToggleDealDescription();
		}

		protected void SetupBuyButton()
		{
			if (inputs == null || inputs.Count == 0)
			{
				return;
			}
			QuantityItem quantityItem = inputs[0];
			if (quantityItem == null)
			{
				logger.Error(string.Format("Item input is null for sale pack definition: {0}", packDefinition));
				return;
			}
			ItemDefinition itemDefinition = definitionService.Get<ItemDefinition>(quantityItem.ID);
			if (itemDefinition == null)
			{
				logger.Error(string.Format("Item input is not an item definition for sale pack definition: {0}", packDefinition));
				return;
			}
			inputItemIcon.gameObject.SetActive(true);
			UIUtils.SetItemIcon(inputItemIcon, itemDefinition);
		}

		private string SetPrice(out bool isCash)
		{
			isCash = false;
			if (inputItemIcon != null && inputItemIcon.gameObject != null)
			{
				inputItemIcon.gameObject.SetActive(false);
			}
			bool flag = !string.IsNullOrEmpty(packDefinition.SKU);
			bool flag2 = packDefinition.PercentagePer100 > 0;
			bool flag3 = inputs.Count > 0;
			inputItemIcon.gameObject.SetActive(false);
			if (salePackDefinition != null && salePackDefinition.Type == SalePackType.Redeemable)
			{
				SetFreeItem();
				SetTimerTitle("Gift", false);
				return string.Empty;
			}
			if (flag3)
			{
				if (inputs[0].ID == 1)
				{
					purchaseCurrencyButton.EnableDoubleConfirm();
				}
				if (flag2)
				{
					int transactionCurrencyCost = TransactionUtil.GetTransactionCurrencyCost(packDefinition.TransactionDefinition.ToDefinition(), definitionService, playerService, (StaticItem)inputs[0].ID);
					SetupBuyButton();
					SetTimerTitle("UpSellTimeLeft", true);
					return ((int)((float)transactionCurrencyCost * packDefinition.getDiscountRate())).ToString();
				}
				SetupBuyButton();
				SetTimerTitle("UpSellTimeLeft", true);
				return upsellService.SumOutput(inputs, inputs[0].ID).ToString();
			}
			if (flag)
			{
				isCash = true;
				SetTimerTitle("UpSellTimeLeft", true);
				return currencyService.GetPriceWithCurrencyAndFormat(packDefinition.SKU);
			}
			if (!flag3)
			{
				SetFreeItem();
				SetTimerTitle("Gift", false);
				return string.Empty;
			}
			return string.Empty;
		}

		protected void SetTimerTitle(string locKey, bool startTimeUpdater)
		{
			if (timerTitle == null)
			{
				logger.Error("timerTitle GameObject is null");
				return;
			}
			if (!string.IsNullOrEmpty(packDefinition.BannerAd))
			{
				timerTitle.LocKey = packDefinition.BannerAd;
				if (offerTime != null && offerTime.gameObject != null)
				{
					offerTime.gameObject.SetActive(false);
				}
			}
			if (salePackDefinition == null || salePackDefinition.Duration == 0)
			{
				return;
			}
			if (offerTime == null)
			{
				logger.Error("offerTime GameObject is null");
				return;
			}
			if (salePackDefinition.TransactionType != UpsellTransactionType.Free && string.IsNullOrEmpty(salePackDefinition.BannerAd))
			{
				SetUpTimer(locKey, startTimeUpdater);
				return;
			}
			if (offerTime != null && offerTime.gameObject != null)
			{
				offerTime.gameObject.SetActive(false);
			}
			timerTitle.LocKey = ((!string.IsNullOrEmpty(salePackDefinition.BannerAd)) ? salePackDefinition.BannerAd : locKey);
		}

		private void SetUpTimer(string locKey, bool startTimeUpdater)
		{
			timerTitle.LocKey = locKey;
			if (!startTimeUpdater || offerTime == null)
			{
				if (offerTime != null && offerTime.gameObject != null)
				{
					offerTime.gameObject.SetActive(false);
				}
			}
			else
			{
				offerTime.gameObject.SetActive(true);
				m_updateSaleTime = UpdateSaleTime("UpSellTimeLeftFormat");
				StartCoroutine(m_updateSaleTime);
			}
		}

		public override void Close(bool instant = false)
		{
			if (m_updateSaleTime != null)
			{
				StopCoroutine(m_updateSaleTime);
			}
			base.Close(instant);
		}

		internal IEnumerator UpdateSaleTime(string timeLocKey)
		{
			bool isValidString = true;
			while (isValidString)
			{
				if (offerTime == null)
				{
					isValidString = false;
					continue;
				}
				offerTime.LocKey = salePackDefinition.BannerAd;
				Sale saleItem = playerService.GetFirstInstanceByDefinitionId<Sale>(salePackDefinition.ID);
				if (saleItem != null)
				{
					int saleTime = timeEventService.GetTimeRemaining(saleItem.ID);
					offerTime.LocKey = timeLocKey;
					string saleTimeStr;
					if (saleTime <= 0)
					{
						saleTimeStr = UIUtils.FormatTime(0, localizationService);
						isValidString = false;
					}
					else
					{
						saleTimeStr = UIUtils.FormatTime(saleTime, localizationService);
					}
					offerTime.text = string.Format(offerTime.text, saleTimeStr);
				}
				yield return new WaitForSeconds(1f);
			}
			m_updateSaleTime = null;
		}
	}
}
