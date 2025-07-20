using System.Collections.Generic;
using UnityEngine;

public class RandomAnimPlayer : MonoBehaviour
{
	public Animator anim;

	public AnimatorStateInfo currentBaseState;

	public List<string> animNames = new List<string>();

	private int curAnimIndex;

	private int firstAnimHash;

	public float endTime = 0.7f;

	public GameObject followObj;

	public bool following;

	public Vector3 startPos = Vector3.zero;

	public Quaternion startQua = Quaternion.identity;

	private void Start()
	{
		if (anim == null)
		{
			anim = GetComponent<Animator>();
		}
		firstAnimHash = Animator.StringToHash("Base Layer." + animNames[0]);
		if (followObj == null)
		{
			following = false;
			return;
		}
		startPos = base.transform.position;
		startQua = base.transform.rotation;
	}

	private void Update()
	{
		currentBaseState = anim.GetCurrentAnimatorStateInfo(0);
		int fullPathHash = anim.GetCurrentAnimatorStateInfo(0).fullPathHash;
		if (fullPathHash == firstAnimHash && currentBaseState.normalizedTime > endTime)
		{
			curAnimIndex = Random.Range(0, animNames.Count - 1);
			if (curAnimIndex < animNames.Count)
			{
				anim.Play(animNames[curAnimIndex]);
			}
		}
		if (following)
		{
			if (followObj != null)
			{
				Vector3 position = followObj.transform.position;
				Quaternion rotation = followObj.transform.rotation;
				base.transform.position = position;
				base.transform.rotation = rotation;
			}
			else
			{
				base.transform.position = startPos;
				base.transform.rotation = startQua;
				following = false;
			}
		}
	}
}
