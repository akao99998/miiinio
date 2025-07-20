using Kampai.Game;
using Kampai.Util;
using UnityEngine;

namespace Kampai.UI.View
{
	public class StickerbookStickerView : KampaiView
	{
		public PopupInfoButtonView buttonView;

		public KampaiImage image;

		public KampaiImage lockImage;

		public bool locked;

		public int stickerDefinitionID;

		internal StickerDefinition stickerDefinition;

		internal void Init(IDefinitionService definitionService)
		{
			stickerDefinition = definitionService.Get<StickerDefinition>(stickerDefinitionID);
			if (locked)
			{
				lockImage.gameObject.SetActive(true);
				image.sprite = UIUtils.LoadSpriteFromPath("img_fill_128");
			}
			else
			{
				image.color = Color.white;
				lockImage.gameObject.SetActive(false);
				image.sprite = UIUtils.LoadSpriteFromPath(stickerDefinition.Image);
			}
			image.maskSprite = UIUtils.LoadSpriteFromPath(stickerDefinition.Mask);
		}
	}
}
