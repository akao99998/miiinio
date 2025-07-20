using UnityEngine;

public class AnimatedUV : MonoBehaviour
{
	public int materialIndex;

	public Vector2 uvAnimationRate = new Vector2(1f, 0f);

	public string textureName = "_MainTex";

	private Vector2 uvOffset = Vector2.zero;

	private Renderer rend;

	private void Start()
	{
		rend = GetComponent<Renderer>();
	}

	private void LateUpdate()
	{
		uvOffset += uvAnimationRate * Time.deltaTime;
		if (rend.enabled)
		{
			rend.materials[materialIndex].SetTextureOffset(textureName, uvOffset);
		}
	}
}
