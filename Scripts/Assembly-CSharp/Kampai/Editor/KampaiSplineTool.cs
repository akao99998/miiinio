using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kampai.Editor
{
	public class KampaiSplineTool : MonoBehaviour
	{
		public Color pathColor = Color.magenta;

		public List<Vector3> nodes = new List<Vector3>
		{
			new Vector3(-5f, 0f, -5f),
			new Vector3(5f, 0f, 5f)
		};

		public bool isLoop;

		public bool useStandardHandles;

		public bool initialized;

		public bool saveRelative;

		public void Initialize()
		{
			if (initialized)
			{
				return;
			}
			initialized = true;
			if (base.transform.childCount < 2)
			{
				nodes = new List<Vector3>
				{
					new Vector3(-5f, 0f, -5f),
					new Vector3(5f, 0f, 5f)
				};
				return;
			}
			nodes.Clear();
			Vector3 position = base.transform.position;
			for (int i = 0; i < base.transform.childCount; i++)
			{
				nodes.Add(base.transform.GetChild(i).transform.position - position);
			}
		}

		public List<Vector3> GetPath()
		{
			List<Vector3> list = new List<Vector3>(nodes);
			if (isLoop)
			{
				list.Insert(0, Vector3.zero);
				list.Add(Vector3.zero);
			}
			Vector3 position = base.transform.position;
			for (int i = 0; i < list.Count; i++)
			{
				List<Vector3> list2;
				List<Vector3> list3 = (list2 = list);
				int index;
				int index2 = (index = i);
				Vector3 vector = list2[index];
				list3[index2] = vector + position;
			}
			return list;
		}

		public void OnDrawGizmos()
		{
			GoSpline goSpline = new GoSpline(GetPath());
			goSpline.buildPath();
			if (isLoop)
			{
				goSpline.closePath();
			}
			Gizmos.color = pathColor;
			goSpline.drawGizmos(50f);
		}

		private void Update()
		{
			throw new NotImplementedException(string.Format("Invalid KampaiSplineTool found on {0}.", base.name));
		}
	}
}
