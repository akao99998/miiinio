using Kampai.Util;
using UnityEngine.UI;

namespace Kampai.UI.View
{
	public class CurrencyWarningDialogView : KampaiView
	{
		public Text CurrencyNeededLabel;

		public Text CurrencyNeededButtonLabel;

		public ButtonView CancelButton;

		public DoubleConfirmButtonView PurchaseButton;

		internal void SetCurrencyNeeded(int cost, int amountNeeded)
		{
			CurrencyNeededLabel.text = UIUtils.FormatLargeNumber(amountNeeded);
			CurrencyNeededButtonLabel.text = UIUtils.FormatLargeNumber(cost);
		}
	}
}
