using System;
using Elevation.Logging;
using Kampai.Util;
using Kampai.Wrappers;
using UnityEngine;

namespace Kampai.Game
{
	public class LuaKernel : IDisposable
	{
		private readonly NativeLibContext context;

		public readonly MasterLuaState L;

		private readonly IKampaiLogger _logger = LogManager.GetClassLogger("LuaKernel") as IKampaiLogger;

		private readonly SafeGCHandle luaSearcherHandle;

		private readonly SafeGCHandle cSearcherHandle;

		public LuaKernel()
		{
			try
			{
				luaSearcherHandle = LuaUtil.MakeHandle(LuaSearcher);
				cSearcherHandle = LuaUtil.MakeHandle(CSearcher);
				context = new NativeLibContext(LogMethod, ErrorMethod);
				L = new MasterLuaState();
				L.luaL_openlibs();
				SetupState(L);
			}
			catch (Exception ex)
			{
				_logger.Error(ex.ToString());
				_logger.Error(ex.Message);
				throw;
			}
		}

		private void SetupState(LuaState state)
		{
			state.lua_createtable(2, 0);
			state.lua_pushinteger(1);
			state.lua_pushlightuserdata(luaSearcherHandle);
			state.lua_pushcclosure(LuaUtil.cfunc_CallDelegate, 1);
			state.lua_settable(-3);
			state.lua_pushinteger(2);
			state.lua_pushlightuserdata(cSearcherHandle);
			state.lua_pushcclosure(LuaUtil.cfunc_CallDelegate, 1);
			state.lua_settable(-3);
			state.lua_getglobal("package");
			state.lua_pushvalue(-2);
			state.lua_setfield(-2, "searchers");
			state.lua_pop(2);
		}

		private int LuaSearcher(LuaState state)
		{
			string arg = state.lua_tostring(1);
			string text = string.Format("LUA/{0}", arg);
			TextAsset textAsset = Resources.Load<TextAsset>(text);
			if (textAsset == null)
			{
				state.lua_pushstring("Failed to load asset " + text);
				return 1;
			}
			state.luaL_loadbufferx(textAsset.text, textAsset.text.Length, text, null);
			return 1;
		}

		private int CSearcher(LuaState state)
		{
			string text = state.lua_tostring(1);
			string[] value = text.Split('.');
			string function_name = "luaopen_" + string.Join("_", value);
			KampaiNativeLib.kampai_push_cfunction_from_lib(state, text, function_name);
			return 1;
		}

		private void LogMethod(string message)
		{
			_logger.Log(KampaiLogLevel.Info, message);
		}

		private void ErrorMethod(string message)
		{
			_logger.Error(message);
		}

		protected virtual void Dispose(bool fromDispose)
		{
			if (fromDispose)
			{
				L.Dispose();
				context.Dispose();
				luaSearcherHandle.Dispose();
				cSearcherHandle.Dispose();
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~LuaKernel()
		{
			Dispose(false);
		}
	}
}
