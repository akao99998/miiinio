using Kampai.Game;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class VillainLairPortalEntryMediator : UIStackMediator<VillainLairPortalEntryView>
	{
		private VillainLairEntranceBuilding thisPortal;

		[Inject]
		public EnterVillainLairSignal enterVillainIslandSignal { get; set; }

		[Inject]
		public HideSkrimSignal hideSignal { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public VillainLairModel villainLairModel { get; set; }

		[Inject]
		public VillainLairAssetsLoadedSignal villainLairAssetsLoadedSignal { get; set; }

		public override void OnRegister()
		{
			base.view.OnMenuClose.AddListener(OnMenuClose);
			base.view.enterLair.ClickedSignal.AddListener(EnterLair);
			villainLairAssetsLoadedSignal.AddListener(SetEnterButtonActive);
			base.OnRegister();
		}

		public override void OnRemove()
		{
			base.view.OnMenuClose.RemoveListener(OnMenuClose);
			base.view.enterLair.ClickedSignal.RemoveListener(EnterLair);
			villainLairAssetsLoadedSignal.RemoveListener(SetEnterButtonActive);
			base.OnRemove();
		}

		public override void Initialize(GUIArguments args)
		{
			thisPortal = args.Get<VillainLairEntranceBuilding>();
			BuildingPopupPositionData buildingPopupPositionData = args.Get<BuildingPopupPositionData>();
			base.view.InitProgrammatic(buildingPopupPositionData);
			SetEnterButtonActive(villainLairModel.areLairAssetsLoaded);
			base.view.Open();
		}

		private void SetEnterButtonActive(bool active)
		{
			base.view.enterLair.GetComponent<Button>().interactable = active;
		}

		private void EnterLair()
		{
			Close();
			enterVillainIslandSignal.Dispatch(thisPortal.VillainLairInstanceID, false);
		}

		protected override void Close()
		{
			base.view.Close();
		}

		private void OnMenuClose()
		{
			hideSignal.Dispatch("VillainLairPortalSkrim");
			guiService.Execute(GUIOperation.Unload, "screen_EnterLair");
		}
	}
}
