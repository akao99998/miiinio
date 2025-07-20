using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kampai.UI
{
	[RequireComponent(typeof(LayoutElement))]
	[RequireComponent(typeof(RectTransform))]
	[ExecuteInEditMode]
	[AddComponentMenu("UI/Layout/Aspect Ratio Layout Element", 14)]
	public class AspectRatioLayoutElement : UIBehaviour
	{
		private RectTransform m_rectTransform;

		private LayoutElement m_layoutElement;

		public bool useWidth = true;

		public float m_paddingPercent = 1f;

		public LayoutElement LayoutElement
		{
			get
			{
				LayoutElement obj = m_layoutElement ?? GetComponent<LayoutElement>();
				LayoutElement result = obj;
				m_layoutElement = obj;
				return result;
			}
		}

		public RectTransform rectTransform
		{
			get
			{
				RectTransform obj = m_rectTransform ?? GetComponent<RectTransform>();
				RectTransform result = obj;
				m_rectTransform = obj;
				return result;
			}
		}

		protected override void Start()
		{
			base.Start();
			UpdateRect();
		}

		protected override void OnTransformParentChanged()
		{
			base.OnTransformParentChanged();
			UpdateRect();
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			UpdateRect();
		}

		protected override void OnDisable()
		{
			UpdateRect();
			base.OnDisable();
		}

		protected override void OnDidApplyAnimationProperties()
		{
			UpdateRect();
		}

		protected override void OnBeforeTransformParentChanged()
		{
			ResetRect();
		}

		private void ResetRect()
		{
			if (!(LayoutElement == null) && !(rectTransform == null))
			{
				if (useWidth)
				{
					LayoutElement layoutElement = LayoutElement;
					float num = 0f;
					LayoutElement.minWidth = num;
					layoutElement.preferredWidth = num;
				}
				else
				{
					LayoutElement layoutElement2 = LayoutElement;
					float num = 0f;
					LayoutElement.minHeight = num;
					layoutElement2.preferredHeight = num;
				}
			}
		}

		private void UpdateRect()
		{
			if (!(LayoutElement == null) && !(rectTransform == null))
			{
				if (useWidth)
				{
					LayoutElement layoutElement = LayoutElement;
					float num = rectTransform.rect.height * m_paddingPercent;
					LayoutElement.minWidth = num;
					layoutElement.preferredWidth = num;
				}
				else
				{
					LayoutElement layoutElement2 = LayoutElement;
					float num = rectTransform.rect.width * m_paddingPercent;
					LayoutElement.minHeight = num;
					layoutElement2.preferredHeight = num;
				}
				LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
			}
		}
	}
}
