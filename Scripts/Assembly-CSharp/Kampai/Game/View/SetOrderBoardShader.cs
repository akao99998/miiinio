using UnityEngine;

namespace Kampai.Game.View
{
	public class SetOrderBoardShader : MonoBehaviour
	{
		private void Start()
		{
			GetComponent<Renderer>().material.shader = Shader.Find("Kampai/Background/(+4) Platform");
		}
	}
}
