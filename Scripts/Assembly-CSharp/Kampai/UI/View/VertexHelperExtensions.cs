using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public static class VertexHelperExtensions
	{
		private static readonly Vector4 s_DefaultTangent = new Vector4(1f, 0f, 0f, -1f);

		private static readonly Vector3 s_DefaultNormal = Vector3.back;

		public static void AddVert(this VertexHelper vh, Vector3 position, Color32 color, Vector2 uv0, Vector2 uv1)
		{
			vh.AddVert(position, color, uv0, uv1, s_DefaultNormal, s_DefaultTangent);
		}
	}
}
