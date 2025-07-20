using System;
using System.Collections.Generic;
using Kampai.Common;
using Kampai.Game;
using UnityEngine;

namespace Kampai.Util
{
	public class PathFinder : IPathFinder
	{
		private enum MoveAction
		{
			Invalid = 0,
			North = 1,
			NorthEast = 2,
			East = 3,
			SouthEast = 4,
			South = 5,
			SouthWest = 6,
			West = 7,
			NorthWest = 8,
			Init = 9
		}

		private Kampai.Game.Environment environment;

		private ScatterList<Point> partyPoints = new ScatterList<Point>(100);

		private ScatterList<Point> walkingPoints = new ScatterList<Point>(100);

		private List<Point> stageBuildingPoints = new List<Point>();

		private float[,] moveScore;

		private MoveAction[,] lastAction;

		private SimplePriorityQueue<Point> moveQueue;

		private int dimX;

		private int dimY;

		private bool allowWalkableUpdate;

		private IRandomService randomService;

		private MinionPartyDefinition minionPartyDefinition;

		private StageBuildingDefinition stageBuildingDefinition;

		public Point RandomPoint
		{
			get
			{
				return walkingPoints.Pick(randomService);
			}
		}

		public Point PartyPoint
		{
			get
			{
				return partyPoints.Pick(randomService);
			}
		}

		[Construct]
		public PathFinder(Kampai.Game.Environment env, IRandomService randomService, IDefinitionService definitionService)
		{
			this.randomService = randomService;
			minionPartyDefinition = definitionService.Get<MinionPartyDefinition>(80000);
			stageBuildingDefinition = definitionService.Get<StageBuildingDefinition>(3054);
			SetEnvironment(env);
		}

		public void SetEnvironment(Kampai.Game.Environment env)
		{
			environment = env;
			dimX = environment.PlayerGrid.GetLength(0);
			dimY = environment.PlayerGrid.GetLength(1);
			moveScore = new float[dimX, dimY];
			lastAction = new MoveAction[dimX, dimY];
			moveQueue = new SimplePriorityQueue<Point>();
			UpdateWalkableRegion();
		}

		public void AllowWalkableUpdates()
		{
			allowWalkableUpdate = true;
		}

		public void UpdateWalkableRegion()
		{
			if (!allowWalkableUpdate)
			{
				return;
			}
			walkingPoints.Clear();
			stageBuildingPoints.Clear();
			for (int i = 0; i < dimX; i++)
			{
				for (int j = 0; j < dimY; j++)
				{
					if (environment.IsWalkable(i, j))
					{
						Point point = new Point(i, j);
						walkingPoints.Add(point);
						if (minionPartyDefinition.Contains(point))
						{
							partyPoints.Add(point);
						}
						if (stageBuildingDefinition.Contains(point))
						{
							stageBuildingPoints.Add(point);
						}
					}
				}
			}
		}

		public Vector3 RandomPosition(bool party)
		{
			Vector3 xZProjection = ((!party) ? RandomPoint : PartyPoint).XZProjection;
			if (!party)
			{
				xZProjection.x -= randomService.NextFloat() - 0.5f;
				xZProjection.z -= randomService.NextFloat() - 0.5f;
			}
			return xZProjection;
		}

		public void ShuffleLists()
		{
			walkingPoints.ShuffleContents(randomService);
			partyPoints.ShuffleContents(randomService);
		}

		public Vector3 GetStageBuildingPosition(int index)
		{
			if (index > stageBuildingPoints.Count)
			{
				return RandomPoint.XZProjection;
			}
			Vector3 xZProjection = stageBuildingPoints[index].XZProjection;
			xZProjection.x += randomService.NextFloat(0f - stageBuildingDefinition.maxMinionOffsetX, stageBuildingDefinition.maxMinionOffsetX);
			xZProjection.z += randomService.NextFloat(0f - stageBuildingDefinition.maxMinionOffsetY, stageBuildingDefinition.maxMinionOffsetY);
			return xZProjection;
		}

		public int GetStageBuildingCapacity()
		{
			return stageBuildingPoints.Count;
		}

		public IList<Vector3> RandomPath(Vector3 startPos, bool party, int mask)
		{
			return FindPath(startPos, RandomPosition(party), mask);
		}

		public IList<Vector3> FindPath(Vector3 startPos, Vector3 goalPos, int modifier, bool forceDestination = false)
		{
			Point point = new Point(Mathf.RoundToInt(startPos.x), Mathf.RoundToInt(startPos.z));
			Point point2 = new Point(Mathf.RoundToInt(goalPos.x), Mathf.RoundToInt(goalPos.z));
			bool flag = false;
			if (!environment.CompareModifiers(point.x, point.y, modifier))
			{
				List<Point> list = Bresenham.Line(point, point2) as List<Point>;
				foreach (Point item in list)
				{
					if (environment.CompareModifiers(item.x, item.y, modifier))
					{
						point = item;
						break;
					}
				}
				flag = true;
			}
			bool flag2 = false;
			if (forceDestination && !environment.CompareModifiers(point2.x, point2.y, modifier))
			{
				List<Point> list2 = Bresenham.Line(point2, point) as List<Point>;
				foreach (Point item2 in list2)
				{
					if (environment.CompareModifiers(item2.x, item2.y, modifier))
					{
						point2 = item2;
						break;
					}
				}
				flag2 = true;
			}
			if (point == point2)
			{
				List<Vector3> list3;
				if (flag2)
				{
					list3 = new List<Vector3>();
					list3.Add(startPos);
					return list3;
				}
				list3 = new List<Vector3>();
				list3.Add(startPos);
				list3.Add(goalPos);
				return list3;
			}
			Reset();
			lastAction[point.x, point.y] = MoveAction.Init;
			moveScore[point.x, point.y] = 0f;
			moveQueue.Enqueue(point, Mathf.RoundToInt(Point.Distance(point, point2)));
			while (moveQueue.Count > 0)
			{
				Point point3 = moveQueue.Dequeue();
				float num = moveScore[point3.x, point3.y];
				if (point3 == point2)
				{
					List<Vector3> list4 = ReconstructPath(point2);
					if (flag)
					{
						list4.Insert(0, startPos);
					}
					else
					{
						list4[0] = startPos;
					}
					if (!flag2)
					{
						list4[list4.Count - 1] = goalPos;
					}
					return list4;
				}
				float nextScore = num + 1f;
				EnqueueMove(point3.x, point3.y + 1, nextScore, MoveAction.North, point2, modifier);
				EnqueueMove(point3.x + 1, point3.y, nextScore, MoveAction.East, point2, modifier);
				EnqueueMove(point3.x, point3.y - 1, nextScore, MoveAction.South, point2, modifier);
				EnqueueMove(point3.x - 1, point3.y, nextScore, MoveAction.West, point2, modifier);
				nextScore = num + 1.4f;
				EnqueueMove(point3.x + 1, point3.y + 1, nextScore, MoveAction.NorthEast, point2, modifier);
				EnqueueMove(point3.x - 1, point3.y + 1, nextScore, MoveAction.NorthWest, point2, modifier);
				EnqueueMove(point3.x + 1, point3.y - 1, nextScore, MoveAction.SouthEast, point2, modifier);
				EnqueueMove(point3.x - 1, point3.y - 1, nextScore, MoveAction.SouthWest, point2, modifier);
			}
			if (forceDestination)
			{
				List<Vector3> list3 = new List<Vector3>();
				list3.Add(goalPos);
				return list3;
			}
			return null;
		}

		private void Reset()
		{
			Array.Clear(lastAction, 0, dimX * dimY);
			Array.Clear(moveScore, 0, dimX * dimY);
			moveQueue.Clear();
		}

		private List<Vector3> ReconstructPath(Point goal)
		{
			List<Vector3> list = new List<Vector3>();
			list.Insert(0, goal.XZProjection);
			Point point = goal;
			for (MoveAction moveAction = lastAction[point.x, point.y]; moveAction != MoveAction.Init; moveAction = lastAction[point.x, point.y])
			{
				lastAction[point.x, point.y] = MoveAction.Invalid;
				switch (moveAction)
				{
				case MoveAction.North:
					point.y--;
					break;
				case MoveAction.NorthEast:
					point.x--;
					point.y--;
					break;
				case MoveAction.East:
					point.x--;
					break;
				case MoveAction.SouthEast:
					point.x--;
					point.y++;
					break;
				case MoveAction.South:
					point.y++;
					break;
				case MoveAction.SouthWest:
					point.x++;
					point.y++;
					break;
				case MoveAction.West:
					point.x++;
					break;
				case MoveAction.NorthWest:
					point.x++;
					point.y--;
					break;
				default:
					return null;
				}
				list.Insert(0, point.XZProjection);
			}
			return list;
		}

		private void EnqueueMove(int x, int y, float nextScore, MoveAction action, Point goal, int modifier)
		{
			if (x >= 0 && x < dimX && y >= 0 && y < dimY && environment.CompareModifiers(x, y, modifier) && lastAction[x, y] == MoveAction.Invalid)
			{
				lastAction[x, y] = action;
				moveScore[x, y] = nextScore;
				Point point = new Point(x, y);
				moveQueue.Enqueue(point, Mathf.RoundToInt(nextScore + Point.Distance(point, goal)));
			}
		}

		public bool IsOccupiable(Location location)
		{
			return !environment.IsOccupied(location);
		}
	}
}
