using UnityEngine;

namespace Kampai.Game.Mignette.EdwardMinionHands.View
{
	public class EdwardMinionHandsTopiaryViewObject : MonoBehaviour
	{
		public GameObject DefaultGO;

		public GameObject FlowerStateGO;

		public GameObject TwigObject;

		private void Start()
		{
			Reset();
		}

		public void Reset()
		{
			DefaultGO.SetActive(false);
			FlowerStateGO.SetActive(false);
			TwigObject.SetActive(false);
		}

		public void ShowDefaultModel()
		{
			DefaultGO.SetActive(true);
		}

		public void SetCooldownViewState(float pct)
		{
			if (pct >= 0.2f)
			{
				FlowerStateGO.SetActive(true);
			}
			if (pct >= 0.5f)
			{
				TwigObject.SetActive(true);
			}
		}
	}
}
