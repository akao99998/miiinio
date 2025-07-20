using Kampai.Util;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class QuestRewardSliderView : KampaiView
	{
		public Text description;

		public Text itemQuantity;

		public Text currencyQuantity;

		public KampaiImage icon;

		private int ID { get; set; }
	}
}
