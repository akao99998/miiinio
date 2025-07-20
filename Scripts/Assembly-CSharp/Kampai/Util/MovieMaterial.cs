using UnityEngine;

namespace Kampai.Util
{
	[RequireComponent(typeof(Renderer))]
	public class MovieMaterial : MonoBehaviour
	{
		public float fps = 15f;

		public int frames = 1;

		private int rows = 1;

		private int cols = 1;

		public bool startAtTop = true;

		private Vector2 scale = Vector2.zero;

		private void Start()
		{
			Reset();
		}

		private void Reset()
		{
			scale = GetComponent<Renderer>().material.mainTextureScale;
			cols = (int)Mathf.Round(1f / scale.x);
			rows = (int)Mathf.Round(1f / scale.y);
		}

		private void Update()
		{
			int num = (int)(Time.time * fps);
			num %= frames;
			int num2 = num % cols;
			int num3 = num / cols;
			if (startAtTop)
			{
				num3 = rows - 1 - num3;
			}
			Vector2 mainTextureOffset = new Vector2((float)num2 * scale.x, (float)num3 * scale.y);
			GetComponent<Renderer>().material.mainTextureOffset = mainTextureOffset;
		}
	}
}
