using strange.extensions.injector.api;
using strange.extensions.signal.impl;

namespace Kampai.Util
{
	public static class FastPooledCommandBinder
	{
		public static void Bind<Sig, Cmd>(IInjectionBinder binder) where Sig : Signal where Cmd : class, IFastPooledCommand
		{
			Sig instance = binder.GetInstance<Sig>();
			FastCommandPool pool = binder.GetInstance<FastCommandPool>();
			pool.WarmupPool<Cmd>(binder);
			instance.AddListener(delegate
			{
				Cmd command = pool.GetCommand<Cmd>(binder);
				command.Execute();
				if (!command.retain)
				{
					pool.ReturnToPool(command);
				}
			});
		}
	}
	public static class FastPooledCommandBinder<T>
	{
		public static void Bind<Sig, Cmd>(IInjectionBinder binder) where Sig : Signal<T> where Cmd : class, IFastPooledCommand<T>
		{
			Sig instance = binder.GetInstance<Sig>();
			FastCommandPool pool = binder.GetInstance<FastCommandPool>();
			pool.WarmupPool<Cmd>(binder);
			instance.AddListener(delegate(T arg1)
			{
				Cmd command = pool.GetCommand<Cmd>(binder);
				command.Execute(arg1);
				if (!command.retain)
				{
					pool.ReturnToPool(command);
				}
			});
		}
	}
	public static class FastPooledCommandBinder<T1, T2>
	{
		public static void Bind<Sig, Cmd>(IInjectionBinder binder) where Sig : Signal<T1, T2> where Cmd : class, IFastPooledCommand<T1, T2>
		{
			Sig instance = binder.GetInstance<Sig>();
			FastCommandPool pool = binder.GetInstance<FastCommandPool>();
			pool.WarmupPool<Cmd>(binder);
			instance.AddListener(delegate(T1 arg1, T2 arg2)
			{
				Cmd command = pool.GetCommand<Cmd>(binder);
				command.Execute(arg1, arg2);
				if (!command.retain)
				{
					pool.ReturnToPool(command);
				}
			});
		}
	}
	public static class FastPooledCommandBinder<T1, T2, T3>
	{
		public static void Bind<Sig, Cmd>(IInjectionBinder binder) where Sig : Signal<T1, T2, T3> where Cmd : class, IFastPooledCommand<T1, T2, T3>
		{
			Sig instance = binder.GetInstance<Sig>();
			FastCommandPool pool = binder.GetInstance<FastCommandPool>();
			pool.WarmupPool<Cmd>(binder);
			instance.AddListener(delegate(T1 arg1, T2 arg2, T3 arg3)
			{
				Cmd command = pool.GetCommand<Cmd>(binder);
				command.Execute(arg1, arg2, arg3);
				if (!command.retain)
				{
					pool.ReturnToPool(command);
				}
			});
		}
	}
	public static class FastPooledCommandBinder<T1, T2, T3, T4>
	{
		public static void Bind<Sig, Cmd>(IInjectionBinder binder) where Sig : Signal<T1, T2, T3, T4> where Cmd : class, IFastPooledCommand<T1, T2, T3, T4>
		{
			Sig instance = binder.GetInstance<Sig>();
			FastCommandPool pool = binder.GetInstance<FastCommandPool>();
			pool.WarmupPool<Cmd>(binder);
			instance.AddListener(delegate(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
			{
				Cmd command = pool.GetCommand<Cmd>(binder);
				command.Execute(arg1, arg2, arg3, arg4);
				if (!command.retain)
				{
					pool.ReturnToPool(command);
				}
			});
		}
	}
}
