using System.Collections.Generic;
using Kampai.Game;
using Kampai.Util;
using UnityEngine;

public static class BuildingUtil
{
	public static int GetFootprintWidth(string footprint)
	{
		int num = 0;
		foreach (char c in footprint)
		{
			if (c != '|')
			{
				num++;
				continue;
			}
			break;
		}
		return num;
	}

	public static int GetFootprintDepth(string footprint)
	{
		int num = 1;
		foreach (char c in footprint)
		{
			if (c == '|')
			{
				num++;
			}
		}
		return num;
	}

	public static int GetHarvestTimeForTaskableBuilding(TaskableBuilding building, IDefinitionService definitionService)
	{
		int result = 0;
		ResourceBuilding resourceBuilding = building as ResourceBuilding;
		if (resourceBuilding != null)
		{
			int itemId = resourceBuilding.Definition.ItemId;
			IngredientsItemDefinition ingredientsItemDefinition = definitionService.Get<IngredientsItemDefinition>(itemId);
			result = (int)ingredientsItemDefinition.TimeToHarvest;
		}
		return result;
	}

	public static Vector3 UIToWorldCoords(Camera camera, Vector2 uiPosition)
	{
		Vector3 inNormal = new Vector3(0f, 1f, 0f);
		Vector3 inPoint = new Vector3(0f, 0f, 0f);
		Plane plane = new Plane(inNormal, inPoint);
		Vector3 position = new Vector3(uiPosition.x, uiPosition.y, 0f);
		Ray ray = camera.ScreenPointToRay(position);
		float enter;
		plane.Raycast(ray, out enter);
		Vector3 point = ray.GetPoint(enter);
		return new Vector3(Mathf.RoundToInt(point.x), 0f, Mathf.RoundToInt(point.z));
	}

	public static Vector3 WorldToUICoords(Camera camera, Vector3 worldPosition)
	{
		return camera.WorldToScreenPoint(worldPosition);
	}

	public static List<Point> GetBuildingSideWalkList(Location buildingLocation, string footprint)
	{
		Point point = new Point(buildingLocation.x, buildingLocation.y);
		List<Point> list = new List<Point>();
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < footprint.Length; i++)
		{
			switch (footprint[i])
			{
			case '.':
			{
				Point item = new Point(point.x + num, point.y + num2);
				list.Add(item);
				break;
			}
			case '|':
				num = 0;
				num2--;
				break;
			}
			num++;
		}
		return list;
	}

	public static Point GetClosestBuildingSidewalk(Location buildingLocation, Vector3 position, string footprint)
	{
		Point point = new Point(buildingLocation.x, buildingLocation.y);
		Point a = default(Point);
		a.XZProjection = position;
		int num = 0;
		int num2 = 0;
		float num3 = float.MaxValue;
		Point result = default(Point);
		bool flag = false;
		for (int i = 0; i < footprint.Length; i++)
		{
			switch (footprint[i])
			{
			case '.':
			{
				Point point2 = new Point(point.x + num, point.y + num2);
				float num4 = Point.Distance(a, point2);
				if (num4 < num3)
				{
					num3 = num4;
					result = point2;
					flag = true;
				}
				break;
			}
			case '|':
				num = 0;
				num2--;
				break;
			case 'x':
				if (!flag)
				{
					Point point2 = new Point(point.x + num, point.y + num2);
					float num4 = Point.Distance(a, point2);
					if (num4 < num3)
					{
						num3 = num4;
						result = point2;
					}
				}
				break;
			}
			num++;
		}
		return result;
	}

	public static bool HasSidewalk(string footprint)
	{
		foreach (char c in footprint)
		{
			if (c == '.')
			{
				return true;
			}
		}
		return false;
	}
}
