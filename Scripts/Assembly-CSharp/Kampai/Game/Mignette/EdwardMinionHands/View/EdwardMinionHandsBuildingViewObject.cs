using System;
using System.Collections.Generic;
using Kampai.Game.Mignette.View;
using Kampai.Main;
using UnityEngine;

namespace Kampai.Game.Mignette.EdwardMinionHands.View
{
	public class EdwardMinionHandsBuildingViewObject : MignetteBuildingViewObject
	{
		[Serializable]
		public class CollectableData
		{
			public int pointValue;

			public int numberInPool;
		}

		public GameObject DefaultBush;

		public Animation ShakeAnimation;

		public GameObject BushLocator;

		public GameObject CollectableGrabbedVfxPrefab;

		public GameObject[] TopiaryPrefabs;

		public CollectableData[] CollectablePoolData;

		public GameObject CollectablePrefab;

		public GameObject CuttingToolPrefab;

		public AnimationCurve TimeBetweenCollectables;

		public bool HasRequestedExit;

		private List<EdwardMinionHandsTopiaryViewObject> Topiaries = new List<EdwardMinionHandsTopiaryViewObject>();

		private EdwardMinionHandsTopiaryViewObject activeTopiary;

		public void Start()
		{
			Topiaries.Clear();
			GameObject[] topiaryPrefabs = TopiaryPrefabs;
			foreach (GameObject original in topiaryPrefabs)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(original);
				gameObject.transform.parent = base.transform;
				gameObject.transform.position = BushLocator.transform.position;
				EdwardMinionHandsTopiaryViewObject component = gameObject.GetComponent<EdwardMinionHandsTopiaryViewObject>();
				Topiaries.Add(component);
			}
			Reset();
			base.gameObject.AddComponent<MignetteBuildingCooldownView>();
		}

		public int DisplayRandomTopiary()
		{
			int num = UnityEngine.Random.Range(0, Topiaries.Count);
			DisplayTopiary(num);
			return num;
		}

		private void DisplayTopiary(int index)
		{
			if (activeTopiary == Topiaries[index])
			{
				return;
			}
			DefaultBush.SetActive(false);
			foreach (EdwardMinionHandsTopiaryViewObject topiary in Topiaries)
			{
				topiary.Reset();
			}
			activeTopiary = Topiaries[index];
			activeTopiary.ShowDefaultModel();
		}

		public void Reset()
		{
			HasRequestedExit = false;
			activeTopiary = null;
			DefaultBush.SetActive(true);
			foreach (EdwardMinionHandsTopiaryViewObject topiary in Topiaries)
			{
				topiary.Reset();
			}
		}

		public override void ResetCooldownView(PlayLocalAudioSignal localAudioSignal)
		{
			Reset();
		}

		public override void UpdateCooldownView(PlayLocalAudioSignal localAudioSignal, int buildingData, float pctDone)
		{
			if (pctDone < 1f)
			{
				if (!HasRequestedExit)
				{
					DisplayTopiary(buildingData);
				}
				if (activeTopiary != null)
				{
					activeTopiary.SetCooldownViewState(pctDone);
				}
			}
		}
	}
}
