using Elevation.Logging;
using Kampai.Common;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class SelectMinionCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("SelectMinionCommand") as IKampaiLogger;

		private bool selected;

		[Inject]
		public int id { get; set; }

		[Inject]
		public bool mute { get; set; }

		[Inject]
		public Boxed<Vector3> runLocation { get; set; }

		[Inject]
		public PickControllerModel model { get; set; }

		[Inject]
		public AnimateSelectedMinionSignal animateSelectedMinionSignal { get; set; }

		[Inject]
		public MinionStateChangeSignal stateChangeSignal { get; set; }

		[Inject]
		public IPlayerService player { get; set; }

		public override void Execute()
		{
			Minion byInstanceId = player.GetByInstanceId<Minion>(id);
			if (byInstanceId == null)
			{
				logger.Error("SelectMinionCommand: Cannot find minion with id of {0}.", id);
				return;
			}
			if (byInstanceId.State == MinionState.Idle || byInstanceId.State == MinionState.Selectable || byInstanceId.State == MinionState.WaitingOnMagnetFinger || byInstanceId.State == MinionState.Selected || byInstanceId.State == MinionState.Uninitialized)
			{
				HandleMinionSelected(byInstanceId);
			}
			if (!selected)
			{
				Vector3 value = runLocation.Value;
				model.Points.Enqueue(new Point(value.x, value.z));
			}
		}

		private void HandleMinionSelected(Minion minionModel)
		{
			SelectMinionState selectMinionState = new SelectMinionState();
			selectMinionState.minionID = id;
			selectMinionState.runLocation = runLocation;
			selectMinionState.muteStatus = mute;
			selectMinionState.triggerIncidentalAnimation = model.CurrentMode != PickControllerModel.Mode.MagnetFinger;
			animateSelectedMinionSignal.Dispatch(selectMinionState);
			if (minionModel.State != MinionState.Selected)
			{
				SelectedMinionModel selectedMinionModel = new SelectedMinionModel();
				selectedMinionModel.ID = id;
				selectedMinionModel.FinishedRouting = false;
				selectedMinionModel.RunLocation = runLocation.Value;
				model.SelectedMinions.Add(id, selectedMinionModel);
				model.HasPlayedSFX = false;
				stateChangeSignal.Dispatch(id, MinionState.Selected);
				selected = true;
			}
		}
	}
}
