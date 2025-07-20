using Kampai.Game;
using UnityEngine;

namespace Kampai.UI.View.UpSell
{
	public class UpSellMessagingModalView : PopupMenuView
	{
		[Header("Base Upsell Message")]
		public LocalizeView Title;

		public LocalizeView Description;

		public ButtonView GoToButton;

		public GameObject GoToImage;

		public GameObject GoToText;

		public GameObject GotItText;

		public ButtonView backGroundButton;

		[Header("UpSell Image")]
		public KampaiImage UpsellMessageImage;

		protected SalePackDefinition salePackDefinition;

		internal void Init(SalePackDefinition sale)
		{
			base.Init();
			salePackDefinition = sale;
			if (salePackDefinition == null)
			{
				logger.Error("Sale Pack Definition is null for Upsell Messaging");
				Close(true);
			}
			else
			{
				LoadSaleInfo();
			}
		}

		protected void LoadSaleInfo()
		{
			SetupLocale();
			bool flag = salePackDefinition.MessageLinkType == SalePackMessageLinkType.None;
			GotItText.SetActive(flag);
			GoToImage.SetActive(!flag);
			GoToText.SetActive(!flag);
			if (salePackDefinition.MessageType == SalePackMessageType.Image)
			{
				SetupImageMessage();
			}
		}

		private void SetupLocale()
		{
			if (!(Title == null) && !string.IsNullOrEmpty(salePackDefinition.LocalizedKey))
			{
				Title.LocKey = salePackDefinition.LocalizedKey;
				if (!(Description == null) && !string.IsNullOrEmpty(salePackDefinition.Description))
				{
					Description.LocKey = salePackDefinition.Description;
				}
			}
		}

		private void SetupImageMessage()
		{
			UpsellMessageImage.sprite = UIUtils.LoadSpriteFromPath(salePackDefinition.MessageImage);
			UpsellMessageImage.maskSprite = UIUtils.LoadSpriteFromPath(salePackDefinition.MessageMask);
		}
	}
}
