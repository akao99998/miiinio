using UnityEngine;

namespace Kampai.Util
{
	public class ColorAnimator : MonoBehaviour
	{
		public Color color = Color.white;

		private void Update()
		{
			GetComponent<Renderer>().material.color = color;
		}
	}
}
