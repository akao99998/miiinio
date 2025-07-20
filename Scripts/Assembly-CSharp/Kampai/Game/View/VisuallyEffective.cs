using Kampai.Util;

namespace Kampai.Game.View
{
	public interface VisuallyEffective
	{
		void TrackVFX(VFXScript vfxScript);

		void UntrackVFX();

		void AnimVFX(string eventName);

		void SetVFXState(string name, string desiredState = null);

		void UpdateVFX();
	}
}
