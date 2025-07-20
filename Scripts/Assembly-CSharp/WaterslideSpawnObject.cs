using UnityEngine;

public class WaterslideSpawnObject : MonoBehaviour
{
	public enum SpawnType
	{
		Collectable = 0,
		Obstacle = 1
	}

	public SpawnType WaterslideObjectType = SpawnType.Obstacle;

	public float PointValue = 10f;
}
