using strange.extensions.pool.api;

namespace Kampai.Util
{
	public interface IFastPooledCommand : IPoolable, IFastPooledCommandBase
	{
		void Execute();
	}
	public interface IFastPooledCommand<T> : IPoolable, IFastPooledCommandBase
	{
		void Execute(T arg1);
	}
	public interface IFastPooledCommand<T1, T2> : IPoolable, IFastPooledCommandBase
	{
		void Execute(T1 arg1, T2 arg2);
	}
	public interface IFastPooledCommand<T1, T2, T3> : IPoolable, IFastPooledCommandBase
	{
		void Execute(T1 arg1, T2 arg2, T3 arg3);
	}
	public interface IFastPooledCommand<T1, T2, T3, T4> : IPoolable, IFastPooledCommandBase
	{
		void Execute(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
	}
}
