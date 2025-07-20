using UnityEngine;

namespace Kampai.Game.Mignette.BalloonBarrage.View
{
	public class BalloonBarrageStaticTargetViewObject : MonoBehaviour
	{
		public int Score = 1;

		private void Start()
		{
			Reset();
		}

		public void Reset()
		{
			base.gameObject.SetActive(true);
		}

		public void OnHit()
		{
			base.gameObject.SetActive(false);
		}
	}
}
