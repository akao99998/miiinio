using Kampai.Game;
using Kampai.Util;
using UnityEngine;

namespace Kampai.UI.View
{
	public static class StoreTabBuilder
	{
		public static StoreTabView Build(StoreTab tab, Transform i_parent, IKampaiLogger logger)
		{
			if (tab == null)
			{
				logger.Fatal(FatalCode.EX_NULL_ARG);
			}
			GameObject original = KampaiResources.Load("cmp_MainMenuItem") as GameObject;
			GameObject gameObject = Object.Instantiate(original);
			StoreTabView component = gameObject.GetComponent<StoreTabView>();
			component.Type = tab.Type;
			component.TabIcon.maskSprite = SetTabIcon(tab.Type, logger);
			component.TabName.text = tab.LocalizedName;
			RectTransform rectTransform = gameObject.transform as RectTransform;
			rectTransform.SetParent(i_parent);
			rectTransform.SetAsFirstSibling();
			rectTransform.localPosition = new Vector3(rectTransform.localPosition.x, rectTransform.localPosition.y, 0f);
			rectTransform.localScale = Vector3.one;
			gameObject.SetActive(false);
			return component;
		}

		public static Sprite SetTabIcon(StoreItemType type, IKampaiLogger logger)
		{
			string text = null;
			switch (type)
			{
			case StoreItemType.BaseResource:
				text = "make";
				break;
			case StoreItemType.Crafting:
				text = "mix";
				break;
			case StoreItemType.Decoration:
				text = "decor";
				break;
			case StoreItemType.Connectable:
				text = "connectable";
				break;
			case StoreItemType.Leisure:
				text = "leisure";
				break;
			case StoreItemType.Featured:
			case StoreItemType.MasterPlanLeftOvers:
				text = "villainLair";
				break;
			case StoreItemType.SpecialEvent:
				text = "event";
				break;
			default:
				logger.Error("store tab key doesn't exist for StoreItemTyoe: {0}", type);
				text = "event";
				break;
			}
			return UIUtils.LoadSpriteFromPath(string.Format("icn_build_mask_cat_{0}", text));
		}
	}
}
