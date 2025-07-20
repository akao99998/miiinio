using System.Collections.Generic;
using UnityEngine;

namespace Kampai.Game.View
{
	public class PathFollower : MonoBehaviour
	{
		public TextAsset Path;

		public Color SplineColor = Color.magenta;

		public float Duration = 1f;

		public bool Reverse;

		private GoSpline BuildSpline(Vector3 relativeOffset)
		{
			if (Path == null)
			{
				return null;
			}
			List<Vector3> list = GoSpline.bytesToVector3List(Path.bytes);
			if (list.Count == 0)
			{
				return null;
			}
			if (Reverse)
			{
				list.Reverse();
			}
			list.Insert(0, Vector3.zero);
			list.Add(Vector3.zero);
			for (int i = 0; i < list.Count; i++)
			{
				List<Vector3> list2;
				List<Vector3> list3 = (list2 = list);
				int index;
				int index2 = (index = i);
				Vector3 vector = list2[index];
				list3[index2] = vector + relativeOffset;
			}
			GoSpline goSpline = new GoSpline(list);
			goSpline.buildPath();
			goSpline.closePath();
			return goSpline;
		}

		private void Start()
		{
			GoSpline goSpline = BuildSpline(Vector3.zero);
			if (goSpline != null)
			{
				Go.to(base.transform, Duration, new GoTweenConfig().positionPath(goSpline, true, GoLookAtType.NextPathNode).setIterations(-1, GoLoopType.RestartFromBeginning));
			}
		}
	}
}
