using UnityEngine;

namespace Kampai.Util
{
	public struct KampaiColor
	{
		public float r { get; set; }

		public float g { get; set; }

		public float b { get; set; }

		public float a { get; set; }

		public Color GetColor()
		{
			return new Color(r, g, b, a);
		}
	}
}
