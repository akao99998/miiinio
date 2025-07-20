using System.Collections.Generic;
using Kampai.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Game
{
	public class Environment : Instance<EnvironmentDefinition>, IEnvironmentNavigation
	{
		[JsonIgnore]
		public EnvironmentGridSquare[,] PlayerGrid;

		public int GetLength(int dimension)
		{
			return PlayerGrid.GetLength(dimension);
		}

		public bool Contains(Point p)
		{
			return Contains(p.x, p.y);
		}

		public bool Contains(int x, int z)
		{
			return x >= 0 && x < PlayerGrid.GetLength(0) && z >= 0 && z < PlayerGrid.GetLength(1);
		}

		public bool IsOccupied(int x, int z)
		{
			return Contains(x, z) && PlayerGrid[x, z].Occupied;
		}

		public bool IsOccupied(Location location)
		{
			return IsOccupied(location.x, location.y);
		}

		public bool IsUnlocked(int x, int z)
		{
			return Contains(x, z) && PlayerGrid[x, z].Unlocked;
		}

		public bool IsUnlocked(Location location)
		{
			return IsUnlocked(location.x, location.y);
		}

		public bool IsWalkable(int x, int z)
		{
			return Contains(x, z) && PlayerGrid[x, z].Walkable;
		}

		public bool IsWalkable(Location location)
		{
			return IsWalkable(location.x, location.y);
		}

		public bool IsCharacterWalkable(int x, int z)
		{
			return Contains(x, z) && PlayerGrid[x, z].CharacterWalkable;
		}

		public bool IsCharacterWalkable(Location location)
		{
			return IsCharacterWalkable(location.x, location.y);
		}

		public bool CompareModifiers(Location location, int modifier)
		{
			return CompareModifiers(location.x, location.y, modifier);
		}

		public bool CompareModifiers(int x, int z, int modifier)
		{
			return Contains(x, z) && (PlayerGrid[x, z].Modifier & modifier) == modifier;
		}

		public Building GetBuilding(int x, int z)
		{
			if (!Contains(x, z))
			{
				return null;
			}
			return PlayerGrid[x, z].Instance as Building;
		}

		public void GetClosestWalkableGridSquares(int x, int y, int count, Queue<Point> points)
		{
			GetClosestGridSquare(x, y, count, points, 4);
		}

		public void GetClosestCharacterWalkableGridSquares(int x, int y, int count, Queue<Point> points)
		{
			GetClosestGridSquare(x, y, count, points, 8);
		}

		public void GetClosestGridSquare(int x, int y, int count, Queue<Point> points, int modifier)
		{
			int length = PlayerGrid.GetLength(0);
			int length2 = PlayerGrid.GetLength(1);
			int num = Mathf.Max(length, length2);
			for (int i = 0; i <= num; i++)
			{
				for (int j = -i; j <= i; j++)
				{
					for (int k = -i; k <= i; k++)
					{
						int num2 = x + j;
						int num3 = y + k;
						if (num2 < 0 || num2 >= length || num3 < 0 || num3 >= length2)
						{
							continue;
						}
						Point item = new Point(num2, num3);
						if (CompareModifiers(num2, num3, modifier) && !points.Contains(item))
						{
							points.Enqueue(item);
							if (points.Count >= count)
							{
								return;
							}
						}
					}
				}
			}
		}

		public Queue<Point> GetMagnetFingerGridSquares(int x, int y)
		{
			Point p = new Point(x, y + 1);
			if (!Contains(p))
			{
				return new Queue<Point>();
			}
			List<Tuple<Point, float>> list = new List<Tuple<Point, float>>();
			Point a = new Point(x, y);
			for (int i = 0; i < PlayerGrid.GetLength(0); i++)
			{
				for (int j = 0; j < PlayerGrid.GetLength(1); j++)
				{
					if ((i != x && i != x + 1 && i != x - 1) || (j != y && j != y + 1 && j != y - 1))
					{
						Vector2 position = PlayerGrid[i, j].Position;
						Point point = new Point(position.x, position.y);
						if (IsWalkable(point.x, point.y))
						{
							Point b = point;
							b.y--;
							b.x++;
							list.Add(Tuple.Create(point, Point.DistanceSquared(a, b)));
						}
					}
				}
			}
			return SortList(list);
		}

		public Queue<Point> GetMinionPartyGridSquares(int x, int y)
		{
			Point p = new Point(x, y + 1);
			if (!Contains(p))
			{
				return new Queue<Point>();
			}
			List<Tuple<Point, float>> list = new List<Tuple<Point, float>>();
			Point a = new Point(x, y);
			for (int i = 0; i < PlayerGrid.GetLength(0); i++)
			{
				for (int j = 0; j < PlayerGrid.GetLength(1); j++)
				{
					Vector2 position = PlayerGrid[i, j].Position;
					Point first = new Point(position.x, position.y);
					if (IsWalkable(first.x, first.y))
					{
						list.Add(Tuple.Create(first, Point.DistanceSquared(a, new Point(first.x + 1, first.y - 1))));
					}
				}
			}
			return SortList(list);
		}

		private Queue<Point> SortList(List<Tuple<Point, float>> gridTuples)
		{
			Queue<Point> queue = new Queue<Point>();
			gridTuples.Sort((Tuple<Point, float> square1, Tuple<Point, float> square2) => square1.Item2.CompareTo(square2.Item2));
			for (int i = 0; i < gridTuples.Count; i++)
			{
				queue.Enqueue(gridTuples[i].Item1);
			}
			return queue;
		}
	}
}
