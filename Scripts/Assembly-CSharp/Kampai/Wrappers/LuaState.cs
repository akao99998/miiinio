using System;
using System.Runtime.InteropServices;

namespace Kampai.Wrappers
{
	public abstract class LuaState : SafeHandle
	{
		private static class NativeMethods
		{
			[DllImport("lua52")]
			public static extern int lua_gettop(IntPtr L);

			[DllImport("lua52")]
			public static extern int lua_pcallk(IntPtr L, int nargs, int nresults, int errfunc, int ctx, LuaCFunction k);

			[DllImport("lua52")]
			public static extern void lua_settop(IntPtr L, int n);

			[DllImport("lua52")]
			public static extern void lua_createtable(IntPtr L, int narr, int nrec);

			[DllImport("lua52")]
			public static extern int luaL_loadstring(IntPtr L, string s);

			[DllImport("lua52")]
			public static extern int luaL_loadbufferx(IntPtr L, string buff, int sz, string name, string mode);

			[DllImport("lua52")]
			public static extern void luaL_openlibs(IntPtr L);

			[DllImport("lua52")]
			public static extern int luaL_ref(IntPtr L, int t);

			[DllImport("lua52")]
			public static extern void luaL_unref(IntPtr L, int t, int reference);

			[DllImport("lua52")]
			public static extern int lua_pushvalue(IntPtr L, int idx);

			[DllImport("lua52")]
			public static extern LuaType lua_type(IntPtr L, int idx);

			[DllImport("lua52")]
			public static extern double lua_tonumberx(IntPtr L, int idx, IntPtr isnum);

			[DllImport("lua52")]
			public static extern int lua_tointegerx(IntPtr L, int idx, IntPtr isnum);

			[DllImport("lua52")]
			public static extern IntPtr lua_tolstring(IntPtr L, int idx, IntPtr len);

			[DllImport("lua52")]
			public static extern IntPtr lua_touserdata(IntPtr L, int idx);

			[DllImport("lua52")]
			public static extern bool lua_toboolean(IntPtr L, int idx);

			[DllImport("lua52")]
			public static extern void lua_pushnil(IntPtr L);

			[DllImport("lua52")]
			public static extern void lua_pushnumber(IntPtr L, double n);

			[DllImport("lua52")]
			public static extern void lua_pushinteger(IntPtr L, int n);

			[DllImport("lua52")]
			public static extern int lua_pushstring(IntPtr L, string s);

			[DllImport("lua52")]
			public static extern void lua_pushcclosure(IntPtr L, LuaCFunction fn, int n);

			[DllImport("lua52")]
			public static extern int lua_pushboolean(IntPtr L, bool b);

			[DllImport("lua52")]
			public static extern void lua_pushlightuserdata(IntPtr L, IntPtr p);

			[DllImport("lua52")]
			public static extern int lua_getglobal(IntPtr L, string var);

			[DllImport("lua52")]
			public static extern int lua_getfield(IntPtr L, int idx, string k);

			[DllImport("lua52")]
			public static extern void lua_rawget(IntPtr L, int idx);

			[DllImport("lua52")]
			public static extern void lua_rawgeti(IntPtr L, int idx, int n);

			[DllImport("lua52")]
			public static extern void lua_setglobal(IntPtr L, string var);

			[DllImport("lua52")]
			public static extern void lua_setfield(IntPtr L, int idx, string k);

			[DllImport("lua52")]
			public static extern void lua_rawset(IntPtr L, int idx);

			[DllImport("lua52")]
			public static extern void lua_rawseti(IntPtr L, int idx, int n);

			[DllImport("lua52")]
			public static extern void lua_setmetatable(IntPtr L, int objindex);

			[DllImport("lua52")]
			public static extern void lua_settable(IntPtr L, int index);

			[DllImport("lua52")]
			public static extern int lua_yieldk(IntPtr L, int nresults, int ctx, LuaCFunction k);

			[DllImport("lua52")]
			public static extern ThreadStatus lua_resume(IntPtr L, IntPtr from, int narg);

			[DllImport("lua52")]
			public static extern int lua_getstack(IntPtr L, int level, IntPtr ar);

			[DllImport("lua52")]
			public static extern int lua_getinfo(IntPtr L, string what, IntPtr ar);

			[DllImport("lua52")]
			public static extern void lua_setupvalue(IntPtr L, int funcindex, int n);

			[DllImport("lua52")]
			public static extern int lua_error(IntPtr L);
		}

		protected const string dllString = "lua52";

		protected const int LUA_MULTRET = -1;

		protected const int LUAI_MAXSTACK = 1000000;

		protected const int LUAI_FIRSTPSEUDOIDX = -1001000;

		public const int LUA_REGISTRYINDEX = -1001000;

		protected const int LUA_REFNIL = -1;

		public override bool IsInvalid
		{
			get
			{
				return handle == IntPtr.Zero;
			}
		}

		public LuaState(bool ownsHandle)
			: base(IntPtr.Zero, ownsHandle)
		{
		}

		public int lua_pcall(int nargs, int nresults, int errfunc)
		{
			return NativeMethods.lua_pcallk(handle, nargs, nresults, errfunc, 0, null);
		}

		public void lua_pop(int n)
		{
			NativeMethods.lua_settop(handle, -n - 1);
		}

		public int luaL_dostring(string s)
		{
			int num = NativeMethods.luaL_loadstring(handle, s);
			if (num > 0)
			{
				return num;
			}
			return lua_pcall(0, -1, 0);
		}

		public string lua_tostring(int idx)
		{
			return Marshal.PtrToStringAnsi(NativeMethods.lua_tolstring(handle, idx, IntPtr.Zero));
		}

		public static int lua_upvalueindex(int i)
		{
			return -1001000 - i;
		}

		public int lua_gettop()
		{
			return NativeMethods.lua_gettop(handle);
		}

		public int lua_pcallk(int nargs, int nresults, int errfunc, int ctx, LuaCFunction k)
		{
			return NativeMethods.lua_pcallk(handle, nargs, nresults, errfunc, ctx, k);
		}

		public void lua_settop(int n)
		{
			NativeMethods.lua_settop(handle, n);
		}

		public void lua_createtable(int narr, int nrec)
		{
			NativeMethods.lua_createtable(handle, narr, nrec);
		}

		public int luaL_loadstring(string s)
		{
			return NativeMethods.luaL_loadstring(handle, s);
		}

		public int luaL_loadbufferx(string buff, int sz, string name, string mode)
		{
			return NativeMethods.luaL_loadbufferx(handle, buff, sz, name, mode);
		}

		public void luaL_openlibs()
		{
			NativeMethods.luaL_openlibs(handle);
		}

		public int luaL_ref(int t)
		{
			return NativeMethods.luaL_ref(handle, t);
		}

		public void luaL_unref(int t, int reference)
		{
			NativeMethods.luaL_unref(handle, t, reference);
		}

		public int lua_pushvalue(int idx)
		{
			return NativeMethods.lua_pushvalue(handle, idx);
		}

		public LuaType lua_type(int idx)
		{
			return NativeMethods.lua_type(handle, idx);
		}

		public double lua_tonumberx(int idx, IntPtr isnum)
		{
			return NativeMethods.lua_tonumberx(handle, idx, isnum);
		}

		public int lua_tointegerx(int idx, IntPtr isnum)
		{
			return NativeMethods.lua_tointegerx(handle, idx, isnum);
		}

		public IntPtr lua_tolstring(int idx, IntPtr len)
		{
			return NativeMethods.lua_tolstring(handle, idx, len);
		}

		public WeakGCHandle lua_touserdata(int idx)
		{
			return new WeakGCHandle(NativeMethods.lua_touserdata(handle, idx));
		}

		public bool lua_toboolean(int idx)
		{
			return NativeMethods.lua_toboolean(handle, idx);
		}

		public void lua_pushnil()
		{
			NativeMethods.lua_pushnil(handle);
		}

		public void lua_pushnumber(double n)
		{
			NativeMethods.lua_pushnumber(handle, n);
		}

		public void lua_pushinteger(int n)
		{
			NativeMethods.lua_pushinteger(handle, n);
		}

		public int lua_pushstring(string s)
		{
			return NativeMethods.lua_pushstring(handle, s);
		}

		public void lua_pushcclosure(LuaCFunction fn, int n)
		{
			NativeMethods.lua_pushcclosure(handle, fn, n);
		}

		public int lua_pushboolean(bool b)
		{
			return NativeMethods.lua_pushboolean(handle, b);
		}

		public void lua_pushlightuserdata(SafeGCHandle p)
		{
			NativeMethods.lua_pushlightuserdata(handle, p.DangerousGetHandle());
		}

		public int lua_getglobal(string var)
		{
			return NativeMethods.lua_getglobal(handle, var);
		}

		public int lua_getfield(int idx, string k)
		{
			return NativeMethods.lua_getfield(handle, idx, k);
		}

		public void lua_rawget(int idx)
		{
			NativeMethods.lua_rawget(handle, idx);
		}

		public void lua_rawgeti(int idx, int n)
		{
			NativeMethods.lua_rawgeti(handle, idx, n);
		}

		public void lua_setglobal(string var)
		{
			NativeMethods.lua_setglobal(handle, var);
		}

		public void lua_setfield(int idx, string k)
		{
			NativeMethods.lua_setfield(handle, idx, k);
		}

		public void lua_rawset(int idx)
		{
			NativeMethods.lua_rawset(handle, idx);
		}

		public void lua_rawseti(int idx, int n)
		{
			NativeMethods.lua_rawseti(handle, idx, n);
		}

		public void lua_setmetatable(int objindex)
		{
			NativeMethods.lua_setmetatable(handle, objindex);
		}

		public void lua_settable(int index)
		{
			NativeMethods.lua_settable(handle, index);
		}

		public int lua_yieldk(int nresults, int ctx, LuaCFunction k)
		{
			return NativeMethods.lua_yieldk(handle, nresults, ctx, k);
		}

		public ThreadStatus lua_resume(LuaState from, int narg)
		{
			return NativeMethods.lua_resume(handle, from.DangerousGetHandle(), narg);
		}

		public int lua_getstack(int level, IntPtr ar)
		{
			return NativeMethods.lua_getstack(handle, level, ar);
		}

		public int lua_getinfo(string what, IntPtr ar)
		{
			return NativeMethods.lua_getinfo(handle, what, ar);
		}

		public void lua_setupvalue(int funcindex, int n)
		{
			NativeMethods.lua_setupvalue(handle, funcindex, n);
		}

		public int lua_error()
		{
			return NativeMethods.lua_error(handle);
		}
	}
}
