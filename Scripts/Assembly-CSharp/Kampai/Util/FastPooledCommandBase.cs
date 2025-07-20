using strange.extensions.pool.api;

namespace Kampai.Util
{
	public class FastPooledCommandBase : IPoolable, IFastPooledCommandBase
	{
		public FastCommandPool commandPool { get; set; }

		public bool retain { get; private set; }

		public void Restore()
		{
		}

		public void Retain()
		{
			retain = true;
		}

		public void Release()
		{
			retain = false;
			if (commandPool != null)
			{
				commandPool.ReturnToPool(this);
			}
		}
	}
}
