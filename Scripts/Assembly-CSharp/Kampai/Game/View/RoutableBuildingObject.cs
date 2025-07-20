using System.Collections.Generic;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.View
{
	public class RoutableBuildingObject : BuildingObject
	{
		protected Transform[] routes;

		private bool routeToSlot;

		protected int stations;

		internal override void Init(Building building, IKampaiLogger logger, IDictionary<string, RuntimeAnimatorController> controllers, IDefinitionService definitionService)
		{
			base.Init(building, logger, controllers, definitionService);
			BuildingDefinition definition = building.Definition;
			if (definition == null)
			{
				logger.Fatal(FatalCode.BV_ILLEGAL_ROUTABLE_DEFINITION, building.Definition.ID.ToString());
			}
			routeToSlot = definition.RouteToSlot;
			stations = definition.WorkStations;
			routes = new Transform[stations];
			bool isRepaired = building.IsBuildingRepaired();
			for (int i = 0; i < stations; i++)
			{
				GameObject route = GetRoute(i, isRepaired);
				if (route == null)
				{
					routes = null;
					break;
				}
				routes[i] = route.transform;
			}
		}

		public Transform GetRouteTransform(int routeIndex)
		{
			return routes[routeIndex];
		}

		internal Vector3 GetRoutePosition(int routeIndex, Building building, Vector3 startingPosition)
		{
			if (routeToSlot && routeIndex >= 0 && routeIndex < routes.Length)
			{
				return routes[routeIndex].position;
			}
			Point closestBuildingSidewalk = BuildingUtil.GetClosestBuildingSidewalk(building.Location, startingPosition, definitionService.GetBuildingFootprint(building.Definition.FootprintID));
			return new Vector3(closestBuildingSidewalk.x, 0f, closestBuildingSidewalk.y);
		}

		private GameObject GetRoute(int index, bool isRepaired)
		{
			GameObject gameObject = base.gameObject.FindChild("route" + index);
			if (gameObject == null && isRepaired)
			{
				logger.Fatal(FatalCode.BV_NO_ROUTE, ID, "BV_NO_ROUTE: Building ID: {0}, Route Index: {1}", ID, index);
			}
			return gameObject;
		}

		internal int GetNumberOfStations()
		{
			return routes.Length;
		}

		internal virtual Vector3 GetRouteRotation(int routeIndex)
		{
			if (routeIndex >= 0 && routeIndex < routes.Length)
			{
				return routes[routeIndex].rotation.eulerAngles;
			}
			return Vector3.zero;
		}

		public virtual void MoveToRoutingPosition(CharacterObject characterObject, int routingIndex)
		{
			if (routingIndex < 0 || routingIndex >= routes.Length)
			{
				logger.Error("MoveToRoutingPosition: routingIndex {0} out of range (routes.Length={1})", routingIndex, routes.Length);
				return;
			}
			Transform transform = routes[routingIndex].transform;
			Transform transform2 = characterObject.gameObject.transform;
			Vector3 vector = ((!(transform2.parent != null)) ? new Vector3(0f, 0f, 0f) : transform2.parent.transform.position);
			Vector3 vector2 = ((!(transform.parent != null)) ? new Vector3(0f, 0f, 0f) : transform.parent.transform.position);
			transform2.localPosition = vector2 + transform.localPosition - vector;
			transform2.rotation = transform.rotation;
		}
	}
}
