using Kampai.Game.Mignette.WaterSlide.View;
using UnityEngine;

public class WaypointNode : MonoBehaviour
{
	public bool UpdateSpeed;

	public float Speed;

	public float acceleration;

	public bool LockInput;

	public bool TurnAnimation;

	public bool EnableWakeVFX;

	public bool DisableWakeVFX;

	public bool FollowSplineRotation = true;

	private WaterSlideMignetteManagerView parentView;

	public void Start()
	{
		parentView = Object.FindObjectOfType<WaterSlideMignetteManagerView>();
	}

	private void OnTriggerEnter(Collider other)
	{
		PathAgent componentInParent = other.transform.GetComponentInParent<PathAgent>();
		if (componentInParent != null)
		{
			if (UpdateSpeed)
			{
				componentInParent.ChangeSpeed(Speed, acceleration);
			}
			if (TurnAnimation)
			{
				componentInParent.OnMinionTurn();
			}
			else
			{
				componentInParent.OnMinionOutofTurn();
			}
			if (EnableWakeVFX)
			{
				componentInParent.VFXManager.DisplayMinionWake(true);
				parentView.EnableWaterAudio(true);
			}
			if (DisableWakeVFX)
			{
				componentInParent.VFXManager.DisplayMinionWake(false);
				parentView.EnableWaterAudio(false);
			}
			if (componentInParent.FollowSplineRotation != FollowSplineRotation)
			{
				componentInParent.FollowSplineRotation = FollowSplineRotation;
			}
			componentInParent.OnRotateMinion(base.transform.localRotation);
		}
	}
}
