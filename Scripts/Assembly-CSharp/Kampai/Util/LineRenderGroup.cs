using System.Collections.Generic;
using UnityEngine;

namespace Kampai.Util
{
	public class LineRenderGroup : MonoBehaviour
	{
		public Transform pathParent;

		public List<Transform> pathNodes = new List<Transform>();

		public Color pathColor = Color.yellow;

		public bool closePath;

		public LineRenderer lineRndr;

		public bool hideNodesAtRuntime = true;

		private void Awake()
		{
			if (lineRndr == null)
			{
				lineRndr = base.gameObject.GetComponent<LineRenderer>();
			}
			if (lineRndr == null)
			{
				lineRndr = base.gameObject.AddComponent<LineRenderer>();
			}
		}

		private void Start()
		{
			if (pathParent == null)
			{
				pathParent = base.gameObject.transform;
			}
			if (hideNodesAtRuntime)
			{
				Renderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<Renderer>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].enabled = false;
				}
			}
			GetComponent<Renderer>().enabled = true;
		}

		public List<Vector3> GetPathList()
		{
			List<Vector3> list = new List<Vector3>();
			for (int i = 0; i < pathNodes.Count; i++)
			{
				list.Add(pathNodes[i].position);
			}
			return list;
		}

		public Vector3 GetStartPosition()
		{
			return base.transform.GetChild(0).transform.position;
		}
	}
}
