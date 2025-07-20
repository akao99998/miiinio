using Kampai.Game;
using Kampai.Main;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class CompositeBuildingMenuView : PopupMenuView
	{
		public Text TitleLabel;

		public Text DescriptionLabel;

		public Text ShuffleButtonLabel;

		public Text MignettesButtonLabel;

		public ButtonView ShuffleButton;

		public ButtonView MignettesButton;

		public void Init(CompositeBuilding building, ILocalizationService localizationService, BuildingPopupPositionData buildingPopupPositionData)
		{
			InitProgrammatic(buildingPopupPositionData);
			TitleLabel.text = localizationService.GetString(building.Definition.LocalizedKey);
			ShuffleButtonLabel.text = localizationService.GetString("CompositeMenu_Shuffle");
			MignettesButtonLabel.text = localizationService.GetString("CompositeMenu_Mignettes");
			DescriptionLabel.text = localizationService.GetString("CompositeMenu_PiecesOwned", building.AttachedCompositePieceIDs.Count, building.Definition.MaxPieces);
			if (building.AttachedCompositePieceIDs.Count < 2)
			{
				ShuffleButton.GetComponent<Button>().interactable = false;
				ShuffleButton.GetComponent<KampaiImage>().color = Color.gray;
			}
			MignettesButton.PlaySoundOnClick = false;
			base.Open();
		}
	}
}
