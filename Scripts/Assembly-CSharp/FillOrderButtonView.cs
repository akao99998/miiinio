using Kampai.Main;
using Kampai.UI.View;
using UnityEngine;
using UnityEngine.UI;

public class FillOrderButtonView : DoubleConfirmButtonView
{
	public RectTransform FillOrderText;

	public KampaiImage FillOrderRushIcon;

	public Text FillOrderRushCost;

	internal OrderBoardButtonState previousState;

	private float fillOrderTextAnchorMaxXForRushing;

	private int currentRushCost;

	[Inject]
	public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

	public override void OnClickEvent()
	{
		updateTapCount();
		if (!isDoubleConfirmed())
		{
			soundFXSignal.Dispatch("Play_button_click_01");
			ShowConfirmMessage();
		}
		ClickedSignal.Dispatch();
	}

	public void Init()
	{
		fillOrderTextAnchorMaxXForRushing = FillOrderRushIcon.rectTransform.anchorMin.x;
	}

	public override void updateTapCount()
	{
		if (previousState == OrderBoardButtonState.Rush)
		{
			base.updateTapCount();
		}
	}

	public override bool isDoubleConfirmed()
	{
		if (previousState == OrderBoardButtonState.Rush)
		{
			if (base.localPersistService.GetDataIntPlayer("DoublePurchaseConfirm") != 0)
			{
				return tapCount == 2;
			}
			return true;
		}
		return true;
	}

	internal OrderBoardButtonState GetLastFillOrderButtonState()
	{
		return previousState;
	}

	internal int GetLastRushCost()
	{
		return currentRushCost;
	}

	internal void SetFillOrderButtonState(OrderBoardButtonState state, int rushCost = -1)
	{
		if (previousState == state && currentRushCost == rushCost)
		{
			if (state == OrderBoardButtonState.Rush)
			{
				ResetTapState();
			}
			return;
		}
		if (state == OrderBoardButtonState.Enable)
		{
			state = OrderBoardButtonState.Rush;
			rushCost = currentRushCost;
		}
		if (state == OrderBoardButtonState.Rush && rushCost == 0)
		{
			state = OrderBoardButtonState.MeetRequirement;
		}
		if (previousState == OrderBoardButtonState.Hide)
		{
			base.gameObject.SetActive(true);
		}
		if (previousState == OrderBoardButtonState.Disable)
		{
			SetupFillOrderButton(true);
		}
		DisableDoubleConfirm();
		switch (state)
		{
		case OrderBoardButtonState.Disable:
			animator.Play("Disabled");
			SetupFillOrderButton(false);
			FillOrderRushIcon.enabled = false;
			FillOrderRushCost.enabled = false;
			FillOrderText.anchorMax = Vector2.one;
			break;
		case OrderBoardButtonState.Hide:
			base.gameObject.SetActive(false);
			break;
		case OrderBoardButtonState.MeetRequirement:
			currentRushCost = 0;
			FillOrderText.anchorMax = Vector2.one;
			FillOrderRushIcon.enabled = false;
			FillOrderRushCost.enabled = false;
			animator.Play("MeetRequirement");
			SetupFillOrderButton(true);
			break;
		case OrderBoardButtonState.Rush:
			ResetTapState();
			EnableDoubleConfirm();
			FillOrderText.anchorMax = new Vector2(fillOrderTextAnchorMaxXForRushing, 1f);
			FillOrderRushIcon.enabled = true;
			FillOrderRushCost.enabled = true;
			FillOrderRushCost.text = rushCost.ToString();
			animator.Play("Rush");
			break;
		}
		previousState = state;
		if (rushCost != -1)
		{
			currentRushCost = rushCost;
		}
	}

	private void SetupFillOrderButton(bool active)
	{
		Button component = GetComponent<Button>();
		component.interactable = active;
	}
}
