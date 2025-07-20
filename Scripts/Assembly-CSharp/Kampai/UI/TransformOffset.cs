using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI
{
	[AddComponentMenu("UI/Effects/Transform Offset", 14)]
	[RequireComponent(typeof(RectTransform))]
	public class TransformOffset : BaseMeshEffect
	{
		public enum Layout
		{
			None = 0,
			Center = 1,
			Vertical = 2,
			Horizontal = 3,
			Top = 4,
			Bottom = 5
		}

		[SerializeField]
		private Vector3 m_offsetPosition = Vector3.zero;

		[SerializeField]
		private Layout m_layout = Layout.Center;

		private RectTransform m_rectTransform;

		public RectTransform rectTransform
		{
			get
			{
				return m_rectTransform;
			}
		}

		public Vector3 offsetPosition
		{
			get
			{
				return m_offsetPosition;
			}
			set
			{
				if (!(m_offsetPosition == value))
				{
					m_offsetPosition = value;
					if (base.graphic != null)
					{
						base.graphic.SetVerticesDirty();
					}
					LayoutRebuilder.MarkLayoutForRebuild((RectTransform)base.transform);
				}
			}
		}

		protected override void Awake()
		{
			base.Awake();
			m_rectTransform = base.transform as RectTransform;
		}

		public override void ModifyMesh(VertexHelper vh)
		{
			if (IsActive())
			{
				List<UIVertex> list = new List<UIVertex>();
				vh.GetUIVertexStream(list);
				ModifyVertices(list);
				vh.Clear();
				vh.AddUIVertexTriangleStream(list);
			}
		}

		public void ModifyVertices(List<UIVertex> verts)
		{
			if (!IsActive())
			{
				return;
			}
			m_rectTransform = m_rectTransform ?? (base.transform as RectTransform);
			if (rectTransform == null)
			{
				return;
			}
			Rect rect = rectTransform.rect;
			Vector3 vector = Vector3.zero;
			if (m_layout != 0)
			{
				float num = float.MaxValue;
				float num2 = float.MaxValue;
				float num3 = 0f;
				float num4 = 0f;
				for (int i = 0; i < verts.Count; i++)
				{
					UIVertex uIVertex = verts[i];
					num = Mathf.Min(num, uIVertex.position.x);
					num3 = Mathf.Max(num3, uIVertex.position.x);
					num2 = Mathf.Min(num2, uIVertex.position.y);
					num4 = Mathf.Max(num4, uIVertex.position.y);
				}
				Vector2 center = rect.center;
				Vector2 vector2 = new Vector2(num, num2);
				Vector2 vector3 = new Vector2(num3 - num, num4 - num2);
				Vector2 vector4 = vector3 / 2f;
				switch (m_layout)
				{
				case Layout.Top:
					center.y = rect.yMax;
					vector4 *= 2f;
					break;
				case Layout.Bottom:
					center.y = rect.yMin;
					vector4 = Vector2.zero;
					break;
				}
				Vector2 vector5 = center - vector4;
				Vector2 vector6 = vector5 - vector2;
				switch (m_layout)
				{
				case Layout.Center:
					vector = new Vector3(vector6.x, vector6.y, 0f);
					break;
				case Layout.Vertical:
				case Layout.Top:
				case Layout.Bottom:
					vector = new Vector3(0f, vector6.y, 0f);
					break;
				case Layout.Horizontal:
					vector = new Vector3(vector6.x, 0f, 0f);
					break;
				}
			}
			vector += m_offsetPosition;
			for (int j = 0; j < verts.Count; j++)
			{
				UIVertex value = verts[j];
				value.position += vector;
				verts[j] = value;
			}
		}
	}
}
