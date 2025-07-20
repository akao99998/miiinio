using Kampai.Game.Mignette.AlligatorSkiing.View;
using UnityEngine;

public class AlligatorVFXTrigger : MonoBehaviour
{
	public GameObject VFXGameObject;

	public Transform VFXParentTransform;

	public bool AttachToTransform;

	private Transform vfxMarkerTransform;

	private Transform vfxTransform;

	private void OnTriggerEnter(Collider other)
	{
		AlligatorSkiingMinionHardpointViewObject component = other.GetComponent<AlligatorSkiingMinionHardpointViewObject>();
		if (component != null)
		{
			vfxTransform = Object.Instantiate(VFXGameObject).transform;
			vfxTransform.SetParent(VFXParentTransform, false);
			vfxMarkerTransform = component.MinionTriggerVFXMarker;
		}
	}

	private void Update()
	{
		if (AttachToTransform && vfxTransform != null)
		{
			VFXParentTransform.position = vfxMarkerTransform.position;
		}
	}
}
