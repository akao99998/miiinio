using System;
using System.Collections.Generic;
using Kampai.Util;
using Kampai.Wrappers;

namespace Kampai.Game
{
	internal sealed class LuaReturnValueContainer : ReturnValueContainer
	{
		private Dictionary<string, LuaReturnValueContainer> keyValues = new Dictionary<string, LuaReturnValueContainer>();

		private List<LuaReturnValueContainer> arrayIndices = new List<LuaReturnValueContainer>();

		public LuaReturnValueContainer(IKampaiLogger logger)
			: base(logger)
		{
		}

		public override void Reset()
		{
			base.Reset();
			keyValues.Clear();
		}

		public int PushToStack(LuaState L)
		{
			switch (base.type)
			{
			case ValueType.Number:
				L.lua_pushnumber(numberValue);
				return 1;
			case ValueType.String:
				L.lua_pushstring(stringValue);
				return 1;
			case ValueType.Boolean:
				L.lua_pushboolean(boolValue);
				return 1;
			case ValueType.Nil:
				L.lua_pushnil();
				return 1;
			case ValueType.Dictionary:
				return PushDictionary(L);
			case ValueType.Array:
				return PushArray(L);
			case ValueType.Void:
				return 0;
			default:
				logger.Error("LuaReturnValueContainer: Don't know how to push {0} onto stack.", Enum.GetName(typeof(ValueType), base.type));
				return 0;
			}
		}

		public int PushArrayValuesToStack(LuaState L)
		{
			int count = arrayIndices.Count;
			for (int i = 0; i < count; i++)
			{
				if (arrayIndices[i].PushToStack(L) == 0)
				{
					L.lua_pushnil();
				}
			}
			return count;
		}

		protected override ReturnValueContainer GetContainerForKey(string key)
		{
			LuaReturnValueContainer value;
			if (keyValues.TryGetValue(key, out value))
			{
				return value;
			}
			value = new LuaReturnValueContainer(logger);
			keyValues[key] = value;
			return value;
		}

		protected override ReturnValueContainer GetContainerForNextIndex()
		{
			LuaReturnValueContainer luaReturnValueContainer = new LuaReturnValueContainer(logger);
			arrayIndices.Add(luaReturnValueContainer);
			return luaReturnValueContainer;
		}

		protected override void ClearKeys()
		{
			keyValues.Clear();
		}

		protected override void ClearArray()
		{
			arrayIndices.Clear();
		}

		private int PushDictionary(LuaState L)
		{
			L.lua_createtable(0, keyValues.Count);
			foreach (KeyValuePair<string, LuaReturnValueContainer> keyValue in keyValues)
			{
				if (keyValue.Value.PushToStack(L) == 0)
				{
					L.lua_pushnil();
				}
				L.lua_setfield(-2, keyValue.Key);
			}
			return 1;
		}

		private int PushArray(LuaState L)
		{
			int count = arrayIndices.Count;
			L.lua_createtable(count, 0);
			for (int i = 0; i < count; i++)
			{
				if (arrayIndices[i].PushToStack(L) == 0)
				{
					L.lua_pushnil();
				}
				L.lua_rawseti(-2, i + 1);
			}
			return 1;
		}
	}
}
