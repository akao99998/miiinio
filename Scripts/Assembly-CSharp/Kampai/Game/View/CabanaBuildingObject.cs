using System.Collections.Generic;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Game.View
{
	public class CabanaBuildingObject : RoutableBuildingObject
	{
		private Transform _routingPoint;

		private Vector3 _offset = new Vector3(1f, 2f, -6f);

		internal override void Init(Building building, IKampaiLogger logger, IDictionary<string, RuntimeAnimatorController> controllers, IDefinitionService definitionService)
		{
			base.Init(building, logger, controllers, definitionService);
			if (routes != null && routes.Length > 0)
			{
				_routingPoint = routes[0];
			}
			if (_routingPoint == null)
			{
				_routingPoint = base.transform;
			}
		}

		public Transform GetRoutingPoint()
		{
			return _routingPoint;
		}

		protected override Vector3 GetIndicatorPosition(bool centerY)
		{
			Vector3 position = base.transform.position;
			return position + _offset;
		}
	}
}
