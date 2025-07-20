using UnityEngine;

public class AlligatorHamController : MonoBehaviour
{
	public SkinnedMeshRenderer PoleMeshRenderer;

	private Shader currentShader;

	private Shader hiddenShader;

	public void Awake()
	{
		currentShader = PoleMeshRenderer.materials[1].shader;
		hiddenShader = Shader.Find("Kampai/Standard/Hidden");
	}

	public void DisplayHam(bool display)
	{
		if (display)
		{
			PoleMeshRenderer.materials[1].shader = currentShader;
		}
		else
		{
			PoleMeshRenderer.materials[1].shader = hiddenShader;
		}
	}
}
