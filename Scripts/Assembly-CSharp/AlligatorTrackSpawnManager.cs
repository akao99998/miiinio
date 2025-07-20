using System;
using System.Collections.Generic;
using UnityEngine;

public class AlligatorTrackSpawnManager : MonoBehaviour
{
	public enum SpawnConfig
	{
		Collectable = 0,
		Obstacle = 1,
		ObstaclesAndCollectables = 2
	}

	[Serializable]
	public struct SpawnerConfiguration
	{
		public float Position;

		public SpawnConfig Config;
	}

	public AlligatorWaypointController Path;

	public List<SpawnerConfiguration> Spawners = new List<SpawnerConfiguration>();

	public List<AlligatorTrackSpawnObject> LootTable = new List<AlligatorTrackSpawnObject>();

	public Transform SpawnedObjectsParent;

	private List<GameObject> spawnedObjects = new List<GameObject>();

	private void Start()
	{
		GenerateSpawnsFromLootTable();
	}

	public AlligatorTrackSpawnObject GetSpawnFromLootTable()
	{
		int index = UnityEngine.Random.Range(0, LootTable.Count);
		return LootTable[index];
	}

	public void GenerateSpawnsFromLootTable()
	{
		int count = Spawners.Count;
		for (int i = 0; i < count; i++)
		{
			Vector3 positionOnMinionSpline = Path.GetPositionOnMinionSpline(Spawners[i].Position);
			int num = 0;
			AlligatorTrackSpawnObject alligatorTrackSpawnObject = null;
			do
			{
				alligatorTrackSpawnObject = GetSpawnFromLootTable();
				num++;
				if (num > 100)
				{
					alligatorTrackSpawnObject = null;
					break;
				}
			}
			while (alligatorTrackSpawnObject.TrackObjectType != Spawners[i].Config && Spawners[i].Config != SpawnConfig.ObstaclesAndCollectables);
			if (!(alligatorTrackSpawnObject == null))
			{
				LootTable.Remove(alligatorTrackSpawnObject);
				GameObject gameObject = UnityEngine.Object.Instantiate(alligatorTrackSpawnObject.gameObject);
				gameObject.transform.position = positionOnMinionSpline;
				gameObject.transform.parent = SpawnedObjectsParent;
				spawnedObjects.Add(gameObject);
			}
		}
	}

	public void GenerateSpawnSeed(int spawns)
	{
		for (int i = 0; i < spawns; i++)
		{
			SpawnerConfiguration item = default(SpawnerConfiguration);
			item.Config = SpawnConfig.ObstaclesAndCollectables;
			item.Position = UnityEngine.Random.Range(0.25f, 0.85f);
			Spawners.Add(item);
		}
	}

	public void ClearSpawns()
	{
		int count = spawnedObjects.Count;
		for (int i = 0; i < count; i++)
		{
			if (spawnedObjects[i] != null)
			{
				UnityEngine.Object.Destroy(spawnedObjects[i]);
			}
		}
		spawnedObjects.Clear();
	}
}
