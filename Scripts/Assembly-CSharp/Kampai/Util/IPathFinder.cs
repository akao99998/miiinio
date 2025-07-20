using System.Collections.Generic;
using Kampai.Game;
using UnityEngine;

namespace Kampai.Util
{
	public interface IPathFinder
	{
		IList<Vector3> FindPath(Vector3 startPos, Vector3 goalPos, int modifier, bool forceDestination = false);

		bool IsOccupiable(Location location);
	}
}
