using System.Collections.Generic;
using UnityEngine;

public class PathController : MonoBehaviour
{
	public Transform PathParent;

	public Color TrackEditorColor = Color.cyan;

	private void Start()
	{
	}

	public IList<Vector3> GetPathList()
	{
		List<Vector3> list = new List<Vector3>();
		Transform transform = null;
		transform = PathParent;
		if (transform != null)
		{
			foreach (Transform item in transform)
			{
				list.Add(item.position);
			}
		}
		return list;
	}

	public GoSpline GetPathSpline()
	{
		List<Vector3> list = GetPathList() as List<Vector3>;
		GoSpline goSpline = null;
		if (list != null)
		{
			goSpline = new GoSpline(list);
			goSpline.buildPath();
		}
		return goSpline;
	}

	public Vector3 GetPositionOnSpline(float t)
	{
		return GetPathSpline().getPointOnPath(t);
	}
}
