using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kampai.Game.Mignette.ButterflyCatch.View
{
	public class ButterflyCatchGameController : MonoBehaviour
	{
		[Serializable]
		public class SpawnedButterfly
		{
			public Material wingMaterial;

			public int pointValue;

			public float probability = 0.1f;
		}

		public Transform CameraTransform;

		public float CameraFieldOfView = 45f;

		public float CameraNearClipPlane = 0.3f;

		public TextAsset[] MinionPaths;

		public TextAsset[] ButterflyPaths;

		private List<GoSpline> minionSplines;

		private List<GoSpline> butterflySplines;

		[SerializeField]
		private GameObject ButterflyPrefab;

		[SerializeField]
		private GameObject BeePrefab;

		[SerializeField]
		private GameObject NetPrefab;

		public GameObject ButterflyCaughtVfxPrefab;

		[SerializeField]
		private SpawnedButterfly[] spawnedButterflies;

		[SerializeField]
		private AnimationCurve maxButterfliesCurve;

		[SerializeField]
		private AnimationCurve maxBeesCurve;

		private float[] weightedButterfly;

		private void Awake()
		{
			InitSplines();
			InitButterflyWeights();
		}

		private void InitButterflyWeights()
		{
			float num = 0f;
			for (int i = 0; i < spawnedButterflies.Length; i++)
			{
				num += spawnedButterflies[i].probability;
			}
			weightedButterfly = new float[spawnedButterflies.Length];
			float num2 = 0f;
			for (int j = 0; j < spawnedButterflies.Length; j++)
			{
				num2 += spawnedButterflies[j].probability / num;
				weightedButterfly[j] = num2;
			}
		}

		private void InitSplines()
		{
			minionSplines = new List<GoSpline>();
			butterflySplines = new List<GoSpline>();
			BuildSplines(MinionPaths, minionSplines);
			BuildSplines(ButterflyPaths, butterflySplines);
		}

		private void BuildSplines(TextAsset[] paths, List<GoSpline> splineList)
		{
			foreach (TextAsset asset in paths)
			{
				List<Vector3> list = LoadPoints(asset);
				if (list.Count > 0)
				{
					list.Insert(0, Vector3.zero);
					list.Add(Vector3.zero);
					Vector3 position = base.transform.position;
					for (int j = 0; j < list.Count; j++)
					{
						List<Vector3> list2;
						List<Vector3> list3 = (list2 = list);
						int index;
						int index2 = (index = j);
						Vector3 vector = list2[index];
						list3[index2] = vector + position;
					}
					GoSpline goSpline = new GoSpline(list);
					goSpline.buildPath();
					goSpline.closePath();
					splineList.Add(goSpline);
				}
			}
		}

		private List<Vector3> LoadPoints(TextAsset asset)
		{
			if (asset == null)
			{
				return new List<Vector3>();
			}
			return GoSpline.bytesToVector3List(asset.bytes);
		}

		public GoSpline GetButterflySpline(int index)
		{
			return butterflySplines[index];
		}

		public GoSpline GetMinionSpline(int index)
		{
			return minionSplines[index];
		}

		public int GetButterflySplineCount()
		{
			return butterflySplines.Count;
		}

		public GameObject SpawnNet()
		{
			return UnityEngine.Object.Instantiate(NetPrefab);
		}

		public GameObject SpawnBee(int activeCount, float timeElapsed)
		{
			if (UnityEngine.Random.value < 0.1f && (float)activeCount < maxBeesCurve.Evaluate(timeElapsed))
			{
				return UnityEngine.Object.Instantiate(BeePrefab);
			}
			return null;
		}

		public GameObject SpawnButterfly(int activeCount, float timeElapsed)
		{
			if ((float)activeCount >= maxButterfliesCurve.Evaluate(timeElapsed))
			{
				return null;
			}
			float value = UnityEngine.Random.value;
			int num = 0;
			for (int i = 0; i < weightedButterfly.Length; i++)
			{
				if (value <= weightedButterfly[i])
				{
					num = i;
					break;
				}
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(ButterflyPrefab);
			ButterflyCatchButterflyViewObject component = gameObject.GetComponent<ButterflyCatchButterflyViewObject>();
			component.RendererForMaterial.material = spawnedButterflies[num].wingMaterial;
			component.myScore = spawnedButterflies[num].pointValue;
			return gameObject;
		}
	}
}
