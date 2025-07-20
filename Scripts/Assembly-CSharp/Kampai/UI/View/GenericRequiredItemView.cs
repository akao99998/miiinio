using UnityEngine;

namespace Kampai.UI.View
{
	public class GenericRequiredItemView : RequiredItemView
	{
		public RectTransform greenBorder;

		public RectTransform redBorder;

		public int Cost { get; set; }
	}
}
