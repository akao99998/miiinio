using strange.extensions.context.api;

namespace Kampai.Game.Trigger
{
	public interface IIsTriggerable
	{
		bool IsTriggered(ICrossContextCapable gameContext);
	}
}
