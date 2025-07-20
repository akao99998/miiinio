using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class CollectionRewardIndicator : MonoBehaviour
	{
		public Text RewardCountLabel;

		public KampaiImage RewardImage;

		public Text RewardLocLabel;

		public void PlayCollectAnimation()
		{
			GetComponent<Animator>().Play("Collect", 0, 0f);
		}
	}
}
