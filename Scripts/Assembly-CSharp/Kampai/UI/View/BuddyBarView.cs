using System.Collections.Generic;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class BuddyBarView : KampaiView
	{
		public ButtonView SkrimButtonView;

		public RectTransform ScrollView;

		public RectTransform ScrollItemParent;

		private int rowCount;

		private IList<BuddyAvatarView> itemViews;

		private float itemWidth;

		internal void Init()
		{
			itemViews = new List<BuddyAvatarView>();
		}

		internal void SetupRowCount(int itemCount)
		{
			rowCount = ((!DeviceCapabilities.IsTablet()) ? 1 : 2);
			rowCount = ((itemCount <= 1) ? 1 : rowCount);
			if (rowCount == 1)
			{
				ScrollView.anchorMin = new Vector2(ScrollView.anchorMin.x, ScrollView.anchorMax.y / 2f);
			}
		}

		internal void InitScrollView(int itemCount)
		{
			float num = itemWidth * (float)(itemCount / rowCount + ((itemCount % rowCount != 0) ? 1 : 0));
			float num2 = ScrollView.offsetMax.x - ScrollView.offsetMin.x;
			if (num <= num2)
			{
				ScrollView.offsetMin = new Vector2(ScrollView.offsetMax.x - num, ScrollView.offsetMin.y);
				ScrollView.GetComponent<ScrollRect>().horizontal = false;
			}
			else
			{
				ScrollView.GetComponent<ScrollRect>().horizontal = true;
			}
			ScrollItemParent.offsetMin = new Vector2(0f, 0f);
			ScrollItemParent.offsetMax = new Vector2(num, 0f);
			base.gameObject.SetActive(true);
			SkrimButtonView.gameObject.SetActive(true);
		}

		internal void AddItem(BuddyAvatarView view, int index)
		{
			int num = index % rowCount;
			int num2 = index / rowCount;
			RectTransform rectTransform = view.transform as RectTransform;
			itemWidth = rectTransform.sizeDelta.x / 2f;
			rectTransform.parent = ScrollItemParent;
			rectTransform.offsetMin = new Vector2((float)num2 * itemWidth, 0f);
			rectTransform.offsetMax = new Vector2((float)(num2 + 1) * itemWidth, 0f);
			rectTransform.anchorMin = new Vector2(0f, 1f / (float)rowCount * (float)num);
			rectTransform.anchorMax = new Vector2(0f, 1f / (float)rowCount * (float)(num + 1));
			rectTransform.localPosition = new Vector3(rectTransform.localPosition.x, rectTransform.localPosition.y, 0f);
			rectTransform.localScale = Vector3.one;
			itemViews.Add(view);
		}

		internal void Close()
		{
			base.gameObject.SetActive(false);
			SkrimButtonView.gameObject.SetActive(false);
			foreach (BuddyAvatarView itemView in itemViews)
			{
				Object.Destroy(itemView.gameObject);
			}
			itemViews.Clear();
		}

		internal bool IsOpen()
		{
			return itemViews.Count > 0;
		}
	}
}
