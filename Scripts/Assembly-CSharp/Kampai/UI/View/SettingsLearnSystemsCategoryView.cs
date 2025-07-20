using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class SettingsLearnSystemsCategoryView : KampaiView
	{
		public Text Title;

		public KampaiImage toggleImage;

		public ScrollableButtonView Button;

		public PlayerTrainingCategoryDefinition Definition;

		public void Init(PlayerTrainingCategoryDefinition definition, ILocalizationService localizationService, bool selected)
		{
			Definition = definition;
			Title.text = localizationService.GetString(definition.categoryTitleLocalizedKey);
			if (selected)
			{
				toggleImage.gameObject.SetActive(true);
			}
			else
			{
				toggleImage.gameObject.SetActive(false);
			}
		}
	}
}
