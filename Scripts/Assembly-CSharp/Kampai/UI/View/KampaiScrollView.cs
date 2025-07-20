using System;
using System.Collections;
using System.Collections.Generic;
using Kampai.Game;
using Kampai.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class KampaiScrollView : KampaiView, IEnumerable, IEnumerable<MonoBehaviour>
	{
		public enum MoveDirection
		{
			Start = 0,
			End = 1,
			LastLocation = 2
		}

		[SerializeField]
		private float m_colunmNumber = 1f;

		[SerializeField]
		private float m_rowNumber = 1f;

		public ScrollRect ScrollRect;

		public RectTransform ItemContainer;

		public IList<MonoBehaviour> ItemViewList = new List<MonoBehaviour>();

		public float ColumnNumber
		{
			get
			{
				return m_colunmNumber;
			}
			set
			{
				m_colunmNumber = value;
			}
		}

		public float RowNumber
		{
			get
			{
				return m_rowNumber;
			}
			set
			{
				m_rowNumber = value;
			}
		}

		public MonoBehaviour this[int index]
		{
			get
			{
				return ItemViewList[index];
			}
			set
			{
				ItemViewList[index] = value;
			}
		}

		public float ItemSize { get; private set; }

		public bool isVertical
		{
			get
			{
				return ScrollRect.vertical;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		internal void SetupScrollView(float columns = -1f, MoveDirection moveDirection = MoveDirection.LastLocation)
		{
			if (columns == -1f)
			{
				columns = ((!isVertical) ? m_rowNumber : m_colunmNumber);
			}
			if (ItemViewList == null)
			{
				return;
			}
			float y = ItemContainer.anchoredPosition.y;
			int count = ItemViewList.Count;
			ScrollRect.verticalNormalizedPosition = 1f;
			int num = count % Mathf.FloorToInt(columns);
			float num2 = ItemSize * (float)(count / Mathf.FloorToInt(columns) + ((num != 0) ? 1 : 0));
			if (isVertical)
			{
				ItemContainer.offsetMin = new Vector2(0f, 0f - num2);
				ItemContainer.offsetMax = new Vector2(0f, 0f);
			}
			else
			{
				ItemContainer.offsetMin = new Vector2(0f, 0f);
				ItemContainer.offsetMax = new Vector2(num2, 0f);
			}
			if ((float)count <= columns * m_rowNumber)
			{
				ScrollRect.movementType = ScrollRect.MovementType.Clamped;
				moveDirection = MoveDirection.Start;
			}
			else
			{
				ScrollRect.movementType = ScrollRect.MovementType.Elastic;
			}
			switch (moveDirection)
			{
			case MoveDirection.Start:
				ItemContainer.anchoredPosition = ((!isVertical) ? new Vector2(0f, 0f) : new Vector2(ItemContainer.anchoredPosition.x, 0f));
				break;
			case MoveDirection.End:
				ItemContainer.anchoredPosition = ((!isVertical) ? new Vector2(y, ItemContainer.anchoredPosition.y) : new Vector2(ItemContainer.anchoredPosition.x, y));
				if (num != 0)
				{
					TweenToPosition(new Vector2(0f, 0f), 1f);
				}
				break;
			case MoveDirection.LastLocation:
				ItemContainer.anchoredPosition = ((!isVertical) ? new Vector2(y, 0f) : new Vector2(ItemContainer.anchoredPosition.x, y));
				break;
			}
		}

		internal Vector3 GetViewSize()
		{
			RectTransform rectTransform = ScrollRect.transform as RectTransform;
			return (!(rectTransform == null)) ? new Vector3(rectTransform.rect.width / m_colunmNumber, rectTransform.rect.height / m_rowNumber) : Vector3.zero;
		}

		private RectTransform PositionItem(MonoBehaviour view, int index)
		{
			RectTransform rectTransform = view.transform as RectTransform;
			if (rectTransform == null)
			{
				return null;
			}
			float num = 1f;
			if (isVertical)
			{
				int num2 = Mathf.FloorToInt(m_colunmNumber);
				int num3 = index % num2;
				int num4 = index / num2;
				rectTransform.sizeDelta = GetViewSize();
				ItemSize = rectTransform.sizeDelta.y;
				rectTransform.SetParent(ItemContainer, false);
				rectTransform.offsetMin = new Vector2(0f, (float)(-num4 - 1) * ItemSize);
				rectTransform.offsetMax = new Vector2(0f, (float)(-num4) * ItemSize);
				rectTransform.anchorMin = new Vector2(num / m_colunmNumber * (float)num3, 1f);
				rectTransform.anchorMax = new Vector2(num / m_colunmNumber * (float)(num3 + 1), 1f);
			}
			else
			{
				int num5 = Mathf.FloorToInt(m_rowNumber);
				int num6 = index % num5;
				int num7 = index / num5;
				rectTransform.sizeDelta = GetViewSize();
				ItemSize = rectTransform.sizeDelta.x;
				rectTransform.SetParent(ItemContainer, false);
				rectTransform.offsetMin = new Vector2((float)num7 * ItemSize, 1f);
				rectTransform.offsetMax = new Vector2((float)(num7 + 1) * ItemSize, 1f);
				rectTransform.anchorMin = new Vector2(0f, num / m_rowNumber * (float)num6);
				rectTransform.anchorMax = new Vector2(0f, num / m_rowNumber * (float)(num6 + 1));
			}
			rectTransform.localScale = Vector3.one;
			return rectTransform;
		}

		public void ClearItems()
		{
			foreach (MonoBehaviour itemView in ItemViewList)
			{
				UnityEngine.Object.Destroy(itemView.gameObject);
			}
			ItemViewList.Clear();
		}

		public void AddList<T>(IList<T> items, Func<int, T, MonoBehaviour> createItemFunc, Func<T, bool> hasItemFunc = null, bool setupScrollAfter = true) where T : Instance
		{
			if (createItemFunc == null)
			{
				SetupScrollView();
				return;
			}
			foreach (T item in items)
			{
				if (hasItemFunc == null || hasItemFunc(item))
				{
					MonoBehaviour slotView = createItemFunc(ItemViewList.Count, item);
					AddItem(slotView);
				}
			}
			if (setupScrollAfter)
			{
				SetupScrollView();
			}
		}

		public void AddItem(MonoBehaviour slotView)
		{
			if (!(slotView == null))
			{
				PositionItem(slotView, ItemViewList.Count);
				ItemViewList.Add(slotView);
			}
		}

		public void TweenToPosition(Vector2 newPosition, float tweenTime)
		{
			if (tweenTime > 0f)
			{
				GoTweenConfig config = new GoTweenConfig().vector2Prop("normalizedPosition", newPosition).setEaseType(GoEaseType.SineOut);
				GoTween tween = new GoTween(ScrollRect, tweenTime, config);
				Go.addTween(tween);
			}
			else
			{
				ScrollRect.normalizedPosition = newPosition;
			}
		}

		public void EnableScrolling(bool horizontal, bool vertial)
		{
			ScrollRect.horizontal = horizontal;
			ScrollRect.vertical = vertial;
		}

		public IEnumerator<MonoBehaviour> GetEnumerator()
		{
			return ItemViewList.GetEnumerator();
		}
	}
}
