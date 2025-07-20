using System.Collections;
using System.Collections.Generic;
using Kampai.Game;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.injector.api;

namespace Kampai.UI.View
{
	public class MignetteCallMinionsMediator : UIStackMediator<MignetteCallMinionsView>
	{
		private MignetteBuilding mignetteBuilding;

		private bool isAutoPanning;

		private bool callMinionsOnPanComplete;

		private int currentIndex;

		private int mignetteBuildingCount;

		private int rushCost;

		private bool ownsMinigamePack;

		private IEnumerator updateTimeCoroutine;

		private CameraAutoPanCompleteSignal cameraAutoPanCompleteSignal;

		private List<MignetteBuilding> validMignetteBuildingList = new List<MignetteBuilding>();

		[Inject]
		public HideSkrimSignal hideSkrim { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public ILocalizationService localService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject(GameElement.CONTEXT)]
		public ICrossContextCapable gameContext { get; set; }

		[Inject]
		public BuildingChangeStateSignal buildingStateSignal { get; set; }

		[Inject]
		public BuildingCooldownCompleteSignal onCooldownCompleteSignal { get; set; }

		[Inject]
		public ITimeEventService timeEventService { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playSFXSignal { get; set; }

		[Inject]
		public ShowNeedXMinionsSignal ShowNeedXMinionsSignal { get; set; }

		[Inject]
		public MignetteCallMinionsSignal mignetteCallMinionsSignal { get; set; }

		[Inject]
		public MignetteCallMinionsModel model { get; set; }

		public override void OnRegister()
		{
			isAutoPanning = false;
			callMinionsOnPanComplete = false;
			base.OnRegister();
			base.view.OnMenuClose.AddListener(OnMenuClose);
			base.view.callMinionsButton.ClickedSignal.AddListener(OnCallMinions);
			base.view.modal.rushButton.ClickedSignal.AddListener(Rush);
			onCooldownCompleteSignal.AddListener(OnBuildingCooldownComplete);
			base.view.leftArrow.ClickedSignal.AddListener(MoveToPreviousBuilding);
			base.view.rightArrow.ClickedSignal.AddListener(MoveToNextBuilding);
			cameraAutoPanCompleteSignal = gameContext.injectionBinder.GetInstance<CameraAutoPanCompleteSignal>();
			cameraAutoPanCompleteSignal.AddListener(PanComplete);
			ownsMinigamePack = playerService.HasPurchasedMinigamePack();
			if (!ownsMinigamePack)
			{
				gameContext.injectionBinder.GetInstance<IdleMinionSignal>().AddListener(base.view.UpdateView);
			}
		}

		public override void OnRemove()
		{
			base.OnRemove();
			StopCoroutine();
			Go.killAllTweensWithTarget(base.view.callMinionsButton.transform);
			base.view.OnMenuClose.RemoveListener(OnMenuClose);
			base.view.callMinionsButton.ClickedSignal.RemoveListener(OnCallMinions);
			base.view.modal.rushButton.ClickedSignal.RemoveListener(Rush);
			onCooldownCompleteSignal.RemoveListener(OnBuildingCooldownComplete);
			base.view.leftArrow.ClickedSignal.RemoveListener(MoveToPreviousBuilding);
			base.view.rightArrow.ClickedSignal.RemoveListener(MoveToNextBuilding);
			cameraAutoPanCompleteSignal.RemoveListener(PanComplete);
			if (!ownsMinigamePack)
			{
				gameContext.injectionBinder.GetInstance<IdleMinionSignal>().RemoveListener(base.view.UpdateView);
			}
		}

		public override void Initialize(GUIArguments args)
		{
			MignetteBuilding mignetteBuilding = args.Get<MignetteBuilding>();
			if (mignetteBuilding != null)
			{
				this.mignetteBuilding = mignetteBuilding;
				BuildingPopupPositionData buildingPopupPositionData = args.Get<BuildingPopupPositionData>();
				base.view.Init(mignetteBuilding, localService, playerService, gameContext.injectionBinder.GetInstance<GameObject>(GameElement.MINION_MANAGER), buildingPopupPositionData);
				InitMignetteBuildingList(this.mignetteBuilding);
				LoadMenu();
			}
		}

		private void LoadMenu()
		{
			if (mignetteBuilding.State == BuildingState.Cooldown)
			{
				base.view.StartTime(mignetteBuilding.StateStartTime, mignetteBuilding.StateStartTime + mignetteBuilding.GetCooldown());
				StopCoroutine();
				StartCoroutine();
			}
			else
			{
				StopCoroutine();
			}
		}

		private void StopCoroutine()
		{
			if (updateTimeCoroutine != null)
			{
				StopCoroutine(updateTimeCoroutine);
				updateTimeCoroutine = null;
			}
		}

		private void StartCoroutine()
		{
			updateTimeCoroutine = UpdateTime();
			StartCoroutine(updateTimeCoroutine);
		}

		private IEnumerator UpdateTime()
		{
			while (true)
			{
				int timeRemaining = timeEventService.GetTimeRemaining(mignetteBuilding.ID);
				base.view.UpdateTime(timeRemaining);
				rushCost = timeEventService.CalculateRushCostForTimer(timeRemaining, RushActionType.COOLDOWN);
				base.view.SetRushCost(rushCost);
				yield return new WaitForSeconds(1f);
			}
		}

		private void Rush()
		{
			if (base.view.modal.rushButton.isDoubleConfirmed())
			{
				playerService.ProcessRush(rushCost, true, RushTransactionCallback, mignetteBuilding.ID);
			}
		}

		private void RushTransactionCallback(PendingCurrencyTransaction pct)
		{
			if (pct.Success)
			{
				timeEventService.RushEvent(mignetteBuilding.ID);
				playSFXSignal.Dispatch("Play_button_premium_01");
			}
		}

		private void OnBuildingCooldownComplete(int instanceId)
		{
			if (mignetteBuilding != null && instanceId == mignetteBuilding.ID)
			{
				buildingStateSignal.Dispatch(instanceId, BuildingState.Idle);
				base.view.RecreateModal(mignetteBuilding);
				LoadMenu();
			}
		}

		private void MoveToPreviousBuilding()
		{
			if (currentIndex <= 0)
			{
				currentIndex = mignetteBuildingCount - 1;
			}
			else
			{
				currentIndex--;
			}
			mignetteBuilding = validMignetteBuildingList[currentIndex];
			ReloadMenu();
		}

		private void MoveToNextBuilding()
		{
			if (currentIndex >= mignetteBuildingCount - 1)
			{
				currentIndex = 0;
			}
			else
			{
				currentIndex++;
			}
			mignetteBuilding = validMignetteBuildingList[currentIndex];
			ReloadMenu();
		}

		private void ReloadMenu()
		{
			base.view.SetArrowButtonsState(false);
			base.view.RecreateModal(mignetteBuilding);
			PanAndShowBuildingMenu(mignetteBuilding);
			LoadMenu();
		}

		private void PanComplete()
		{
			isAutoPanning = false;
			base.view.SetArrowButtonsState(true);
			if (callMinionsOnPanComplete)
			{
				callMinionsOnPanComplete = false;
				OnCallMinions();
			}
		}

		private void PanAndShowBuildingMenu(Building building)
		{
			ICrossContextInjectionBinder injectionBinder = gameContext.injectionBinder;
			GameObject instance = injectionBinder.GetInstance<GameObject>(GameElement.BUILDING_MANAGER);
			BuildingManagerView component = instance.GetComponent<BuildingManagerView>();
			BuildingObject buildingObject = component.GetBuildingObject(building.ID);
			Vector3 position = buildingObject.transform.position;
			ScreenPosition screenPosition = building.Definition.ScreenPosition;
			isAutoPanning = true;
			injectionBinder.GetInstance<CameraAutoMoveSignal>().Dispatch(position, new Boxed<ScreenPosition>(screenPosition), new CameraMovementSettings(CameraMovementSettings.Settings.KeepUIOpen, building, null), false);
			injectionBinder.GetInstance<ShowHiddenBuildingsSignal>().Dispatch();
		}

		private void InitMignetteBuildingList(MignetteBuilding currentMignetteBuilding)
		{
			IList<MignetteBuilding> instancesByType = playerService.GetInstancesByType<MignetteBuilding>();
			for (int i = 0; i < instancesByType.Count; i++)
			{
				MignetteBuilding item = instancesByType[i];
				validMignetteBuildingList.Add(item);
			}
			currentIndex = validMignetteBuildingList.IndexOf(currentMignetteBuilding);
			mignetteBuildingCount = validMignetteBuildingList.Count;
			if (mignetteBuildingCount <= 1)
			{
				base.view.SetArrowButtonsVisibleAndActive(false);
			}
		}

		private void OnCallMinions()
		{
			if (!ownsMinigamePack && !OpenBuildingMenuCommand.HasEnoughFreeMinionsToAssignToBuilding(playerService, mignetteBuilding))
			{
				ShowNeedXMinionsSignal.Dispatch(mignetteBuilding.GetMinionSlotsOwned());
				return;
			}
			if (isAutoPanning)
			{
				callMinionsOnPanComplete = true;
				return;
			}
			model.NumberOfMinionsToCall = mignetteBuilding.MinionSlotsOwned;
			model.Building = mignetteBuilding;
			model.SignalSender = base.view.gameObject;
			mignetteCallMinionsSignal.Dispatch();
			base.view.Close();
		}

		protected override void Close()
		{
			playSFXSignal.Dispatch("Play_menu_disappear_01");
			base.view.Close();
		}

		private void OnMenuClose()
		{
			hideSkrim.Dispatch("MignetteSkrim");
			string prefab = (ownsMinigamePack ? "MignetteCallMinionsMenu" : "MignetteCallMinionsRequiredMenu");
			guiService.Execute(GUIOperation.Unload, prefab);
		}
	}
}
