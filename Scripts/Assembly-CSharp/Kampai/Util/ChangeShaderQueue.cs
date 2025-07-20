using Kampai.Util.Graphics;
using UnityEngine;

namespace Kampai.Util
{
	public class ChangeShaderQueue : MonoBehaviour
	{
		public RenderQueue Queue = RenderQueue.Background;

		public int Offset;

		public int MaterialIndex;

		private void Awake()
		{
			Renderer component = GetComponent<Renderer>();
			if (component != null && component.materials != null && component.materials[MaterialIndex] != null)
			{
				component.materials[MaterialIndex].renderQueue = (int)(Queue + Offset);
			}
		}
	}
}
