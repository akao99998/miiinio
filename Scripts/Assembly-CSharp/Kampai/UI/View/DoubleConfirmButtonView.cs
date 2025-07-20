using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class DoubleConfirmButtonView : ButtonView, IPointerDownHandler, IPointerUpHandler, IEventSystemHandler, IDoubleConfirmHandler
	{
		protected bool isInConfirmState;

		protected IEnumerator waitEnumerator;

		protected int tapCount;

		protected bool doubleTapConfirm = true;

		public Animator animator;

		public bool clickOnDoubleConfirm;

		[Inject]
		public ILocalPersistanceService localPersistService { get; set; }

		protected override void Start()
		{
			base.Start();
			animator = base.gameObject.GetComponent<Animator>();
			if (!(animator == null))
			{
				animator.applyRootMotion = false;
			}
		}

		public override void OnClickEvent()
		{
			if (PlaySoundOnClick)
			{
				base.playSFXSignal.Dispatch("Play_button_click_01");
			}
			updateTapCount();
			bool flag = isDoubleConfirmed();
			if (!flag)
			{
				ShowConfirmMessage();
				if (clickOnDoubleConfirm)
				{
					return;
				}
			}
			ClickedSignal.Dispatch();
			if (flag)
			{
				ResetAnim();
			}
		}

		public virtual void ResetTapState()
		{
			tapCount = 0;
			if (animator != null)
			{
				animator.SetBool("Pressed_Confirm", false);
			}
			isInConfirmState = false;
		}

		public virtual void updateTapCount()
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

		public virtual void DisableDoubleConfirm()
		{
			doubleTapConfirm = false;
		}

		public virtual void EnableDoubleConfirm()
		{
			doubleTapConfirm = true;
		}

		public virtual void ShowConfirmMessage()
		{
			bool flag = doubleTapConfirm && localPersistService.GetDataIntPlayer("DoublePurchaseConfirm") != 0;
			if (animator != null && flag)
			{
				animator.SetBool("Pressed_Confirm", flag);
				isInConfirmState = flag;
				if (waitEnumerator != null)
				{
					StopCoroutine(waitEnumerator);
				}
				waitEnumerator = Wait();
				StartCoroutine(waitEnumerator);
			}
		}

		public virtual bool isDoubleConfirmed()
		{
			if (!doubleTapConfirm)
			{
				return true;
			}
			if (localPersistService.GetDataIntPlayer("DoublePurchaseConfirm") != 0)
			{
				return tapCount == 2;
			}
			return true;
		}

		public virtual void ResetAnim()
		{
			if (animator != null)
			{
				animator.Play("Normal", 0, 0f);
			}
		}

		public virtual void Disable()
		{
			ResetTapState();
			if (animator != null)
			{
				base.gameObject.GetComponent<Button>().interactable = false;
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

		public virtual void OnPointerDown(PointerEventData eventData)
		{
			if (waitEnumerator != null)
			{
				StopCoroutine(waitEnumerator);
				waitEnumerator = null;
			}
		}

		public virtual void OnPointerUp(PointerEventData eventData)
		{
			if (isInConfirmState && waitEnumerator == null)
			{
				waitEnumerator = Wait();
				StartCoroutine(waitEnumerator);
			}
		}
	}
}
