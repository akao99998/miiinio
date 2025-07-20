using UnityEngine;

public class ForceAxisPosition : MonoBehaviour
{
	public enum AxisForce
	{
		X_AXIS = 0,
		Y_AXIS = 1,
		Z_AXIS = 2,
		XZ_AXIS = 3,
		XY_AXIS = 4,
		YZ_AXIS = 5,
		XYZ_AXIS = 6
	}

	public Vector3 forcedPosition = Vector3.zero;

	public AxisForce limitToAxis;

	private Vector3 pos = Vector3.zero;

	private void Start()
	{
	}

	private void Update()
	{
		pos = base.transform.position;
		switch (limitToAxis)
		{
		case AxisForce.X_AXIS:
			pos.x = forcedPosition.x;
			break;
		case AxisForce.Y_AXIS:
			pos.y = forcedPosition.y;
			break;
		case AxisForce.Z_AXIS:
			pos.z = forcedPosition.z;
			break;
		case AxisForce.XZ_AXIS:
			pos.x = forcedPosition.x;
			pos.z = forcedPosition.z;
			break;
		case AxisForce.XY_AXIS:
			pos.x = forcedPosition.x;
			pos.y = forcedPosition.y;
			break;
		case AxisForce.YZ_AXIS:
			pos.y = forcedPosition.y;
			pos.z = forcedPosition.z;
			break;
		case AxisForce.XYZ_AXIS:
			pos.x = forcedPosition.x;
			pos.y = forcedPosition.y;
			pos.z = forcedPosition.z;
			break;
		}
		base.transform.position = pos;
	}
}
