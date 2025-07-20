using Kampai.Game;
using Kampai.Util;
using strange.extensions.signal.impl;

namespace Kampai.Tools.AnimationToolKit
{
	public class GachaButtonView : KampaiView
	{
		private AnimationDefinition def;

		public Signal<AnimationDefinition> FireGachaSignal = new Signal<AnimationDefinition>();

		public void OnButtonPress()
		{
			FireGachaSignal.Dispatch(def);
		}

		public void SetGachaDefinition(AnimationDefinition def)
		{
			this.def = def;
		}
	}
}
