using System;
using Kampai.Game;
using Kampai.Game.Transaction;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	[RequireComponent(typeof(Animator))]
	[RequireComponent(typeof(Button))]
	public class UpsellButtonView : DoubleConfirmButtonView
	{
		public enum Type
		{
			Free = 0,
			ItemInput = 1,
			Cash = 2
		}

		public Type type;

		public Button button;

		public KampaiImage inputItemIcon;

		public LocalizeView costText;

		[Header("Animators")]
		[Tooltip("If this is not set asm_buttonClick_Tertiary is used by default")]
		public RuntimeAnimatorController freeCollectButtonAnimator;

		protected ICurrencyService currencyService;

		protected IDefinitionService definitionService;

		protected ILocalizationService localizationService;

		protected IKampaiLogger logger;

		protected IPlayerService playerService;

		protected IUpsellService upsellService;

		private TransactionInstance transactionInstance;

		protected override void Awake()
		{
			base.Awake();
			button = GetComponent<Button>();
		}

		internal void Init(IUpsellService upsellService, IPlayerService playerService, ICurrencyService currencyService, IDefinitionService defService, ILocalizationService locService)
		{
			this.upsellService = upsellService;
			this.currencyService = currencyService;
			definitionService = defService;
			localizationService = locService;
			this.playerService = playerService;
			DisableDoubleConfirm();
		}

		public override void Disable()
		{
			ResetTapState();
			button.interactable = false;
		}

		public void SetupButton(TransactionInstance transactionInstance, string SKU, int percentagePer100, bool isFree, string buttonLocKey)
		{
			this.transactionInstance = transactionInstance;
			string text = SetPrice(SKU, percentagePer100, isFree, buttonLocKey);
			if (type != Type.Cash || string.Compare(text, localizationService.GetString("StoreBuy"), StringComparison.Ordinal) == 0)
			{
				costText.text = text;
				return;
			}
			costText.Format(costText.text, text);
		}

		public string SetPrice(string SKU, int PercentagePer100, bool isFree, string buttonLocKey)
		{
			EnableInputIcon(false);
			bool flag = !string.IsNullOrEmpty(SKU);
			bool flag2 = PercentagePer100 > 0;
			bool flag3 = transactionInstance.GetInputCount() > 0;
			if (isFree)
			{
				if (string.IsNullOrEmpty(buttonLocKey))
				{
					buttonLocKey = "Collect";
				}
				SetupFreeItemButton(buttonLocKey, freeCollectButtonAnimator);
				type = Type.Free;
				return localizationService.GetString(buttonLocKey);
			}
			if (flag3)
			{
				QuantityItem inputItem = transactionInstance.GetInputItem(0);
				type = Type.ItemInput;
				if (inputItem == null)
				{
					return string.Empty;
				}
				if (inputItem.ID == 1)
				{
					EnableDoubleConfirm();
				}
				if (flag2)
				{
					int transactionCurrencyCost = TransactionUtil.GetTransactionCurrencyCost(transactionInstance.ToDefinition(), definitionService, playerService, (StaticItem)inputItem.ID);
					SetupBuyButton(inputItem.ID);
					return ((int)((float)transactionCurrencyCost * (1f - (float)PercentagePer100 / 100f))).ToString();
				}
				SetupBuyButton(inputItem.ID);
				return upsellService.SumOutput(transactionInstance.Inputs, inputItem.ID).ToString();
			}
			if (flag)
			{
				type = Type.Cash;
				return currencyService.GetPriceWithCurrencyAndFormat(SKU);
			}
			return string.Empty;
		}

		private void EnableInputIcon(bool isEnabled)
		{
			if (inputItemIcon != null && inputItemIcon.gameObject != null)
			{
				inputItemIcon.gameObject.SetActive(isEnabled);
			}
		}

		public void SetupBuyButton(int itemId)
		{
			ItemDefinition itemDefinition = definitionService.Get<ItemDefinition>(itemId);
			if (itemDefinition == null)
			{
				logger.Error(string.Format("Item input is not an item definition for quantity item: {0}", itemId));
			}
			else
			{
				SetInputIcon(itemDefinition, inputItemIcon);
			}
		}

		private static void SetInputIcon(ItemDefinition itemDefinition, KampaiImage inputItemIcon)
		{
			inputItemIcon.gameObject.SetActive(true);
			UIUtils.SetItemIcon(inputItemIcon, itemDefinition);
		}

		public void SetupFreeItemButton(string buttonLocKey, RuntimeAnimatorController freeCollectButtonAnimator)
		{
			if (freeCollectButtonAnimator == null)
			{
				freeCollectButtonAnimator = KampaiResources.Load<RuntimeAnimatorController>("asm_buttonClick_Tertiary");
			}
			SetCostText(buttonLocKey, logger);
			if (animator == null || freeCollectButtonAnimator == null)
			{
				logger.Error(string.Format("Button animator is null: {0}", this));
			}
			else
			{
				animator.runtimeAnimatorController = freeCollectButtonAnimator;
			}
		}

		public void SetCostText(string buttonLocKey, IKampaiLogger logger)
		{
			if (costText == null)
			{
				if (costText == null)
				{
					logger.Error("Purchase Button Cost GameObject is null");
				}
			}
			else
			{
				costText.LocKey = buttonLocKey;
			}
		}
	}
}
