using Kampai.Util;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class MasterPlanCooldownRewardItemView : KampaiView
	{
		public KampaiImage icon;

		public Text countText;

		public void SetCount(int count)
		{
			if (count <= 1)
			{
				countText.gameObject.SetActive(false);
			}
			else
			{
				countText.text = count.ToString();
			}
		}
	}
}
