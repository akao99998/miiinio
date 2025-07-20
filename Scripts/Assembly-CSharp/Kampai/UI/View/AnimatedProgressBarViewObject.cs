using UnityEngine;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class AnimatedProgressBarViewObject : MonoBehaviour
	{
		public float FillDiffDuration = 0.3f;

		public float FillMainDuration = 1.2f;

		public float FillMainWaitTime = 0.6f;

		public Image FillImageMain;

		public Image FillImageDifference;

		public Text ValueText;

		private int targetValue;

		private float previousDisplayedValue;

		private float displayedValue;

		private float displayedDiffValue;

		private int maxValue;

		private float fillTimePassed;

		private bool isAnimatingFill;

		public void Init(int currentValue, int maxValue)
		{
			targetValue = currentValue;
			displayedValue = currentValue;
			displayedDiffValue = currentValue;
			this.maxValue = maxValue;
			RefreshView();
		}

		public void AnimateToValue(int newValue)
		{
			fillTimePassed = 0f;
			isAnimatingFill = true;
			previousDisplayedValue = displayedValue;
			targetValue = newValue;
			RefreshView();
		}

		private void RefreshView()
		{
			FillImageMain.fillAmount = displayedValue / (float)maxValue;
			FillImageDifference.fillAmount = displayedDiffValue / (float)maxValue;
			ValueText.text = UIUtils.FormatLargeNumber((int)Mathf.Round(displayedValue)) + " / " + UIUtils.FormatLargeNumber(maxValue);
		}

		private void Update()
		{
			if (isAnimatingFill)
			{
				fillTimePassed += Time.deltaTime;
				float num = Mathf.Min(1f, fillTimePassed / FillDiffDuration);
				float b = (fillTimePassed - FillMainWaitTime) / FillMainDuration;
				b = Mathf.Max(0f, Mathf.Min(1f, b));
				if (b < 1f)
				{
					float num2 = (float)targetValue - previousDisplayedValue;
					displayedValue = previousDisplayedValue + b * num2;
					displayedDiffValue = previousDisplayedValue + num * num2;
				}
				else
				{
					displayedValue = targetValue;
					displayedDiffValue = targetValue;
					isAnimatingFill = false;
				}
				RefreshView();
			}
		}
	}
}
