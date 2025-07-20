using System;
using System.Runtime.InteropServices;

namespace Kampai.Wrappers
{
	public class MasterLuaState : LuaState
	{
		private static class NativeMethods
		{
			[DllImport("lua52")]
			public static extern IntPtr luaL_newstate();

			[DllImport("lua52")]
			public static extern void lua_close(IntPtr L);
		}

		public MasterLuaState()
			: base(true)
		{
			handle = NativeMethods.luaL_newstate();
		}

		protected override bool ReleaseHandle()
		{
			NativeMethods.lua_close(handle);
			handle = IntPtr.Zero;
			return true;
		}
	}
}
