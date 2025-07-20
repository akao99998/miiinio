using UnityEngine;

public class UnlockableScrollableButtonView : ScrollableButtonView
{
	public GameObject AnimatableObject;

	protected override void Start()
	{
		base.Start();
		animator = AnimatableObject.GetComponent<Animator>();
	}
}
