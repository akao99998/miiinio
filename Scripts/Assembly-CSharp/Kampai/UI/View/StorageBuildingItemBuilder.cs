using Kampai.Game;
using Kampai.Util;
using UnityEngine;

namespace Kampai.UI.View
{
	public static class StorageBuildingItemBuilder
	{
		public static StorageBuildingItemView Build(Item item, Definition definition, int count, IKampaiLogger logger)
		{
			if (definition == null)
			{
				logger.Fatal(FatalCode.EX_NULL_ARG);
			}
			GameObject original = KampaiResources.Load("cmp_StorageBuildingItem") as GameObject;
			GameObject gameObject = Object.Instantiate(original);
			StorageBuildingItemView component = gameObject.GetComponent<StorageBuildingItemView>();
			ItemDefinition itemDefinition = definition as ItemDefinition;
			if (string.IsNullOrEmpty(itemDefinition.Mask) || string.IsNullOrEmpty(itemDefinition.Image))
			{
				logger.Log(KampaiLogLevel.Error, "Your ItemDefinition: {0} doesn't have an image/mask image defined", itemDefinition.ID);
				itemDefinition.Mask = "btn_Circle01_mask";
				itemDefinition.Mask = "btn_Circle01_mask";
			}
			Sprite sprite = UIUtils.LoadSpriteFromPath(itemDefinition.Image);
			Sprite maskSprite = UIUtils.LoadSpriteFromPath(itemDefinition.Mask);
			component.StorageItem = item;
			component.ItemIcon.sprite = sprite;
			component.ItemIcon.maskSprite = maskSprite;
			component.ItemQuantity.text = count.ToString();
			return component;
		}
	}
}
