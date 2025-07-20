using Kampai.Game;
using Kampai.Main;

namespace Kampai.UI.View
{
	public class CompositeBuildingMenuMediator : UIStackMediator<CompositeBuildingMenuView>
	{
		private CompositeBuilding compositeBuilding;

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public HideSkrimSignal hideSkrim { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		[Inject]
		public ShuffleCompositeBuildingPiecesSignal shuffleCompositeBuildingPiecesSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			base.view.ShuffleButton.ClickedSignal.AddListener(OnShuffleClicked);
			base.view.MignettesButton.ClickedSignal.AddListener(OnMignettesClicked);
			base.view.OnMenuClose.AddListener(FinishClose);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			base.view.ShuffleButton.ClickedSignal.RemoveListener(OnShuffleClicked);
			base.view.MignettesButton.ClickedSignal.RemoveListener(OnMignettesClicked);
			base.view.OnMenuClose.RemoveListener(FinishClose);
		}

		public override void Initialize(GUIArguments args)
		{
			compositeBuilding = args.Get<CompositeBuilding>();
			BuildingPopupPositionData buildingPopupPositionData = args.Get<BuildingPopupPositionData>();
			buildingPopupPositionData.EndPosition = buildingPopupPositionData.StartPosition;
			base.view.Init(compositeBuilding, localizationService, buildingPopupPositionData);
		}

		protected override void Close()
		{
			base.view.Close();
		}

		private void FinishClose()
		{
			soundFXSignal.Dispatch("Play_menu_disappear_01");
			hideSkrim.Dispatch("BuildingSkrim");
			guiService.Execute(GUIOperation.Unload, PrefabName);
		}

		private void OnShuffleClicked()
		{
			shuffleCompositeBuildingPiecesSignal.Dispatch(compositeBuilding.ID);
		}

		private void OnMignettesClicked()
		{
			Close();
			IGUICommand iGUICommand = guiService.BuildCommand(GUIOperation.Load, "screen_TownHall");
			iGUICommand.skrimScreen = "TownHallSkrim";
			iGUICommand.darkSkrim = true;
			guiService.Execute(iGUICommand);
		}
	}
}
