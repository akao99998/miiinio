using Kampai.Util;
using strange.extensions.signal.impl;

namespace Kampai.Tools.AnimationToolKit
{
	public class AnimationToolKitButtonView : KampaiView
	{
		public AnimationToolKitButtonType ButtonType;

		public Signal<AnimationToolKitButtonType> ButtonPressSignal = new Signal<AnimationToolKitButtonType>();

		public void OnButtonPress()
		{
			ButtonPressSignal.Dispatch(ButtonType);
		}
	}
}
