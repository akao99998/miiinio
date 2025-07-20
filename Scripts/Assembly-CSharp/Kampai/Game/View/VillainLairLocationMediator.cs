using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Main;
using Kampai.UI.View;
using Kampai.Util;
using strange.extensions.mediation.impl;

namespace Kampai.Game.View
{
	public class VillainLairLocationMediator : Mediator
	{
		private IKampaiLogger logger = LogManager.GetClassLogger("VillainLairLocationMediator") as IKampaiLogger;

		[Inject]
		public VillainLairLocationView view { get; set; }

		[Inject]
		public LairEnvironmentElementClickedSignal lairEnvironmentElementClickedSignal { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ILocalizationService localizationService { get; set; }

		[Inject]
		public IMasterPlanService masterPlanService { get; set; }

		[Inject]
		public PopupMessageWithComponentBuildingSignal popupMessageWithComponentBuildingSignal { get; set; }

		[Inject]
		public EnableAllVillainLairCollidersSignal enableAllVillainLairCollidersSignal { get; set; }

		[Inject]
		public EnableOneVillainLairColliderSignal enableOneVillainLairColliderSignal { get; set; }

		[Inject]
		public UpdateAllVillainLairCollidersSignal updateAllVillainLairCollidersSignal { get; set; }

		[Inject]
		public DisplayMasterPlanCooldownAlertSignal displayAlertUISignal { get; set; }

		[Inject]
		public ClickedVillainLairGhostedComponentBuildingSignal clickedGhostComponentSignal { get; set; }

		[Inject]
		public DisplayMasterPlanSignal displayMasterPlanSignal { get; set; }

		[Inject]
		public DisplayMasterPlanIntroDialogSignal displayMasterPlanIntroDialogSignal { get; set; }

		[Inject]
		public MasterPlanSelectComponentSignal selectComponentSignal { get; set; }

		[Inject]
		public VillainLairModel lairModel { get; set; }

		public override void OnRegister()
		{
			enableOneVillainLairColliderSignal.AddListener(PlatformHitboxAltered);
			enableAllVillainLairCollidersSignal.AddListener(view.EnableAllColliders);
			lairEnvironmentElementClickedSignal.AddListener(PlatformClicked);
			updateAllVillainLairCollidersSignal.AddListener(UpdateCollidersAndComponents);
			clickedGhostComponentSignal.AddListener(GhostComponentClicked);
		}

		public override void OnRemove()
		{
			enableOneVillainLairColliderSignal.RemoveListener(PlatformHitboxAltered);
			enableAllVillainLairCollidersSignal.RemoveListener(view.EnableAllColliders);
			lairEnvironmentElementClickedSignal.RemoveListener(PlatformClicked);
			updateAllVillainLairCollidersSignal.RemoveListener(UpdateCollidersAndComponents);
			clickedGhostComponentSignal.RemoveListener(GhostComponentClicked);
		}

		private void PlatformClicked(int instanceID)
		{
			MasterPlan currentMasterPlan = masterPlanService.CurrentMasterPlan;
			if (currentMasterPlan == null || masterPlanService.AllComponentsAreComplete(currentMasterPlan.Definition.ID) || lairModel.leavingLair || !lairModel.currentActiveLair.hasVisited)
			{
				return;
			}
			if (!currentMasterPlan.introHasBeenDisplayed)
			{
				displayMasterPlanIntroDialogSignal.Dispatch();
			}
			else if (currentMasterPlan.cooldownUTCStartTime > 0)
			{
				if (lairModel.seenCooldownAlert)
				{
					displayAlertUISignal.Dispatch(currentMasterPlan);
				}
			}
			else if (view != null && view.colliderInstanceKeysToComponentIDs.ContainsKey(instanceID))
			{
				if (instanceID == view.masterPlanPlatformCollider.GetInstanceID())
				{
					MasterPlanComponentBuildingDefinition masterPlanComponentBuildingDefinition = definitionService.Get<MasterPlanComponentBuildingDefinition>(currentMasterPlan.Definition.BuildingDefID);
					string @string = localizationService.GetString(masterPlanComponentBuildingDefinition.LocalizedKey);
					popupMessageWithComponentBuildingSignal.Dispatch(@string, false, masterPlanComponentBuildingDefinition.ID);
				}
				else
				{
					int type = view.colliderInstanceKeysToComponentIDs[instanceID];
					displayMasterPlanSignal.Dispatch(type);
				}
			}
		}

		private void GhostComponentClicked(MasterPlanComponentBuildingObject obj)
		{
			if (!lairModel.currentActiveLair.hasVisited)
			{
				return;
			}
			MasterPlan currentMasterPlan = masterPlanService.CurrentMasterPlan;
			if (currentMasterPlan == null)
			{
				return;
			}
			List<int> componentDefinitionIDs = currentMasterPlan.Definition.ComponentDefinitionIDs;
			List<int> compBuildingDefinitionIDs = currentMasterPlan.Definition.CompBuildingDefinitionIDs;
			int definitionID = obj.DefinitionID;
			for (int i = 0; i < componentDefinitionIDs.Count; i++)
			{
				if (compBuildingDefinitionIDs[i] == definitionID)
				{
					MasterPlanComponent firstInstanceByDefinitionId = playerService.GetFirstInstanceByDefinitionId<MasterPlanComponent>(componentDefinitionIDs[i]);
					if (firstInstanceByDefinitionId != null && firstInstanceByDefinitionId.State > MasterPlanComponentState.NotStarted && firstInstanceByDefinitionId.State < MasterPlanComponentState.Scaffolding)
					{
						selectComponentSignal.Dispatch(currentMasterPlan.Definition, i, false);
						break;
					}
					displayMasterPlanSignal.Dispatch(componentDefinitionIDs[i]);
				}
			}
		}

		private void PlatformHitboxAltered(bool enable, int componentBuildingDefID)
		{
			MasterPlan currentMasterPlan = masterPlanService.CurrentMasterPlan;
			if (currentMasterPlan == null)
			{
				return;
			}
			for (int i = 0; i < currentMasterPlan.Definition.CompBuildingDefinitionIDs.Count; i++)
			{
				if (currentMasterPlan.Definition.CompBuildingDefinitionIDs[i] == componentBuildingDefID)
				{
					view.EnableCollider(currentMasterPlan.Definition.ComponentDefinitionIDs[i], enable);
					break;
				}
			}
		}

		private void UpdateCollidersAndComponents()
		{
			MasterPlan currentMasterPlan = masterPlanService.CurrentMasterPlan;
			view.SetUpInstanceIDs(currentMasterPlan.Definition, playerService, logger);
		}
	}
}
