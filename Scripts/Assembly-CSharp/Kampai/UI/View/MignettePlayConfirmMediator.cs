using Kampai.Common;
using Kampai.Game;
using Kampai.Game.View;
using UnityEngine;
using strange.extensions.context.api;

namespace Kampai.UI.View
{
	public class MignettePlayConfirmMediator : UIStackMediator<MignettePlayConfirmView>
	{
		private MignetteBuilding mignetteBuilding;

		private MignetteBuildingObject mignetteBuildingObject;

		[Inject]
		public CloseAllOtherMenuSignal closeSignal { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public EjectAllMinionsFromBuildingSignal ejectAllMinionsFromBuildingSignal { get; set; }

		[Inject]
		public StartMignetteSignal startMignetteSignal { get; set; }

		[Inject]
		public PickControllerModel pickControllerModel { get; set; }

		[Inject]
		public ShowHUDSignal showHUDSignal { get; set; }

		[Inject]
		public ShowStoreSignal showStoreSignal { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		public override void OnRegister()
		{
			base.OnRegister();
			closeSignal.Dispatch(base.view.gameObject);
			base.view.CloseButton.ClickedSignal.AddListener(OnCloseButtonClicked);
			base.view.PlayButton.ClickedSignal.AddListener(OnPlayButtonClicked);
			base.view.gameObject.SetActive(false);
			SetMainHUDVisible(false);
		}

		public override void Initialize(GUIArguments args)
		{
			MignetteBuilding building = args.Get<MignetteBuilding>();
			Init(building);
		}

		private void SetMainHUDVisible(bool visible)
		{
			pickControllerModel.ForceDisabled = !visible;
			showHUDSignal.Dispatch(visible);
			showStoreSignal.Dispatch(visible);
		}

		public override void OnRemove()
		{
			base.OnRemove();
			base.view.CloseButton.ClickedSignal.RemoveListener(OnCloseButtonClicked);
			base.view.PlayButton.ClickedSignal.RemoveListener(OnPlayButtonClicked);
		}

		protected override void Update()
		{
			if (mignetteBuildingObject != null)
			{
				if (mignetteBuildingObject.GetMignetteMinionCount() == mignetteBuilding.GetMinionSlotsOwned())
				{
					base.view.PlayButton.gameObject.SetActive(true);
				}
				else
				{
					base.view.PlayButton.gameObject.SetActive(false);
				}
			}
		}

		private void Init(MignetteBuilding building)
		{
			if (building != null)
			{
				mignetteBuilding = building;
				base.view.gameObject.SetActive(true);
				GameObject instance = gameContext.injectionBinder.GetInstance<GameObject>(GameElement.BUILDING_MANAGER);
				BuildingManagerMediator component = instance.GetComponent<BuildingManagerMediator>();
				mignetteBuildingObject = component.view.GetBuildingObject(mignetteBuilding.ID) as MignetteBuildingObject;
			}
		}

		private void CancelMignette()
		{
			ejectAllMinionsFromBuildingSignal.Dispatch(mignetteBuilding.ID);
			SetMainHUDVisible(true);
			HideConfirmUI();
		}

		private void HideConfirmUI()
		{
			guiService.Execute(GUIOperation.Unload, "MignettePlayConfirmMenu");
		}

		protected override void Close()
		{
			CancelMignette();
		}

		private void OnCloseButtonClicked()
		{
			CancelMignette();
		}

		private void OnPlayButtonClicked()
		{
			if (mignetteBuildingObject.GetMignetteMinionCount() == mignetteBuilding.GetMinionSlotsOwned())
			{
				startMignetteSignal.Dispatch(mignetteBuilding.ID);
				HideConfirmUI();
			}
		}
	}
}
