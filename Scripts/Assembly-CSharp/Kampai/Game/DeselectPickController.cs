using System.Collections;
using System.Collections.Generic;
using Kampai.Common;
using Kampai.Game.View;
using Kampai.Main;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class DeselectPickController : Command
	{
		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public PickControllerModel model { get; set; }

		[Inject]
		public Environment environment { get; set; }

		[Inject]
		public Vector3 inputPosition { get; set; }

		[Inject]
		public MinionMoveToSignal moveSignal { get; set; }

		[Inject]
		public PlayGlobalSoundFXSignal playFXSignal { get; set; }

		[Inject]
		public MinionStateChangeSignal stateChangeSignal { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		[Inject]
		public CameraUtils cameraUtils { get; set; }

		[Inject(GameElement.MINION_MANAGER)]
		public GameObject minionManager { get; set; }

		public override void Execute()
		{
			Vector3 xZProjection = cameraUtils.GroundPlaneRaycast(inputPosition);
			MinionManagerView component = minionManager.GetComponent<MinionManagerView>();
			Point p = default(Point);
			p.XZProjection = xZProjection;
			if (environment.Contains(p))
			{
				MoveMinion(p, component);
			}
		}

		private void MoveMinion(Point p, MinionManagerView view)
		{
			if (model.SelectedMinions.Count > 0)
			{
				if (model.Points == null)
				{
					model.Points = new Queue<Point>(playerService.GetMinionCount());
				}
				else
				{
					model.Points.Clear();
				}
				environment.GetClosestWalkableGridSquares(p.x, p.y, playerService.GetMinionCount(), model.Points);
				routineRunner.StopTimer("MinionSelectionComplete");
				SortAndMoveMinions(view, p);
				playFXSignal.Dispatch("Play_minion_confirm_select_01");
			}
		}

		private IEnumerator MoveMinion(int id, int x, int y, bool mute, float delay)
		{
			yield return new WaitForSeconds(delay);
			moveSignal.Dispatch(id, new Vector3(x, 0f, y), mute);
		}

		private void SortAndMoveMinions(MinionManagerView view, Point p)
		{
			List<Tuple<int, float>> list = new List<Tuple<int, float>>(model.SelectedMinions.Count);
			foreach (int key in model.SelectedMinions.Keys)
			{
				Vector3 objectPosition = view.GetObjectPosition(key);
				list.Add(new Tuple<int, float>(key, Point.DistanceSquared(p, Point.FromVector3(objectPosition))));
			}
			list.Sort((Tuple<int, float> a, Tuple<int, float> b) => a.Item2.CompareTo(b.Item2));
			bool mute = false;
			float num = 0f;
			bool flag = true;
			foreach (Tuple<int, float> item2 in list)
			{
				int item = item2.Item1;
				Point point = model.Points.Dequeue();
				if (flag)
				{
					playFXSignal.Dispatch("Play_minion_confirm_path_01");
					flag = false;
				}
				stateChangeSignal.Dispatch(item, MinionState.Selected);
				routineRunner.StartCoroutine(MoveMinion(item, point.x, point.y, mute, num));
				num += Random.Range(0.01f, 0.2f);
				mute = true;
			}
			playFXSignal.Dispatch("Play_minion_confirm_select_01");
		}
	}
}
