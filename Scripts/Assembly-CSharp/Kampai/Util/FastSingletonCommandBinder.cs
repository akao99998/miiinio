using strange.extensions.injector.api;
using strange.extensions.signal.impl;

namespace Kampai.Util
{
	public static class FastSingletonCommandBinder
	{
		public static void Bind<Sig, Cmd>(IInjectionBinder binder) where Sig : Signal where Cmd : class, IFastCommand
		{
			Sig instance = binder.GetInstance<Sig>();
			Cmd command = (Cmd)null;
			instance.AddListener(delegate
			{
				if (command == null)
				{
					command = binder.GetInstance<Cmd>();
				}
				command.Execute();
			});
		}
	}
	public static class FastSingletonCommandBinder<T>
	{
		public static void Bind<Sig, Cmd>(IInjectionBinder binder) where Sig : Signal<T> where Cmd : class, IFastCommand<T>
		{
			Sig instance = binder.GetInstance<Sig>();
			Cmd command = (Cmd)null;
			instance.AddListener(delegate(T arg1)
			{
				if (command == null)
				{
					command = binder.GetInstance<Cmd>();
				}
				command.Execute(arg1);
			});
		}
	}
	public static class FastSingletonCommandBinder<T1, T2>
	{
		public static void Bind<Sig, Cmd>(IInjectionBinder binder) where Sig : Signal<T1, T2> where Cmd : class, IFastCommand<T1, T2>
		{
			Sig instance = binder.GetInstance<Sig>();
			Cmd command = (Cmd)null;
			instance.AddListener(delegate(T1 arg1, T2 arg2)
			{
				if (command == null)
				{
					command = binder.GetInstance<Cmd>();
				}
				command.Execute(arg1, arg2);
			});
		}
	}
	public static class FastSingletonCommandBinder<T1, T2, T3>
	{
		public static void Bind<Sig, Cmd>(IInjectionBinder binder) where Sig : Signal<T1, T2, T3> where Cmd : class, IFastCommand<T1, T2, T3>
		{
			Sig instance = binder.GetInstance<Sig>();
			Cmd command = (Cmd)null;
			instance.AddListener(delegate(T1 arg1, T2 arg2, T3 arg3)
			{
				if (command == null)
				{
					command = binder.GetInstance<Cmd>();
				}
				command.Execute(arg1, arg2, arg3);
			});
		}
	}
	public static class FastSingletonCommandBinder<T1, T2, T3, T4>
	{
		public static void Bind<Sig, Cmd>(IInjectionBinder binder) where Sig : Signal<T1, T2, T3, T4> where Cmd : class, IFastCommand<T1, T2, T3, T4>
		{
			Sig instance = binder.GetInstance<Sig>();
			Cmd command = (Cmd)null;
			instance.AddListener(delegate(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
			{
				if (command == null)
				{
					command = binder.GetInstance<Cmd>();
				}
				command.Execute(arg1, arg2, arg3, arg4);
			});
		}
	}
}
