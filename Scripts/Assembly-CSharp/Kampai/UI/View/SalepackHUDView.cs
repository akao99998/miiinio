using Kampai.Game;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class SalepackHUDView : KampaiView
	{
		public ButtonView SalePackButton;

		public Transform VFXRoot;

		public Text ItemText;

		public KampaiImage ItemIcon;

		public Signal closeSignal = new Signal();

		public Sale SalePackItem { get; set; }

		public void Init()
		{
		}

		public void SetupIcon(SalePackDefinition salePackDefinition)
		{
			if (!string.IsNullOrEmpty(salePackDefinition.GlassIconImage) && !string.IsNullOrEmpty(salePackDefinition.GlassIconMask))
			{
				ItemIcon.sprite = UIUtils.LoadSpriteFromPath(salePackDefinition.GlassIconImage);
				ItemIcon.maskSprite = UIUtils.LoadSpriteFromPath(salePackDefinition.GlassIconMask);
			}
		}

		public void Close()
		{
			closeSignal.Dispatch();
		}
	}
}
