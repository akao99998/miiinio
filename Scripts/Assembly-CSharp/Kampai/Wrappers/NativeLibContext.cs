using System;
using System.Runtime.InteropServices;
using AOT;

namespace Kampai.Wrappers
{
	public class NativeLibContext : IDisposable
	{
		private static class NativeMethods
		{
			[DllImport("lua52")]
			public static extern void lua_kampai_set_log_func(IntPtr instance, NativeCallbackDelegate callback);

			[DllImport("lua52")]
			public static extern void lua_kampai_set_error_func(IntPtr instance, NativeCallbackDelegate callback);
		}

		public delegate void NativeCallbackDelegate(IntPtr instance, IntPtr str, int strLength);

		private GCHandle thisHandle;

		private Action<string> debugAction;

		private Action<string> errorAction;

		public NativeLibContext(Action<string> log_method, Action<string> error_method)
		{
			debugAction = log_method;
			errorAction = error_method;
			thisHandle = GCHandle.Alloc(this);
			IntPtr instance = GCHandle.ToIntPtr(thisHandle);
			NativeMethods.lua_kampai_set_log_func(instance, HandleDebugLog);
			NativeMethods.lua_kampai_set_error_func(instance, HandleErrorLog);
		}

		[MonoPInvokeCallback(typeof(NativeCallbackDelegate))]
		public static void HandleDebugLog(IntPtr instance, IntPtr str, int strLength)
		{
			string obj = Marshal.PtrToStringAnsi(str, strLength);
			NativeLibContext nativeLibContext = GCHandle.FromIntPtr(instance).Target as NativeLibContext;
			nativeLibContext.debugAction(obj);
		}

		[MonoPInvokeCallback(typeof(NativeCallbackDelegate))]
		public static void HandleErrorLog(IntPtr instance, IntPtr str, int strLength)
		{
			string obj = Marshal.PtrToStringAnsi(str, strLength);
			NativeLibContext nativeLibContext = GCHandle.FromIntPtr(instance).Target as NativeLibContext;
			nativeLibContext.errorAction(obj);
		}

		protected virtual void Dispose(bool fromDispose)
		{
			NativeMethods.lua_kampai_set_log_func(IntPtr.Zero, null);
			NativeMethods.lua_kampai_set_error_func(IntPtr.Zero, null);
			thisHandle.Free();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~NativeLibContext()
		{
			Dispose(false);
		}
	}
}
