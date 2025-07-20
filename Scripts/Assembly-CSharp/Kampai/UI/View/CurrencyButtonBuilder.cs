using System;
using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;

namespace Kampai.UI.View
{
	public static class CurrencyButtonBuilder
	{
		public static CurrencyButtonView Build(ILocalizationService localService, CurrencyItemDefinition definition, StoreItemDefinition storeItemDef, string inputStr, string outputStr, Transform i_parent, bool hasVFX)
		{
			if (definition == null)
			{
				throw new ArgumentNullException("definition", "CurrencyButtonBuilder: You are passing in null definitions!");
			}
			GameObject original = KampaiResources.Load("cmp_purchaseCurrencyButton") as GameObject;
			GameObject gameObject = UnityEngine.Object.Instantiate(original);
			CurrencyButtonView component = gameObject.GetComponent<CurrencyButtonView>();
			if (hasVFX)
			{
				GameObject original2 = KampaiResources.Load(definition.VFX) as GameObject;
				GameObject gameObject2 = UnityEngine.Object.Instantiate(original2);
				Vector3Serialize vFXOffset = definition.VFXOffset;
				if (vFXOffset != null)
				{
					gameObject2.transform.position = new Vector3(vFXOffset.x, vFXOffset.y, vFXOffset.z);
				}
				gameObject2.transform.SetParent(component.VFXRoot, false);
				component.VFXPrefab = gameObject2;
			}
			component.Description.text = localService.GetStringUpper(definition.LocalizedKey);
			component.isCOPPAGated = definition.COPPAGated;
			component.ItemWorth.text = outputStr;
			if (storeItemDef.Type == StoreItemType.GrindCurrency)
			{
				component.CostCurrencyIcon.gameObject.SetActive(false);
			}
			else
			{
				component.CostCurrencyIcon.gameObject.SetActive(false);
			}
			if (storeItemDef.Type == StoreItemType.SalePack)
			{
				component.isStarterPack = true;
			}
			if (storeItemDef.PercentOff > 0)
			{
				string @string = localService.GetString("PercentOff", storeItemDef.PercentOff);
				if (@string != null && @string.Trim().Length > 0)
				{
					component.ValueBanner.SetActive(true);
					component.ValueBannerText.text = @string;
					if (storeItemDef.IsFeatured)
					{
						component.ValueImage.color = Color.green;
					}
				}
			}
			Sprite sprite = UIUtils.LoadSpriteFromPath(definition.Image);
			component.ItemImage.sprite = sprite;
			Sprite maskSprite = UIUtils.LoadSpriteFromPath(definition.Mask);
			component.ItemImage.maskSprite = maskSprite;
			component.ItemPrice.text = inputStr;
			RectTransform rectTransform = gameObject.transform as RectTransform;
			rectTransform.SetParent(i_parent, false);
			return component;
		}
	}
}
