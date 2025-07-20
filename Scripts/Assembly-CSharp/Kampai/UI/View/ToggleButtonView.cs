using Kampai.Util;
using strange.extensions.signal.impl;

namespace Kampai.UI.View
{
	public class ToggleButtonView : KampaiView
	{
		public Signal<bool> OnValueChangedSignal = new Signal<bool>();

		public bool IsOn { get; private set; }

		public virtual void OnValueChanged(bool isOff)
		{
			OnValueChangedSignal.Dispatch(isOff);
			IsOn = !isOff;
		}
	}
}
