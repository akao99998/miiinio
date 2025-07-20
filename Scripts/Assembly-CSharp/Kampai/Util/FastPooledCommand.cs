using strange.extensions.pool.api;

namespace Kampai.Util
{
	public abstract class FastPooledCommand : FastPooledCommandBase
	{
		public abstract void Execute();
	}
	public abstract class FastPooledCommand<T> : FastPooledCommandBase, IPoolable, IFastPooledCommand<T>, IFastPooledCommandBase
	{
		public abstract void Execute(T arg1);
	}
	public abstract class FastPooledCommand<T1, T2> : FastPooledCommandBase, IPoolable, IFastPooledCommand<T1, T2>, IFastPooledCommandBase
	{
		public abstract void Execute(T1 arg1, T2 arg2);
	}
	public abstract class FastPooledCommand<T1, T2, T3> : FastPooledCommandBase, IPoolable, IFastPooledCommand<T1, T2, T3>, IFastPooledCommandBase
	{
		public abstract void Execute(T1 arg1, T2 arg2, T3 arg3);
	}
	public abstract class FastPooledCommand<T1, T2, T3, T4> : FastPooledCommandBase, IPoolable, IFastPooledCommand<T1, T2, T3, T4>, IFastPooledCommandBase
	{
		public abstract void Execute(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
	}
}
