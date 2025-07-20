using System;
using System.Collections;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.UI;
using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.command.impl;

namespace Kampai.Game.View
{
	public class EnterVillainLairCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("EnterVillainLairCommand") as IKampaiLogger;

		private bool isFaded;

		private VillainLair currentLair;

		private MasterPlan masterPlan;

		[Inject]
		public int villainLairInstanceID { get; set; }

		[Inject]
		public bool shouldDisplayMasterPlanUI { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public IPrestigeService prestigeService { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public ITimeService timeService { get; set; }

		[Inject]
		public IMasterPlanService masterPlanService { get; set; }

		[Inject]
		public VillainLairModel villainLairModel { get; set; }

		[Inject]
		public CameraMoveToCustomPositionSignal customCameraPositionSignal { get; set; }

		[Inject]
		public EnableVillainLairHudSignal enableVillainHudSignal { get; set; }

		[Inject]
		public FadeBlackSignal fadeBlackSignal { get; set; }

		[Inject]
		public HideAllWayFindersSignal hideAllWayFindersSignal { get; set; }

		[Inject]
		public DisplayVolcanoLairVillainWayfinderSignal displayVolcanoWayfinderSignal { get; set; }

		[Inject]
		public DisplayMasterPlanSignal displayMasterPlanSignal { get; set; }

		[Inject]
		public HideFluxWayfinder hideFluxWayfinderSignal { get; set; }

		[Inject]
		public CreateNamedCharacterViewSignal createNamedCharacterViewSignal { get; set; }

		[Inject]
		public InitializeVillainSignal initVillainSignal { get; set; }

		[Inject]
		public LoadVillainLairInstancesSignal loadVillainLairInstancesSignal { get; set; }

		[Inject]
		public DisplayVillainSignal displayVillainSignal { get; set; }

		[Inject]
		public DisplayMasterPlanCooldownRewardDialogSignal displayCooldownRewardSignal { get; set; }

		[Inject]
		public CloseAllMessageDialogs closeAllDialogsSignal { get; set; }

		[Inject]
		public GetWayFinderSignal getWayFinderSignal { get; set; }

		[Inject]
		public StopEntranceWayfinderPulseSignal stopPulseSignal { get; set; }

		[Inject]
		public DisplayMasterPlanCooldownAlertSignal displayAlertUISignal { get; set; }

		[Inject]
		public ShowHUDSignal showHUDSignal { get; set; }

		[Inject]
		public SetBuildMenuEnabledSignal setBuildMenuEnabledSignal { get; set; }

		[Inject]
		public DisplayMasterPlanOnboardingSignal displayOnboardingSignal { get; set; }

		[Inject]
		public IGhostComponentService ghostComponentService { get; set; }

		[Inject]
		public EnableOneVillainLairColliderSignal enableOneVillainLairColliderSignal { get; set; }

		[Inject]
		public GenerateNewMasterPlanSignal generateNewMasterPlanSignal { get; set; }

		[Inject]
		public DisplayMasterPlanIntroDialogSignal displayMasterPlanIntroDialogSignal { get; set; }

		public override void Execute()
		{
			closeAllDialogsSignal.Dispatch();
			currentLair = playerService.GetByInstanceId<VillainLair>(villainLairInstanceID);
			if (currentLair == null)
			{
				logger.Error("Trying to enter a  villain lair that doesn't exist");
				return;
			}
			if (!villainLairModel.areLairAssetsLoaded)
			{
				logger.Fatal(FatalCode.CMD_INCOMPLETE_VILLAIN_LAIR_ASSETS_SIMPLE_UI, "Assets for Villain Lair {0} are not loaded", currentLair.ID);
			}
			villainLairModel.goingToLair = true;
			showHUDSignal.Dispatch(false);
			masterPlan = masterPlanService.CurrentMasterPlan;
			if (masterPlan == null || masterPlan.completionCount > 0)
			{
				generateNewMasterPlanSignal.Dispatch(new Boxed<Action>(CheckInstancesAndFadeOut));
			}
			else
			{
				CheckInstancesAndFadeOut();
			}
		}

		private void CheckInstancesAndFadeOut()
		{
			masterPlan = masterPlanService.CurrentMasterPlan;
			if (!villainLairModel.villainLairInstances.ContainsKey(currentLair.ID))
			{
				loadVillainLairInstancesSignal.Dispatch(currentLair.ID, delegate
				{
				});
				LoadVillain();
			}
			SetPlatformStatesAndGhostComponents();
			IGUICommand command = guiService.BuildCommand(GUIOperation.LoadStatic, "FadeBlack");
			guiService.Execute(command);
			routineRunner.StartCoroutine(FadeBlack());
		}

		private void LoadVillain()
		{
			if (masterPlan.cooldownUTCStartTime != 0)
			{
				return;
			}
			int villainCharacterDefID = masterPlan.Definition.VillainCharacterDefID;
			VillainDefinition villainDefinition = definitionService.Get<VillainDefinition>(villainCharacterDefID);
			Villain villain = playerService.GetFirstInstanceByDefinitionId<Villain>(villainDefinition.ID);
			if (villain == null)
			{
				villain = (Villain)villainDefinition.Build();
				playerService.Add(villain);
			}
			List<PrestigeDefinition> all = definitionService.GetAll<PrestigeDefinition>();
			foreach (PrestigeDefinition item in all)
			{
				if (item.TrackedDefinitionID == villainCharacterDefID)
				{
					Prestige prestige = playerService.GetFirstInstanceByDefinitionId<Prestige>(item.ID);
					if (prestige == null)
					{
						prestige = prestigeService.CreatePrestige(item.ID);
					}
					prestige.state = PrestigeState.Taskable;
					prestige.trackedInstanceId = villain.ID;
					prestige.UTCTimeUnlocked = timeService.CurrentTime();
				}
			}
			createNamedCharacterViewSignal.Dispatch(villain);
		}

		private void SetPlatformStatesAndGhostComponents()
		{
			if (masterPlan.cooldownUTCStartTime != 0)
			{
				return;
			}
			for (int i = 0; i < masterPlan.Definition.ComponentDefinitionIDs.Count; i++)
			{
				MasterPlanComponent firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<MasterPlanComponent>(masterPlan.Definition.ComponentDefinitionIDs[i]);
				if (firstInstanceByDefinitionId != null && firstInstanceByDefinitionId.State > MasterPlanComponentState.NotStarted)
				{
					if (firstInstanceByDefinitionId.State < MasterPlanComponentState.Scaffolding)
					{
						ghostComponentService.DisplayComponentMarkedAsInProgress(firstInstanceByDefinitionId);
					}
					enableOneVillainLairColliderSignal.Dispatch(false, firstInstanceByDefinitionId.buildingDefID);
				}
			}
		}

		private IEnumerator FadeBlack()
		{
			yield return null;
			IList<Action> actions = new List<Action> { (Action)FadedOutCallback };
			fadeBlackSignal.Dispatch(true, actions);
		}

		private void FadedOutCallback()
		{
			hideAllWayFindersSignal.Dispatch();
			setBuildMenuEnabledSignal.Dispatch(false);
			isFaded = true;
			villainLairModel.goingToLair = false;
			villainLairModel.currentActiveLair = currentLair;
			customCameraPositionSignal.Dispatch(currentLair.Definition.CustomCameraPositionDefinitionId, new Boxed<Action>(CameraCallback));
			MasterPlanDefinition definition = masterPlan.Definition;
			if (masterPlan.cooldownUTCStartTime == 0)
			{
				displayVolcanoWayfinderSignal.Dispatch();
				hideFluxWayfinderSignal.Dispatch(false);
				EnableBuildingsWayFinders();
				displayVillainSignal.Dispatch(definition.VillainCharacterDefID, true);
				initVillainSignal.Dispatch(currentLair, definition.VillainCharacterDefID);
			}
			else
			{
				displayAlertUISignal.Dispatch(masterPlan);
			}
			if (villainLairModel.villainLairInstances.ContainsKey(currentLair.ID))
			{
				villainLairModel.villainLairInstances[currentLair.ID].SetActive(true);
			}
		}

		private void FadedBackInCallback()
		{
			isFaded = false;
			if (shouldDisplayMasterPlanUI)
			{
				displayMasterPlanSignal.Dispatch(0);
			}
			if (!currentLair.hasVisited)
			{
				displayOnboardingSignal.Dispatch(65006);
			}
			else
			{
				enableVillainHudSignal.Dispatch(true);
				if (!masterPlan.introHasBeenDisplayed)
				{
					displayMasterPlanIntroDialogSignal.Dispatch();
				}
			}
			if (masterPlan.displayCooldownReward)
			{
				displayCooldownRewardSignal.Dispatch();
			}
			stopPulseSignal.Dispatch();
		}

		private void CameraCallback()
		{
			if (isFaded)
			{
				IList<Action> list = new List<Action>();
				list.Add(FadedBackInCallback);
				fadeBlackSignal.Dispatch(false, list);
			}
		}

		private void EnableBuildingsWayFinders()
		{
			List<MasterPlanComponentBuilding> instancesByType = playerService.GetInstancesByType<MasterPlanComponentBuilding>();
			foreach (MasterPlanComponentBuilding item in instancesByType)
			{
				getWayFinderSignal.Dispatch(item.ID, delegate(int wayFinderId, IWayFinderView wayFinderView)
				{
					if (wayFinderView != null)
					{
						wayFinderView.SetForceHide(false);
					}
				});
			}
		}
	}
}
