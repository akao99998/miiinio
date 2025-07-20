using UnityEngine;

public class MinionMoveIndicator : MonoBehaviour
{
	public Animation indicatorAnimation;

	public MeshRenderer meshRenderer;

	private void Start()
	{
		indicatorAnimation.Play();
		meshRenderer.enabled = true;
	}
}
