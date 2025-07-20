using System;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.UI;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game.View
{
	public class ExitVillainLairCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("ExitVillainLairCommand") as IKampaiLogger;

		[Inject]
		public Boxed<Action> callback { get; set; }

		[Inject]
		public IGUIService guiService { get; set; }

		[Inject]
		public VillainLairModel villainLairModel { get; set; }

		[Inject]
		public EnableVillainLairHudSignal enableVillainHudSignal { get; set; }

		[Inject]
		public ShowHUDSignal showHUDSignal { get; set; }

		[Inject]
		public CameraMoveToCustomPositionSignal customCameraPositionSignal { get; set; }

		[Inject]
		public FadeBlackSignal fadeBlackSignal { get; set; }

		[Inject]
		public ShowAllWayFindersSignal showAllWayFindersSignal { get; set; }

		[Inject]
		public CloseAllOtherMenuSignal closeUISignal { get; set; }

		[Inject]
		public DisplayVillainSignal displayVillainSignal { get; set; }

		[Inject]
		public CloseAllMessageDialogs closeAllDialogsSignal { get; set; }

		[Inject]
		public DisplayMasterPlanCooldownAlertSignal displayCooldownAlertSignal { get; set; }

		[Inject]
		public IGhostComponentService ghostService { get; set; }

		[Inject]
		public HideFluxWayfinder hideFluxWayfinder { get; set; }

		[Inject]
		public IMasterPlanService masterPlanService { get; set; }

		public override void Execute()
		{
			villainLairModel.leavingLair = true;
			closeAllDialogsSignal.Dispatch();
			IGUICommand command = guiService.BuildCommand(GUIOperation.LoadStatic, "FadeBlack");
			GameObject gameObject = guiService.Execute(command);
			if (gameObject == null)
			{
				logger.Warning("Trying to exit villain lair, but fade black prefab was not loaded");
			}
			closeUISignal.Dispatch(null);
			IList<Action> list = new List<Action>();
			list.Add(FadeInCallback);
			fadeBlackSignal.Dispatch(true, list);
		}

		private void FadeInCallback()
		{
			if (villainLairModel.currentActiveLair == null)
			{
				logger.Fatal(FatalCode.CMD_EXIT_VILLAIN_LAIR_NULL_REFERENCE);
			}
			villainLairModel.currentActiveLair = null;
			villainLairModel.leavingLair = false;
			enableVillainHudSignal.Dispatch(false);
			showAllWayFindersSignal.Dispatch();
			showHUDSignal.Dispatch(true);
			customCameraPositionSignal.Dispatch(60011, new Boxed<Action>(CameraCallback));
			hideFluxWayfinder.Dispatch(true);
			ghostService.ClearGhostComponentBuildings(true, true);
			MasterPlan currentMasterPlan = masterPlanService.CurrentMasterPlan;
			if (currentMasterPlan != null)
			{
				displayVillainSignal.Dispatch(currentMasterPlan.Definition.VillainCharacterDefID, false);
			}
		}

		private void CameraCallback()
		{
			IList<Action> list = new List<Action>();
			list.Add(callback.Value);
			list.Add(FadeOutCallback);
			fadeBlackSignal.Dispatch(false, list);
			MasterPlan currentMasterPlan = masterPlanService.CurrentMasterPlan;
			if (currentMasterPlan != null && currentMasterPlan.displayCooldownAlert)
			{
				displayCooldownAlertSignal.Dispatch(currentMasterPlan);
			}
		}

		private void FadeOutCallback()
		{
			foreach (GameObject value in villainLairModel.villainLairInstances.Values)
			{
				value.SetActive(false);
			}
		}
	}
}
