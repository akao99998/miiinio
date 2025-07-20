using System;
using System.Runtime.InteropServices;

namespace Kampai.Wrappers
{
	public static class KampaiNativeLib
	{
		public struct DebugData
		{
			public string name;

			public int line_number;
		}

		private class NativeMethods
		{
			[DllImport("lua52")]
			public static extern IntPtr kampai_create_debug();

			[DllImport("lua52")]
			public static extern void kampai_free_debug(IntPtr debug);

			[DllImport("lua52")]
			public static extern DebugData kampai_get_debug(IntPtr L, string what, IntPtr ar);

			[DllImport("lua52")]
			public static extern int kampai_push_cfunction_from_lib(IntPtr L, string name, string function_name);
		}

		public const string dllString = "lua52";

		public static IntPtr kampai_create_debug()
		{
			return NativeMethods.kampai_create_debug();
		}

		public static void kampai_free_debug(IntPtr debug)
		{
			NativeMethods.kampai_free_debug(debug);
		}

		public static DebugData kampai_get_debug(LuaState L, string what, IntPtr ar)
		{
			return NativeMethods.kampai_get_debug(L.DangerousGetHandle(), what, ar);
		}

		public static int kampai_push_cfunction_from_lib(LuaState L, string name, string function_name)
		{
			return NativeMethods.kampai_push_cfunction_from_lib(L.DangerousGetHandle(), name, function_name);
		}
	}
}
