using Kampai.Game;
using Kampai.Main;
using Kampai.Util;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class SettingsLearnSystemsCategoryItemView : KampaiView
	{
		public Text Title;

		public Image CheckMark;

		public ScrollableButtonView Button;

		public PlayerTrainingDefinition Definition;

		public void Init(PlayerTrainingDefinition definition, ILocalizationService localizationService, bool hasCheckMark)
		{
			Definition = definition;
			Title.text = localizationService.GetString(definition.trainingTitleLocalizedKey);
			CheckMark.enabled = hasCheckMark;
		}
	}
}
