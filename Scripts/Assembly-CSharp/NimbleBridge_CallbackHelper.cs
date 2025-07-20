using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

internal class NimbleBridge_CallbackHelper : MonoBehaviour
{
	private delegate void BridgeDisposeCallback(IntPtr userInfoPtr);

	private static NimbleBridge_CallbackHelper s_instance;

	private List<Action> m_pendingCallbacks = new List<Action>();

	private volatile bool m_callbacksPending;

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_setDisposeCallback(BridgeDisposeCallback callback);

	private void Awake()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.transform.gameObject);
	}

	internal static NimbleBridge_CallbackHelper Get()
	{
		if (object.ReferenceEquals(s_instance, null))
		{
			if (!Application.isPlaying)
			{
				return null;
			}
			GameObject gameObject = new GameObject("NimbleCallbackHelper");
			s_instance = gameObject.AddComponent<NimbleBridge_CallbackHelper>();
			NimbleBridge_setDisposeCallback(DisposeCallback);
		}
		return s_instance;
	}

	[MonoPInvokeCallback(typeof(BridgeDisposeCallback))]
	public static void DisposeCallback(IntPtr userInfoPtr)
	{
		GCHandle.FromIntPtr(userInfoPtr).Free();
	}

	internal IntPtr MakeCallbackData(object data)
	{
		return GCHandle.ToIntPtr(GCHandle.Alloc(data));
	}

	internal object GetData(IntPtr dataPtr)
	{
		return GCHandle.FromIntPtr(dataPtr).Target;
	}

	internal void RunOnMainThread(Action action)
	{
		lock (m_pendingCallbacks)
		{
			m_pendingCallbacks.Add(action);
			m_callbacksPending = true;
		}
	}

	private void Update()
	{
		if (!m_callbacksPending)
		{
			return;
		}
		List<Action> list;
		lock (m_pendingCallbacks)
		{
			list = new List<Action>(m_pendingCallbacks);
			m_pendingCallbacks.Clear();
			m_callbacksPending = false;
		}
		foreach (Action item in list)
		{
			item();
		}
	}
}
