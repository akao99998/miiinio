using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class NimbleBridge_Persistence : SafeHandle
{
	public enum Storage
	{
		STORAGE_DOCUMENT = 0,
		STORAGE_CACHE = 1,
		STORAGE_TEMP = 2
	}

	public enum RemoteSynchronization
	{
		REMOTE_LOCAL = 0,
		REMOTE_ICLOUD = 1,
		REMOTE_VAULT_SERVICE = 2
	}

	public enum MergePolicy
	{
		MERGE_OVERWRITE = 0,
		MERGE_SOURCE_FIRST = 1,
		MERGE_TARGET_FIRST = 2
	}

	public override bool IsInvalid
	{
		get
		{
			return base.IsClosed || handle == IntPtr.Zero;
		}
	}

	internal NimbleBridge_Persistence()
		: base(IntPtr.Zero, true)
	{
	}

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_PersistenceWrapper_Dispose(NimbleBridge_Persistence persistenceWrapper);

	[DllImport("NimbleCInterface")]
	private static extern string NimbleBridge_Persistence_getIdentifier(NimbleBridge_Persistence persistenceWrapper);

	[DllImport("NimbleCInterface")]
	private static extern int NimbleBridge_Persistence_getStorage(NimbleBridge_Persistence persistenceWrapper);

	[DllImport("NimbleCInterface")]
	private static extern bool NimbleBridge_Persistence_getEncryption(NimbleBridge_Persistence persistenceWrapper);

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_Persistence_setEncryption(NimbleBridge_Persistence persistenceWrapper, bool encryption);

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_Persistence_setValue(NimbleBridge_Persistence persistenceWrapper, string key, string value);

	[DllImport("NimbleCInterface")]
	private static extern string NimbleBridge_Persistence_getStringValue(NimbleBridge_Persistence persistenceWrapper, string key);

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_Persistence_addEntries(NimbleBridge_Persistence persistenceWrapper, IntPtr map);

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_Persistence_clean(NimbleBridge_Persistence persistenceWrapper);

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_Persistence_synchronize(NimbleBridge_Persistence persistenceWrapper);

	protected override bool ReleaseHandle()
	{
		NimbleBridge_PersistenceWrapper_Dispose(this);
		return true;
	}

	public string GetIdentifier()
	{
		return NimbleBridge_Persistence_getIdentifier(this);
	}

	public Storage GetStorage()
	{
		return (Storage)NimbleBridge_Persistence_getStorage(this);
	}

	public bool GetEncryption()
	{
		return NimbleBridge_Persistence_getEncryption(this);
	}

	public void SetEncryption(bool encryption)
	{
		NimbleBridge_Persistence_setEncryption(this, encryption);
	}

	public void SetValue(string key, string value)
	{
		NimbleBridge_Persistence_setValue(this, key, value);
	}

	public string GetStringValue(string key)
	{
		return NimbleBridge_Persistence_getStringValue(this, key);
	}

	public void AddEntries(Dictionary<string, string> dictionary)
	{
		IntPtr intPtr = IntPtr.Zero;
		try
		{
			intPtr = MarshalUtility.ConvertDictionaryToPtr(dictionary);
			NimbleBridge_Persistence_addEntries(this, intPtr);
		}
		finally
		{
			if (intPtr != IntPtr.Zero)
			{
				MarshalUtility.DisposeMapPtr(intPtr);
			}
		}
	}

	public void Clean()
	{
		NimbleBridge_Persistence_clean(this);
	}

	public void Synchronize()
	{
		NimbleBridge_Persistence_synchronize(this);
	}
}
