using System;
using System.Collections.Generic;
using UnityEngine;

public class TrackSpawnManager : MonoBehaviour
{
	[Serializable]
	public struct ObjectToSpawn
	{
		public GameObject SpawnPrefab;

		public int MaxCount;

		public float Weight;
	}

	public PathController Path;

	public List<float> SpawerLocations = new List<float>();

	public ObjectToSpawn[] PossibleSpawns;

	public Transform SpawnedObjectsParent;

	private List<GameObject> spawnedObjects = new List<GameObject>();

	private Dictionary<int, int> ObjectCounts = new Dictionary<int, int>();

	private void Start()
	{
		InititalizeIds();
		GenerateSpawnsOnTrack();
	}

	private void InititalizeIds()
	{
		int num = PossibleSpawns.Length;
		for (int i = 0; i < num; i++)
		{
			ObjectCounts.Add(i, 0);
		}
	}

	public void GenerateSpawnsOnTrack()
	{
		int max = PossibleSpawns.Length;
		int count = SpawerLocations.Count;
		for (int i = 0; i < count; i++)
		{
			Vector3 positionOnSpline = Path.GetPositionOnSpline(SpawerLocations[i]);
			int num = 0;
			do
			{
				num = UnityEngine.Random.Range(0, max);
			}
			while (ObjectCounts[num] >= PossibleSpawns[num].MaxCount && PossibleSpawns[num].MaxCount > -1);
			GameObject gameObject = UnityEngine.Object.Instantiate(PossibleSpawns[num].SpawnPrefab);
			gameObject.transform.position = positionOnSpline;
			gameObject.transform.parent = SpawnedObjectsParent;
			spawnedObjects.Add(gameObject);
			Dictionary<int, int> objectCounts;
			Dictionary<int, int> dictionary = (objectCounts = ObjectCounts);
			int key;
			int key2 = (key = num);
			key = objectCounts[key];
			dictionary[key2] = key + 1;
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
