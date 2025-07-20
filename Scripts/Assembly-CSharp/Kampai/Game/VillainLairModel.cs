using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kampai.Game
{
	public class VillainLairModel
	{
		public enum LairPrefabType
		{
			LAIR = 0,
			LOCKED_PLOT = 1,
			UNLOCKED_PLOT = 2
		}

		public IDictionary<int, GameObject> villainLairInstances;

		public Dictionary<int, GameObject> asyncLoadedPrefabs = new Dictionary<int, GameObject>();

		public VillainLair currentActiveLair { get; set; }

		public bool goingToLair { get; set; }

		public bool leavingLair { get; set; }

		public bool isPortalResourceModalOpen { get; set; }

		public bool areLairAssetsLoaded
		{
			get
			{
				return asyncLoadedPrefabs.Keys.Count >= Enum.GetValues(typeof(LairPrefabType)).Length;
			}
		}

		public bool seenCooldownAlert { get; set; }

		public GoTweenFlow cameraFlow { get; set; }

		public VillainLairModel()
		{
			villainLairInstances = new Dictionary<int, GameObject>();
			currentActiveLair = null;
			seenCooldownAlert = false;
		}
	}
}
