using Kampai.Main;
using Kampai.Util;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class RushButtonView : DoubleConfirmButtonView
	{
		public QuantityItem Item;

		public Signal<int, QuantityItem, bool> RushButtonClickedSignal = new Signal<int, QuantityItem, bool>();

		[Inject]
		public PlayGlobalSoundFXSignal soundFXSignal { get; set; }

		public int RushCost { get; set; }

		public bool SkipDoubleConfirm { get; set; }

		public override void OnClickEvent()
		{
			updateTapCount();
			if (!isDoubleConfirmed() && !SkipDoubleConfirm)
			{
				soundFXSignal.Dispatch("Play_button_click_01");
				ShowConfirmMessage();
			}
			ClickedSignal.Dispatch();
			RushButtonClickedSignal.Dispatch(RushCost, Item, isDoubleConfirmed());
		}
	}
}
