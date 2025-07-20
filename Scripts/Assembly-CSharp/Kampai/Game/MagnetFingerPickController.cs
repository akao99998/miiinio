using System.Collections;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Common;
using Kampai.Game.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;
using strange.extensions.signal.impl;

namespace Kampai.Game
{
	public class MagnetFingerPickController : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("MagnetFingerPickController") as IKampaiLogger;

		[Inject]
		public int pickEvent { get; set; }

		[Inject]
		public Vector3 inputPosition { get; set; }

		[Inject]
		public PickControllerModel model { get; set; }

		[Inject(GameElement.MINION_MANAGER)]
		public GameObject minionManager { get; set; }

		[Inject]
		public MinionStateChangeSignal stateChangeSignal { get; set; }

		[Inject]
		public CameraUtils cameraUtils { get; set; }

		[Inject]
		public SelectMinionSignal selectMinionSignal { get; set; }

		[Inject]
		public Environment environment { get; set; }

		[Inject]
		public IRandomService randomService { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public ILocalPersistanceService localPersistService { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public DisplayPlayerTrainingSignal displaySignal { get; set; }

		public override void Execute()
		{
			switch (pickEvent)
			{
			case 1:
				Initialize();
				break;
			case 2:
				model.CurrentMagnetFingerTimer += Time.deltaTime;
				model.DurationBetweenMinions -= 0.06f * Time.deltaTime;
				if (model.CurrentMagnetFingerTimer > model.DurationBetweenMinions)
				{
					SelectMinion();
				}
				break;
			case 3:
				if (model.ValidLocation)
				{
					MinionFreeze(false);
					Reset();
					int count = model.SelectedMinions.Count;
					LocalPersistMagnetFingerAction(count);
					routineRunner.StartCoroutine(DelayPlayerTraining());
				}
				break;
			}
		}

		private void Initialize()
		{
			if (model.MMView == null)
			{
				model.MMView = minionManager.GetComponent<MinionManagerView>();
			}
			Vector3 xZProjection = cameraUtils.GroundPlaneRaycast(inputPosition);
			Point p = default(Point);
			p.XZProjection = xZProjection;
			if (!environment.Contains(p) || !environment.IsWalkable(p.x, p.y))
			{
				model.ValidLocation = false;
				return;
			}
			model.ValidLocation = true;
			model.Points = environment.GetMagnetFingerGridSquares(p.x, p.y);
			SendSelectedSignals();
			model.Minions = model.MMView.GetMinionListSortedByDistanceAndState(inputPosition, false);
			MinionFreeze(true);
			SelectMinion();
		}

		private void SendSelectedSignals()
		{
			foreach (KeyValuePair<int, SelectedMinionModel> selectedMinion in model.SelectedMinions)
			{
				int key = selectedMinion.Key;
				Point point = model.Points.Dequeue();
				selectMinionSignal.Dispatch(key, new Boxed<Vector3>(new Vector3(point.x, 0f, point.y)), true);
			}
		}

		private void MinionFreeze(bool freeze)
		{
			foreach (int minion in model.Minions)
			{
				Minion byInstanceId = playerService.GetByInstanceId<Minion>(minion);
				if (freeze)
				{
					if (byInstanceId.State != MinionState.Leisure)
					{
						stateChangeSignal.Dispatch(minion, MinionState.WaitingOnMagnetFinger);
						MinionObject minionObject = model.MMView.Get(minion);
						minionObject.EnqueueAction(new RotateAction(minionObject, Camera.main.transform.eulerAngles.y - 180f, 360f, logger), true);
					}
				}
				else if (byInstanceId.State == MinionState.WaitingOnMagnetFinger)
				{
					stateChangeSignal.Dispatch(minion, MinionState.Idle);
				}
			}
		}

		private void SelectMinion()
		{
			if (model.Minions.Count != 0)
			{
				int param = model.Minions.Dequeue();
				Point point = model.Points.Dequeue();
				int num = 200 / (model.SelectedMinions.Count + 1);
				int num2 = randomService.NextInt(0, 100);
				bool param2 = ((num < num2) ? true : false);
				selectMinionSignal.Dispatch(param, new Boxed<Vector3>(new Vector3(point.x, 0f, point.y)), param2);
				model.CurrentMagnetFingerTimer = 0f;
			}
		}

		private void Reset()
		{
			model.CurrentMagnetFingerTimer = 0f;
			model.DurationBetweenMinions = 0.2f;
		}

		private void LocalPersistMagnetFingerAction(int selectedMinionCount)
		{
			if (selectedMinionCount > 0 && !localPersistService.HasKey("didyouknow_MagnetFinger"))
			{
				localPersistService.PutDataInt("didyouknow_MagnetFinger", 1);
			}
		}

		private IEnumerator DelayPlayerTraining()
		{
			yield return new WaitForSeconds(1f);
			displaySignal.Dispatch(19000015, false, new Signal<bool>());
		}
	}
}
