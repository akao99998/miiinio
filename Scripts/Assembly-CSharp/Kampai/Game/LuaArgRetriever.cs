using System;
using Kampai.Wrappers;

namespace Kampai.Game
{
	internal sealed class LuaArgRetriever : IArgRetriever
	{
		private LuaState L;

		public int Length { get; private set; }

		public void Setup(LuaState l)
		{
			L = l;
			Length = l.lua_gettop();
		}

		public int GetInt(int index)
		{
			return L.lua_tointegerx(index, IntPtr.Zero);
		}

		public float GetFloat(int index)
		{
			return (float)L.lua_tonumberx(index, IntPtr.Zero);
		}

		public string GetString(int index)
		{
			return L.lua_tostring(index);
		}

		public bool GetBoolean(int index)
		{
			return L.lua_toboolean(index);
		}

		public object Get(int index, Type type)
		{
			if (type == typeof(int))
			{
				return GetInt(index);
			}
			if (type == typeof(float))
			{
				return GetFloat(index);
			}
			if (type == typeof(bool))
			{
				return GetBoolean(index);
			}
			if (type == typeof(string))
			{
				return GetString(index);
			}
			return null;
		}
	}
}
