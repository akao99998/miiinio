using System;
using AOT;

namespace Kampai.Wrappers
{
	public static class LuaUtil
	{
		public static SafeGCHandle MakeHandle(LuaDelegate func)
		{
			return new SafeGCHandle(func);
		}

		[MonoPInvokeCallback(typeof(LuaCFunction))]
		public static int cfunc_CallDelegate(IntPtr Lptr)
		{
			LuaState luaState = new WeakLuaState(Lptr);
			LuaDelegate luaDelegate = luaState.lua_touserdata(LuaState.lua_upvalueindex(1)).Target as LuaDelegate;
			return luaDelegate(luaState);
		}

		[MonoPInvokeCallback(typeof(LuaCFunction))]
		public static int cfunc_CallDelegateFromStackTop(IntPtr Lptr)
		{
			LuaState luaState = new WeakLuaState(Lptr);
			LuaDelegate luaDelegate = luaState.lua_touserdata(-1).Target as LuaDelegate;
			return luaDelegate(luaState);
		}
	}
}
