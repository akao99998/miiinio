using Kampai.Game.Mignette.AlligatorSkiing.View;
using UnityEngine;

public class AlligatorCheckpoint : MonoBehaviour
{
	public Material CheckpointPassedMat;

	public MeshRenderer[] RenderersToSwap;

	private void OnTriggerEnter(Collider other)
	{
		AlligatorSkiingMinionHardpointViewObject component = other.GetComponent<AlligatorSkiingMinionHardpointViewObject>();
		if (component != null)
		{
			MeshRenderer[] renderersToSwap = RenderersToSwap;
			foreach (MeshRenderer meshRenderer in renderersToSwap)
			{
				meshRenderer.material = CheckpointPassedMat;
			}
		}
	}
}
