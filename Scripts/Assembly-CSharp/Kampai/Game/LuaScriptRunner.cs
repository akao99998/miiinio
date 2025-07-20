using System;
using System.Text;
using Elevation.Logging;
using Kampai.Util;
using Kampai.Wrappers;
using UnityEngine;

namespace Kampai.Game
{
	public class LuaScriptRunner : IDisposable, IQuestScriptRunner
	{
		private enum PauseResumeState
		{
			RUNNING = 0,
			WANT_TO_PAUSE = 1,
			PAUSED = 2
		}

		public readonly IKampaiLogger logger = LogManager.GetClassLogger("LuaScriptRunner") as IKampaiLogger;

		private LuaState masterState;

		private LuaState threadState;

		private readonly LuaArgRetriever argRetriever = new LuaArgRetriever();

		private readonly LuaReturnValueContainer returnContainer;

		private int envTableRef;

		private int qsTableRef;

		private int qsutilTableRef;

		private QuestScriptInstance questInstance;

		private string fileName;

		private bool canContinue;

		private PauseResumeState pauseResumeState;

		private LuaReturnValueContainer _invokationValues;

		private SafeGCHandle EnvIndexHandle;

		private SafeGCHandle EnvNewIndexHandle;

		private SafeGCHandle QSIndexHandle;

		private SafeGCHandle QSNewIndexHandle;

		private SafeGCHandle InvokeMethodFromLuaHandle;

		private SafeGCHandle ContinuationHandle;

		private string startMethodName;

		private StringBuilder errorMessageBuilder = new StringBuilder();

		private bool hasRanMethod;

		private bool _isDisposed;

		[Inject]
		public QuestScriptController controller { get; set; }

		[Inject]
		public QuestScriptKernel qsKernel { get; set; }

		public QuestRunnerLanguage Lang
		{
			get
			{
				return QuestRunnerLanguage.Lua;
			}
		}

		public Action<QuestScriptInstance> OnQuestScriptComplete { get; set; }

		public ReturnValueContainer InvokationValues
		{
			get
			{
				return _invokationValues;
			}
		}

		public LuaScriptRunner(LuaKernel kernel)
		{
			masterState = kernel.L;
			returnContainer = new LuaReturnValueContainer(logger);
			_invokationValues = new LuaReturnValueContainer(logger);
			EnvIndexHandle = LuaUtil.MakeHandle(EnvIndex);
			EnvNewIndexHandle = LuaUtil.MakeHandle(EnvNewIndex);
			QSIndexHandle = LuaUtil.MakeHandle(QSIndex);
			QSNewIndexHandle = LuaUtil.MakeHandle(QSNewIndex);
			InvokeMethodFromLuaHandle = LuaUtil.MakeHandle(InvokeMethodFromLua);
			ContinuationHandle = LuaUtil.MakeHandle(continuation);
		}

		private void CreateLuaThread()
		{
			LuaThreadState luaThreadState = (LuaThreadState)(threadState = new LuaThreadState(masterState));
			luaThreadState.lua_createtable(0, 0);
			luaThreadState.lua_createtable(0, 4);
			luaThreadState.lua_pushvalue(-1);
			int n = luaThreadState.luaL_ref(-1001000);
			luaThreadState.lua_pushlightuserdata(EnvIndexHandle);
			luaThreadState.lua_pushcclosure(LuaUtil.cfunc_CallDelegate, 1);
			luaThreadState.lua_setfield(-2, "__index");
			luaThreadState.lua_pushlightuserdata(EnvNewIndexHandle);
			luaThreadState.lua_pushcclosure(LuaUtil.cfunc_CallDelegate, 1);
			luaThreadState.lua_setfield(-2, "__newindex");
			luaThreadState.lua_setmetatable(-2);
			envTableRef = luaThreadState.luaL_ref(-1001000);
			luaThreadState.lua_createtable(0, 0);
			luaThreadState.lua_createtable(0, 4);
			luaThreadState.lua_pushlightuserdata(QSIndexHandle);
			luaThreadState.lua_pushcclosure(LuaUtil.cfunc_CallDelegate, 1);
			luaThreadState.lua_setfield(-2, "__index");
			luaThreadState.lua_pushlightuserdata(QSNewIndexHandle);
			luaThreadState.lua_pushcclosure(LuaUtil.cfunc_CallDelegate, 1);
			luaThreadState.lua_setfield(-2, "__newindex");
			luaThreadState.lua_setmetatable(-2);
			qsTableRef = luaThreadState.luaL_ref(-1001000);
			luaThreadState.lua_createtable(0, 0);
			luaThreadState.lua_pushvalue(-1);
			luaThreadState.lua_rawgeti(-1001000, n);
			luaThreadState.lua_setmetatable(-2);
			string text = Resources.Load<TextAsset>("LUA/Utilities").text;
			if (luaThreadState.luaL_loadbufferx(text, text.Length, "LUA/Utilities.txt", null) > 0)
			{
				LogLuaRuntimeError();
				luaThreadState.lua_pushnil();
			}
			else
			{
				luaThreadState.lua_pushvalue(-2);
				luaThreadState.lua_setupvalue(-2, 1);
				if (luaThreadState.lua_pcall(0, 0, 0) > 0)
				{
					LogLuaRuntimeError();
				}
			}
			qsutilTableRef = luaThreadState.luaL_ref(-1001000);
		}

		private int EnvIndex(LuaState L)
		{
			string text = L.lua_tostring(2);
			int num = 0;
			switch (text)
			{
			default:
			{
				int num2;
				if (num2 == 1)
				{
					num = qsutilTableRef;
					break;
				}
				L.lua_pushvalue(2);
				L.lua_rawget(1);
				if (L.lua_type(-1) == LuaType.LUA_TNIL)
				{
					L.lua_pop(1);
					L.lua_getglobal(text);
				}
				return 1;
			}
			case "qs":
				num = qsTableRef;
				break;
			}
			L.lua_rawgeti(-1001000, num);
			return 1;
		}

		private static int EnvNewIndex(LuaState L)
		{
			string text = L.lua_tostring(2);
			if (text == "qs")
			{
				L.lua_pushstring("Please don't attempt to set the qs field. Thanks!");
				return L.lua_error();
			}
			L.lua_pushvalue(2);
			L.lua_pushvalue(3);
			L.lua_rawset(1);
			return 0;
		}

		private int QSIndex(LuaState L)
		{
			string text = L.lua_tostring(2);
			if (!qsKernel.HasApiFunction(text))
			{
				L.lua_pushvalue(2);
				L.lua_rawget(1);
				return 1;
			}
			L.lua_pushlightuserdata(InvokeMethodFromLuaHandle);
			L.lua_pushstring(text);
			L.lua_pushcclosure(LuaUtil.cfunc_CallDelegate, 2);
			return 1;
		}

		private int QSNewIndex(LuaState L)
		{
			string text = L.lua_tostring(2);
			if (!qsKernel.HasApiFunction(text))
			{
				L.lua_pushvalue(2);
				L.lua_pushvalue(3);
				L.lua_rawset(1);
				return 0;
			}
			L.lua_pushstring(string.Format("Hey! You can't overwrite the C# bound method {0}! Knock it off!", text));
			return L.lua_error();
		}

		private int InvokeMethodFromLua(LuaState L)
		{
			string text = L.lua_tostring(LuaState.lua_upvalueindex(2));
			Func<QuestScriptController, IArgRetriever, ReturnValueContainer, bool> apiFunction = qsKernel.GetApiFunction(text);
			if (apiFunction == null)
			{
				L.lua_pushstring(string.Format("Woah! The method {0} was unbound from the C# side!", text));
				return L.lua_error();
			}
			argRetriever.Setup(L);
			returnContainer.Reset();
			if (apiFunction(controller, argRetriever, returnContainer))
			{
				return returnContainer.PushToStack(L);
			}
			L.lua_pushlightuserdata(ContinuationHandle);
			return L.lua_yieldk(0, 0, LuaUtil.cfunc_CallDelegateFromStackTop);
		}

		private int continuation(LuaState L)
		{
			return returnContainer.PushToStack(L);
		}

		public void Start(QuestScriptInstance questScriptInstance, string scriptText, string filename, string startMethodName)
		{
			DisposedCheck();
			questInstance = questScriptInstance;
			fileName = filename;
			this.startMethodName = startMethodName;
			hasRanMethod = false;
			controller.Setup(questInstance);
			controller.ContinueSignal.AddListener(ContinueFromYield);
			CreateLuaThread();
			if (threadState.luaL_loadbufferx(scriptText, scriptText.Length, filename, null) > 0)
			{
				string message = threadState.lua_tostring(-1);
				threadState.lua_pop(1);
				LogLuaError(message);
			}
			else
			{
				threadState.lua_rawgeti(-1001000, envTableRef);
				threadState.lua_setupvalue(-2, 1);
				canContinue = true;
				Continue(0);
			}
		}

		public void Stop()
		{
			DisposedCheck();
			InternalStop();
		}

		public void Pause()
		{
			DisposedCheck();
			if (pauseResumeState == PauseResumeState.RUNNING)
			{
				pauseResumeState = PauseResumeState.WANT_TO_PAUSE;
			}
		}

		public void Resume()
		{
			DisposedCheck();
			PauseResumeState pauseResumeState = this.pauseResumeState;
			if (pauseResumeState != 0)
			{
				this.pauseResumeState = PauseResumeState.RUNNING;
				if (pauseResumeState == PauseResumeState.PAUSED)
				{
					Continue(0);
				}
			}
		}

		private void InternalStop()
		{
			controller.ContinueSignal.RemoveListener(ContinueFromYield);
			controller.Stop();
		}

		private void Continue(int nargs)
		{
			if (!canContinue)
			{
				logger.Error("LuaQuestStepRunner: Attempting to continue without a yielding thread.");
				return;
			}
			canContinue = false;
			ThreadStatus threadStatus = threadState.lua_resume(masterState, nargs);
			if (threadStatus == ThreadStatus.LUA_OK)
			{
				HandleContinueFinished();
			}
			else if (threadStatus > ThreadStatus.LUA_YIELD)
			{
				LogLuaRuntimeError();
				CreateLuaThread();
				Stop();
			}
			else
			{
				canContinue = true;
			}
		}

		private void ContinueFromYield()
		{
			if (pauseResumeState == PauseResumeState.WANT_TO_PAUSE)
			{
				pauseResumeState = PauseResumeState.PAUSED;
			}
			else
			{
				Continue(0);
			}
		}

		private void LogLuaError(string message)
		{
			logger.Log(KampaiLogLevel.Error, "Lua Error in {0}: {1}", fileName, message);
		}

		private void LogLuaRuntimeError()
		{
			string value = threadState.lua_tostring(-1);
			threadState.lua_pop(1);
			errorMessageBuilder.AppendLine(value);
			IntPtr intPtr = KampaiNativeLib.kampai_create_debug();
			for (int i = 0; threadState.lua_getstack(i, intPtr) > 0; i++)
			{
				KampaiNativeLib.DebugData debugData = KampaiNativeLib.kampai_get_debug(threadState, "nSl", intPtr);
				errorMessageBuilder.Append(debugData.name);
				errorMessageBuilder.Append(" : ");
				errorMessageBuilder.Append(debugData.line_number);
			}
			KampaiNativeLib.kampai_free_debug(intPtr);
			LogLuaError(errorMessageBuilder.ToString());
			errorMessageBuilder.Length = 0;
		}

		private void HandleContinueFinished()
		{
			if (!hasRanMethod && startMethodName != null)
			{
				hasRanMethod = true;
				threadState.lua_rawgeti(-1001000, envTableRef);
				threadState.lua_getfield(-1, startMethodName);
				canContinue = true;
				int nargs = _invokationValues.PushArrayValuesToStack(threadState);
				Continue(nargs);
			}
			else
			{
				Stop();
				if (OnQuestScriptComplete != null)
				{
					OnQuestScriptComplete(questInstance);
				}
			}
		}

		protected virtual void Dispose(bool fromDispose)
		{
			if (fromDispose)
			{
				DisposedCheck();
				Stop();
				threadState.Dispose();
				EnvIndexHandle.Dispose();
				EnvNewIndexHandle.Dispose();
				QSIndexHandle.Dispose();
				QSNewIndexHandle.Dispose();
				InvokeMethodFromLuaHandle.Dispose();
				ContinuationHandle.Dispose();
			}
			_isDisposed = true;
		}

		private void DisposedCheck()
		{
			if (_isDisposed)
			{
				throw new ObjectDisposedException(ToString());
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~LuaScriptRunner()
		{
			Dispose(false);
		}
	}
}
