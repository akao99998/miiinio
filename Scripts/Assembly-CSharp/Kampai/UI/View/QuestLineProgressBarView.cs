using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class QuestLineProgressBarView : KampaiView
	{
		public Text QuestLineTitleText;

		public Text QuestLineProgressText;

		public Image FillImage;

		internal void SetTitle(string title)
		{
			QuestLineTitleText.text = title;
		}

		internal void UpdateProgress(int CompletedCount, int TotalCount)
		{
			QuestLineProgressText.text = string.Format("{0}/{1}", CompletedCount, TotalCount);
			float x = (float)CompletedCount / (float)TotalCount;
			FillImage.rectTransform.anchorMax = new Vector2(x, 1f);
		}
	}
}
