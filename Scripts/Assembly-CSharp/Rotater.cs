using System;
using UnityEngine;

public class Rotater : MonoBehaviour
{
	private Vector3 rotationSession = default(Vector3);

	private Vector3 targetRotation = default(Vector3);

	private bool shouldRotate = true;

	private bool shouldTrackRotation;

	public float XRate;

	public float YRate;

	public float ZRate = 30f;

	public event EventHandler RotationSessionCompleteCallback;

	private void FixedUpdate()
	{
		if (shouldRotate)
		{
			Vector3 zero = Vector3.zero;
			zero.x = Time.deltaTime * XRate;
			zero.y = Time.deltaTime * YRate;
			zero.z = Time.deltaTime * ZRate;
			CheckRotationSession(zero);
			base.transform.Rotate(zero);
		}
	}

	public void SetTargetRotation(Vector3 targetAccumulation)
	{
		shouldRotate = true;
		shouldTrackRotation = true;
		targetRotation = targetAccumulation;
	}

	private void CheckRotationSession(Vector3 rot)
	{
		if (shouldTrackRotation)
		{
			rotationSession += rot;
			if ((rotationSession - targetRotation).magnitude < 0.1f)
			{
				shouldRotate = false;
				rotationSession = Vector3.zero;
				OnRotationComplete();
			}
		}
	}

	private void OnRotationComplete()
	{
		EventHandler rotationSessionCompleteCallback = this.RotationSessionCompleteCallback;
		if (rotationSessionCompleteCallback != null)
		{
			rotationSessionCompleteCallback(this, EventArgs.Empty);
		}
	}
}
