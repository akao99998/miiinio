using System;
using UnityEngine;

[ExecuteInEditMode]
public class AlligatorVFXParticleRotation : MonoBehaviour
{
	public enum FollowAxis
	{
		X = 0,
		Y = 1,
		Z = 2
	}

	public ParticleSystem particleSys;

	public FollowAxis followAxis = FollowAxis.Y;

	private void Start()
	{
		if (particleSys == null)
		{
			particleSys = base.gameObject.GetComponent<ParticleSystem>();
		}
		if (particleSys == null)
		{
			throw new ArgumentNullException("particleSys", "Please assign particleSys on gameObject " + base.gameObject.name);
		}
	}

	private void Update()
	{
		switch (followAxis)
		{
		case FollowAxis.X:
			particleSys.startRotation = base.transform.rotation.eulerAngles.x * ((float)Math.PI / 180f);
			break;
		case FollowAxis.Y:
			particleSys.startRotation = base.transform.rotation.eulerAngles.y * ((float)Math.PI / 180f);
			break;
		case FollowAxis.Z:
			particleSys.startRotation = base.transform.rotation.eulerAngles.z * ((float)Math.PI / 180f);
			break;
		}
	}
}
