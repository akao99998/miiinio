using System;
using UnityEngine;

namespace Kampai.Game.Mignette.BalloonBarrage.View
{
	public class BalloonBarrageGameController : MonoBehaviour
	{
		[Serializable]
		public class StaticBasketAndPoints
		{
			public int ScoreValue;

			public GameObject[] BasketLocators;

			public int BasketMaterialIndex;
		}

		public GameObject MignetteObjects;

		public Transform FloatingMinionLocator;

		public StaticBasketAndPoints[] MinionStaticTargetLocators;

		public GameObject MangoToShowForPrepareThrow;
	}
}
