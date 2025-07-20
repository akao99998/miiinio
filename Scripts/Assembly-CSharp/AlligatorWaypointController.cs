using System.Collections.Generic;
using UnityEngine;

public class AlligatorWaypointController : MonoBehaviour
{
	public enum PathType
	{
		Minion = 0,
		Alligator = 1
	}

	public Transform FollowCameraMarker;

	public Transform StartingWaypoint;

	public Transform StartingHampoint;

	public TextAsset AlligatorPath;

	public TextAsset MinionPath;

	private List<Vector3> alligatorPoints;

	private List<Vector3> minionPoints;

	private GoSpline alligatorSpline;

	private GoSpline minionSpline;

	private void Awake()
	{
		InitSplines();
	}

	private void InitSplines()
	{
		alligatorPoints = LoadPoints(AlligatorPath);
		alligatorSpline = new GoSpline(alligatorPoints);
		alligatorSpline.buildPath();
		minionPoints = LoadPoints(MinionPath);
		minionSpline = new GoSpline(minionPoints);
		minionSpline.buildPath();
	}

	private List<Vector3> LoadPoints(TextAsset asset)
	{
		if (asset == null)
		{
			return new List<Vector3>();
		}
		return GoSpline.bytesToVector3List(asset.bytes);
	}

	public GoSpline GetMinionSpline()
	{
		return minionSpline;
	}

	public GoSpline GetAlligatorSpline()
	{
		return alligatorSpline;
	}

	public Vector3 GetPositionOnMinionSpline(float t)
	{
		return minionSpline.getPointOnPath(t);
	}
}
