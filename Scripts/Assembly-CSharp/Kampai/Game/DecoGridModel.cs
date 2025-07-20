using System;
using System.Collections.Generic;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Game
{
	public class DecoGridModel
	{
		private const int POINT_TAKEN = 100;

		private Dictionary<Point, int> decoGrid = new Dictionary<Point, int>();

		private readonly Point North = new Point(0, 1);

		private readonly Point East = new Point(1, 0);

		private readonly Point South = new Point(0, -1);

		private readonly Point West = new Point(-1, 0);

		private Point lastPlacedDeco = new Point(-1, -1);

		public bool AddDeco(int x, int y, int id)
		{
			Point p = new Point(x, y);
			return AddDeco(p, id);
		}

		public bool AddDeco(Point p, int id)
		{
			if (!decoGrid.ContainsKey(p))
			{
				decoGrid[p] = id;
				return true;
			}
			return false;
		}

		public void RemoveDeco(int x, int y)
		{
			Point key = new Point(x, y);
			if (decoGrid.ContainsKey(key))
			{
				decoGrid.Remove(key);
			}
		}

		public Vector3 GetNewPieceLocation(int x, int y, int id, Environment environment)
		{
			Point point = new Point(x, y);
			int neighbors = GetNeighbors(point, id, false, environment);
			Point point2 = new Point(0, 0);
			if (lastPlacedDeco.x > -1)
			{
				point2 = new Point(lastPlacedDeco.x - point.x, lastPlacedDeco.y - point.y);
			}
			Vector3 right = Vector3.right;
			Vector3 result;
			switch (neighbors)
			{
			case 15:
				result = right;
				break;
			case 0:
			case 8:
			case 13:
				result = right;
				break;
			case 3:
			case 6:
			case 9:
			case 12:
				if (point2 == North)
				{
					result = Vector3.back;
					break;
				}
				if (point2 == South)
				{
					result = Vector3.forward;
					break;
				}
				if (point2 == East)
				{
					result = Vector3.left;
					break;
				}
				if (point2 == West)
				{
					result = right;
					break;
				}
				switch (neighbors)
				{
				case 9:
				case 12:
					result = right;
					break;
				case 6:
					result = Vector3.forward;
					break;
				default:
					result = Vector3.left;
					break;
				}
				break;
			case 10:
				result = ((!(point2 == West)) ? Vector3.forward : Vector3.back);
				break;
			case 5:
				result = ((!(point2 == South)) ? Vector3.left : right);
				break;
			case 4:
			case 14:
				result = Vector3.forward;
				break;
			case 2:
			case 7:
				result = Vector3.left;
				break;
			case 1:
			case 11:
				result = Vector3.back;
				break;
			default:
				result = right;
				break;
			}
			lastPlacedDeco = point;
			return result;
		}

		private int GetNeighbors(Point point, int id, bool checkIdentical, Environment environment = null)
		{
			int num = 0;
			foreach (int value in Enum.GetValues(typeof(AdjacentDirection)))
			{
				int neighboringPieceId = GetNeighboringPieceId(point, (AdjacentDirection)value, environment);
				if ((checkIdentical && neighboringPieceId == id) || (!checkIdentical && neighboringPieceId == 100))
				{
					num |= value;
				}
			}
			return num;
		}

		public ConnectableBuildingPieceType GetConnectablePieceType(int x, int y, int id, out int outDirection)
		{
			Point point = new Point(x, y);
			int neighbors = GetNeighbors(point, id, true);
			outDirection = 0;
			switch (neighbors)
			{
			case 15:
				return ConnectableBuildingPieceType.CROSS;
			case 7:
			case 11:
			case 13:
			case 14:
				outDirection = RotateTShape(neighbors);
				return ConnectableBuildingPieceType.TSHAPE;
			case 5:
			case 10:
				outDirection = RotateStraightPiece(neighbors);
				return ConnectableBuildingPieceType.STRAIGHT;
			case 3:
			case 6:
			case 9:
			case 12:
				outDirection = RotateCorner(neighbors);
				return ConnectableBuildingPieceType.CORNER;
			case 1:
			case 2:
			case 4:
			case 8:
				outDirection = RotateEndCap(neighbors);
				return ConnectableBuildingPieceType.ENDCAP;
			case 0:
				return ConnectableBuildingPieceType.POST;
			default:
				return ConnectableBuildingPieceType.POST;
			}
		}

		private int RotateTShape(int neighbors)
		{
			switch (neighbors)
			{
			case 14:
				return 0;
			case 13:
				return 90;
			case 11:
				return 180;
			case 7:
				return 270;
			default:
				return 0;
			}
		}

		private int RotateStraightPiece(int neighbors)
		{
			switch (neighbors)
			{
			case 10:
				return 0;
			case 5:
				return 90;
			default:
				return 0;
			}
		}

		private int RotateCorner(int neighbors)
		{
			switch (neighbors)
			{
			case 12:
				return 90;
			case 9:
				return 180;
			case 6:
				return 0;
			case 3:
				return 270;
			default:
				return 0;
			}
		}

		private int RotateEndCap(int neighbors)
		{
			switch (neighbors)
			{
			case 8:
				return 0;
			case 1:
				return 90;
			case 2:
				return 180;
			case 4:
				return 270;
			default:
				return 0;
			}
		}

		public int GetNeighboringPieceId(Point point, AdjacentDirection dir, Environment environment)
		{
			Point point2;
			switch (dir)
			{
			case AdjacentDirection.North:
				point2 = North;
				break;
			case AdjacentDirection.South:
				point2 = South;
				break;
			case AdjacentDirection.East:
				point2 = East;
				break;
			case AdjacentDirection.West:
				point2 = West;
				break;
			default:
				return 0;
			}
			int x = point.x + point2.x;
			int num = point.y + point2.y;
			if (environment != null)
			{
				if (!environment.IsUnlocked(x, num) || environment.IsOccupied(x, num))
				{
					return 100;
				}
				return 0;
			}
			point.x = x;
			point.y = num;
			if (!decoGrid.ContainsKey(point))
			{
				return 0;
			}
			return decoGrid[point];
		}
	}
}
