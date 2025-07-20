using System.Collections;
using Kampai.Main;
using Kampai.UI.View;
using Kampai.Util;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using strange.extensions.signal.impl;

public class ScrollableButtonView : KampaiView, IPointerDownHandler, IDragHandler, IPointerUpHandler, IEventSystemHandler, IDoubleConfirmHandler
{
	public Signal ClickedSignal = new Signal();

	private IEnumerator waitEnumerator;

	private int tapCount;

	protected bool doubleTap;

	private bool isInConfirmState;

	internal Animator animator;

	[SerializeField]
	private bool ignoreAnimator;

	private ScrollRect myScrollView;

	private float currentOffset;

	private Vector2 startPos;

	[Inject]
	public ILocalPersistanceService localPersistService { get; set; }

	[Inject]
	public PlayGlobalSoundFXSignal playSFXSignal { get; set; }

	private float MaxClickOffset
	{
		get
		{
			return 0.15f * Screen.dpi;
		}
	}

	private ScrollRect scrollRect
	{
		get
		{
			if (myScrollView == null)
			{
				myScrollView = base.transform.GetComponentInParent<ScrollRect>();
			}
			return myScrollView;
		}
	}

	public virtual void OnPointerDown(PointerEventData eventData)
	{
		startPos = eventData.position;
		if (scrollRect != null)
		{
			myScrollView.OnBeginDrag(eventData);
		}
		if (waitEnumerator != null)
		{
			StopCoroutine(waitEnumerator);
			waitEnumerator = null;
		}
	}

	public virtual void OnDrag(PointerEventData eventData)
	{
		if (myScrollView != null)
		{
			myScrollView.OnDrag(eventData);
		}
	}

	public virtual void OnPointerUp(PointerEventData eventData)
	{
		currentOffset = (eventData.position - startPos).magnitude;
		if (myScrollView != null)
		{
			myScrollView.OnEndDrag(eventData);
		}
		Button component = GetComponent<Button>();
		if (component != null)
		{
			if (component.interactable)
			{
				if (currentOffset < MaxClickOffset)
				{
					ButtonClicked();
				}
				else
				{
					ResetAnim();
				}
			}
			KampaiButton kampaiButton = component as KampaiButton;
			if (kampaiButton != null)
			{
				kampaiButton.ChangeToNormalState();
			}
		}
		if (base.gameObject.activeInHierarchy && isInConfirmState && waitEnumerator == null)
		{
			waitEnumerator = Wait();
			StartCoroutine(waitEnumerator);
		}
	}

	public virtual void ButtonClicked()
	{
		updateTapCount();
		ClickedSignal.Dispatch();
		if (!isDoubleConfirmed())
		{
			playSFXSignal.Dispatch("Play_button_click_01");
			ShowConfirmMessage();
		}
		else
		{
			ResetAnim();
		}
	}

	public void ResetTapState()
	{
		tapCount = 0;
		if (animator != null)
		{
			animator.SetBool("Pressed_Confirm", false);
		}
		isInConfirmState = false;
	}

	public void updateTapCount()
	{
		if (doubleTap)
		{
			if (tapCount < 2)
			{
				tapCount++;
			}
			else
			{
				tapCount = 1;
			}
		}
	}

	public virtual void EnableDoubleConfirm()
	{
		doubleTap = true;
	}

	public void DisableDoubleConfirm()
	{
		doubleTap = false;
	}

	public virtual void ShowConfirmMessage()
	{
		bool flag = doubleTap && localPersistService.GetDataIntPlayer("DoublePurchaseConfirm") != 0;
		if (animator != null && flag)
		{
			animator.SetBool("Pressed_Confirm", flag);
			isInConfirmState = flag;
			if (base.gameObject.activeSelf && waitEnumerator != null)
			{
				StopCoroutine(waitEnumerator);
			}
			if (base.gameObject.activeSelf && animator.isActiveAndEnabled)
			{
				waitEnumerator = Wait();
				StartCoroutine(Wait());
			}
		}
	}

	public bool isDoubleConfirmed()
	{
		if (doubleTap)
		{
			if (localPersistService.GetDataIntPlayer("DoublePurchaseConfirm") != 0)
			{
				return tapCount == 2;
			}
			return true;
		}
		return true;
	}

	public virtual void ResetAnim()
	{
		ResetTapState();
		if (animator != null)
		{
			animator.Play("Normal", 0, 0f);
		}
	}

	public virtual void Disable()
	{
		AddAnimator();
		if (animator != null)
		{
			animator.Play("Disabled");
		}
	}

	private IEnumerator Wait()
	{
		yield return new WaitForSeconds(2.5f);
		if (waitEnumerator != null)
		{
			ResetTapState();
			waitEnumerator = null;
		}
	}

	protected override void Start()
	{
		base.Start();
		AddAnimator();
	}

	private void AddAnimator()
	{
		if (animator == null && !ignoreAnimator)
		{
			animator = base.gameObject.GetComponent<Animator>();
			if (!(animator == null))
			{
				animator.applyRootMotion = false;
			}
		}
	}
}
