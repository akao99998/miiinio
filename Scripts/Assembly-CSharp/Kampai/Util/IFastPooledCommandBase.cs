using strange.extensions.pool.api;

namespace Kampai.Util
{
	public interface IFastPooledCommandBase : IPoolable
	{
		FastCommandPool commandPool { get; set; }
	}
}
