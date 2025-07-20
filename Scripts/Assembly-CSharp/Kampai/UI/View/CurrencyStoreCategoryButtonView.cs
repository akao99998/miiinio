using Kampai.Game;
using Kampai.Main;
using UnityEngine;
using UnityEngine.UI;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class CurrencyStoreCategoryButtonView : ScrollableButtonView
	{
		public StoreBadgeView BadgeView;

		[Header("Selected Button")]
		public GameObject PanelForSelected;

		public Text SelectedCategoryTitle;

		public KampaiImage SelectedCategoryIcon;

		[Header("Deselected Button")]
		public Text CategoryTitle;

		public KampaiImage CategoryIcon;

		public new Signal<CurrencyStoreCategoryDefinition> ClickedSignal = new Signal<CurrencyStoreCategoryDefinition>();

		public CurrencyStoreCategoryDefinition categoryDefintiion { get; set; }

		public void Init(CurrencyStoreCategoryDefinition categoryDefinition, ILocalizationService localizationService)
		{
			categoryDefintiion = categoryDefinition;
			Text categoryTitle = CategoryTitle;
			string @string = localizationService.GetString(categoryDefinition.LocalizedKey);
			SelectedCategoryTitle.text = @string;
			categoryTitle.text = @string;
			KampaiImage categoryIcon = CategoryIcon;
			Sprite sprite = UIUtils.LoadSpriteFromPath(categoryDefinition.Image);
			SelectedCategoryIcon.sprite = sprite;
			categoryIcon.sprite = sprite;
			KampaiImage categoryIcon2 = CategoryIcon;
			sprite = UIUtils.LoadSpriteFromPath(categoryDefinition.Mask);
			SelectedCategoryIcon.maskSprite = sprite;
			categoryIcon2.maskSprite = sprite;
			PanelForSelected.SetActive(false);
		}

		public void SetBadgeCount(int badgeCount)
		{
			BadgeView.SetNewUnlockCounter(badgeCount);
		}

		public void MarkAsViewed()
		{
			BadgeView.SetNewUnlockCounter(0);
		}

		public override void ButtonClicked()
		{
			base.playSFXSignal.Dispatch("Play_button_click_01");
			MarkAsViewed();
			ClickedSignal.Dispatch(categoryDefintiion);
			MarkAsSelected();
		}

		public void MarkAsDeselected()
		{
			PanelForSelected.SetActive(false);
		}

		public void MarkAsSelected()
		{
			PanelForSelected.SetActive(true);
		}
	}
}
