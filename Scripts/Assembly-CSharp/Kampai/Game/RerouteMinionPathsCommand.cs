using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Game.View;
using Kampai.Util;
using UnityEngine;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class RerouteMinionPathsCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("RerouteMinionPathsCommand") as IKampaiLogger;

		private Queue<Point> points = new Queue<Point>();

		[Inject]
		public Tuple<Point, Point> box { get; set; }

		[Inject(GameElement.MINION_MANAGER)]
		public GameObject minionManager { get; set; }

		[Inject]
		public IPlayerService playerService { get; set; }

		[Inject]
		public Environment environment { get; set; }

		[Inject]
		public MinionMoveToSignal moveToSignal { get; set; }

		[Inject]
		public MinionRunToSignal runToSignal { get; set; }

		[Inject]
		public PathFinder pathFinder { get; set; }

		public override void Execute()
		{
			MinionManagerView component = minionManager.GetComponent<MinionManagerView>();
			Init(component);
			Point item = box.Item1;
			Point item2 = box.Item2;
			item.x--;
			item.y--;
			item2.x++;
			item2.y++;
			List<ActionableObject> list = new List<ActionableObject>();
			component.GetObjectsInArea(item, item2, list);
			points.Clear();
			environment.GetClosestWalkableGridSquares(item2.x + 1, item.y - 1, list.Count, points);
			bool type = false;
			foreach (MinionObject item3 in list)
			{
				MinionState state = playerService.GetByInstanceId<Minion>(item3.ID).State;
				if (state == MinionState.Tasking || state == MinionState.Leisure)
				{
					continue;
				}
				PathAction pathAction = item3.currentAction as PathAction;
				if (pathAction == null)
				{
					if (points.Count == 0)
					{
						break;
					}
					runToSignal.Dispatch(item3.ID, points.Dequeue().XZProjection, 5.5f, type);
					type = true;
				}
			}
		}

		private void Init(MinionManagerView mm)
		{
			List<Tuple<int, Vector3>> list = new List<Tuple<int, Vector3>>();
			mm.GetPathingObjects(list);
			foreach (Tuple<int, Vector3> item3 in list)
			{
				int item = item3.Item1;
				Vector3 item2 = item3.Item2;
				GameObject gameObject = mm.GetGameObject(item);
				if (gameObject == null)
				{
					logger.Fatal(FatalCode.CMD_REROUTE_MINION, "Null GameObject: ", item);
					break;
				}
				MinionObject component = gameObject.GetComponent<MinionObject>();
				if (component == null)
				{
					logger.Fatal(FatalCode.CMD_REROUTE_MINION, "Null MinionObject: ", item);
					break;
				}
				IList<Vector3> list2 = pathFinder.FindPath(gameObject.transform.position, item2, 4);
				if (list2 == null || list2.Count == 1)
				{
					component.ReplaceActionsOfType(new AppearAction(component, item2, logger));
				}
				else if (component.currentAction is ConstantSpeedPathAction)
				{
					ConstantSpeedPathAction constantSpeedPathAction = component.currentAction as ConstantSpeedPathAction;
					float speed = constantSpeedPathAction.Speed;
					component.ReplaceActionsOfType(new ConstantSpeedPathAction(component, list2, speed, logger));
				}
				else if (component.currentAction is PathAction)
				{
					PathAction pathAction = component.currentAction as PathAction;
					float time = pathAction.RemainingTime();
					component.ReplaceActionsOfType(new PathAction(component, list2, time, logger));
				}
				else
				{
					moveToSignal.Dispatch(item, item2, false);
				}
			}
		}
	}
}
