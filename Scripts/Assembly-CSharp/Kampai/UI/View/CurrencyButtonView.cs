using Kampai.Game;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class CurrencyButtonView : KampaiView
	{
		public bool PlaySoundOnClick = true;

		public Text Description;

		public Text ItemPrice;

		public Text ItemWorth;

		public KampaiImage ItemImage;

		public KampaiImage CostCurrencyIcon;

		public Image GreyOut;

		public Transform VFXRoot;

		public GameObject VFXPrefab;

		public Button purchaseButton;

		public Button infoButton;

		public Button imageButton;

		public GameObject ValueBanner;

		public Text ValueBannerText;

		public KampaiImage ValueImage;

		public GameObject MoreInfoButton;

		public Signal PurchaseClickedSignal = new Signal();

		public Signal InfoClickedSignal = new Signal();

		public Animator animator;

		public RuntimeAnimatorController controller;

		public bool isStarterPack;

		public StoreItemDefinition Definition { get; set; }

		public bool isCOPPAGated { get; set; }

		protected override void Start()
		{
			base.Start();
			if (isStarterPack)
			{
				animator.runtimeAnimatorController = controller;
				return;
			}
			MoreInfoButton.gameObject.SetActive(false);
			imageButton.enabled = false;
		}

		public void OnPurchaseClickEvent()
		{
			PurchaseClickedSignal.Dispatch();
		}

		public void OnInfoClickEvent()
		{
			InfoClickedSignal.Dispatch();
		}

		public void UnlockButton(bool isEnabled)
		{
			if (purchaseButton != null)
			{
				purchaseButton.gameObject.SetActive(isEnabled);
				GreyOut.gameObject.SetActive(!isEnabled);
				infoButton.gameObject.SetActive(isEnabled);
			}
		}
	}
}
