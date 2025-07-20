using System;
using System.Runtime.InteropServices;

namespace Kampai.Wrappers
{
	public class LuaThreadState : LuaState
	{
		private static class NativeMethods
		{
			[DllImport("lua52")]
			public static extern IntPtr lua_newthread(IntPtr L);
		}

		private int threadReference = -1;

		private readonly LuaState strongState;

		public override bool IsInvalid
		{
			get
			{
				return strongState.IsInvalid || handle == IntPtr.Zero;
			}
		}

		public LuaThreadState(LuaState L)
			: base(true)
		{
			strongState = L;
			handle = NativeMethods.lua_newthread(L.DangerousGetHandle());
			threadReference = L.luaL_ref(-1001000);
		}

		protected override bool ReleaseHandle()
		{
			if (strongState.IsInvalid)
			{
				return true;
			}
			strongState.luaL_unref(-1001000, threadReference);
			threadReference = -1;
			handle = IntPtr.Zero;
			return true;
		}
	}
}
