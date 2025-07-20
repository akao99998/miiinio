using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SimpleJSON;

internal class MarshalUtility
{
	public const string CINTERFACE_LIB_NAME = "NimbleCInterface";

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_deleteMap(IntPtr map);

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_deleteData(IntPtr data);

	[DllImport("NimbleCInterface")]
	private static extern void NimbleBridge_deleteStringArray(IntPtr array);

	internal static Dictionary<string, string> ConvertPtrToDictionary(IntPtr mapPtr)
	{
		NimbleBridge_Map nimbleBridge_Map = (NimbleBridge_Map)Marshal.PtrToStructure(mapPtr, typeof(NimbleBridge_Map));
		IntPtr[] array = new IntPtr[nimbleBridge_Map.length];
		Marshal.Copy(nimbleBridge_Map.keys, array, 0, nimbleBridge_Map.length);
		string[] array2 = new string[nimbleBridge_Map.length];
		for (int i = 0; i < nimbleBridge_Map.length; i++)
		{
			array2[i] = Marshal.PtrToStringAuto(array[i]);
		}
		IntPtr[] array3 = new IntPtr[nimbleBridge_Map.length];
		Marshal.Copy(nimbleBridge_Map.values, array3, 0, nimbleBridge_Map.length);
		string[] array4 = new string[nimbleBridge_Map.length];
		for (int j = 0; j < nimbleBridge_Map.length; j++)
		{
			array4[j] = Marshal.PtrToStringAuto(array3[j]);
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>(nimbleBridge_Map.length);
		for (int k = 0; k < nimbleBridge_Map.length; k++)
		{
			dictionary[array2[k]] = array4[k];
		}
		NimbleBridge_deleteMap(mapPtr);
		return dictionary;
	}

	internal static IntPtr ConvertDictionaryToPtr(Dictionary<string, string> dictionary)
	{
		NimbleBridge_Map nimbleBridge_Map = default(NimbleBridge_Map);
		IntPtr zero = IntPtr.Zero;
		nimbleBridge_Map.length = dictionary.Count;
		string[] array = new string[nimbleBridge_Map.length];
		string[] array2 = new string[nimbleBridge_Map.length];
		nimbleBridge_Map.keys = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IntPtr)) * nimbleBridge_Map.length);
		nimbleBridge_Map.values = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IntPtr)) * nimbleBridge_Map.length);
		zero = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NimbleBridge_Map)));
		dictionary.Keys.CopyTo(array, 0);
		IntPtr[] array3 = new IntPtr[nimbleBridge_Map.length];
		for (int i = 0; i < nimbleBridge_Map.length; i++)
		{
			array3[i] = Marshal.StringToHGlobalAuto(array[i]);
		}
		Marshal.Copy(array3, 0, nimbleBridge_Map.keys, nimbleBridge_Map.length);
		dictionary.Values.CopyTo(array2, 0);
		IntPtr[] array4 = new IntPtr[nimbleBridge_Map.length];
		for (int j = 0; j < nimbleBridge_Map.length; j++)
		{
			array4[j] = Marshal.StringToHGlobalAuto(array2[j]);
		}
		Marshal.Copy(array4, 0, nimbleBridge_Map.values, nimbleBridge_Map.length);
		Marshal.StructureToPtr(nimbleBridge_Map, zero, false);
		return zero;
	}

	internal static void DisposeMapPtr(IntPtr mapPtr)
	{
		if (mapPtr != IntPtr.Zero)
		{
			NimbleBridge_Map nimbleBridge_Map = (NimbleBridge_Map)Marshal.PtrToStructure(mapPtr, typeof(NimbleBridge_Map));
			for (int i = 0; i < nimbleBridge_Map.length; i++)
			{
				Marshal.FreeHGlobal(Marshal.ReadIntPtr(nimbleBridge_Map.keys, i * IntPtr.Size));
				Marshal.FreeHGlobal(Marshal.ReadIntPtr(nimbleBridge_Map.values, i * IntPtr.Size));
			}
			if (nimbleBridge_Map.keys != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(nimbleBridge_Map.keys);
			}
			if (nimbleBridge_Map.values != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(nimbleBridge_Map.values);
			}
			Marshal.FreeHGlobal(mapPtr);
		}
	}

	internal static byte[] ConvertPtrToData(IntPtr dataPtr)
	{
		NimbleBridge_Data nimbleBridge_Data = (NimbleBridge_Data)Marshal.PtrToStructure(dataPtr, typeof(NimbleBridge_Data));
		byte[] array = new byte[nimbleBridge_Data.length];
		if (nimbleBridge_Data.bytes != IntPtr.Zero)
		{
			Marshal.Copy(nimbleBridge_Data.bytes, array, 0, nimbleBridge_Data.length);
		}
		else
		{
			array = null;
		}
		NimbleBridge_deleteData(dataPtr);
		return array;
	}

	internal static IntPtr ConvertDataToPtr(byte[] array)
	{
		NimbleBridge_Data nimbleBridge_Data = default(NimbleBridge_Data);
		if (array != null)
		{
			nimbleBridge_Data.bytes = Marshal.AllocHGlobal(array.Length * 1);
			nimbleBridge_Data.length = array.Length;
			Marshal.Copy(array, 0, nimbleBridge_Data.bytes, array.Length);
		}
		else
		{
			nimbleBridge_Data.bytes = IntPtr.Zero;
			nimbleBridge_Data.length = 0;
		}
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(nimbleBridge_Data));
		Marshal.StructureToPtr(nimbleBridge_Data, intPtr, false);
		return intPtr;
	}

	internal static void DisposeDataPtr(IntPtr dataPtr)
	{
		NimbleBridge_Data nimbleBridge_Data = (NimbleBridge_Data)Marshal.PtrToStructure(dataPtr, typeof(NimbleBridge_Data));
		if (nimbleBridge_Data.bytes != IntPtr.Zero)
		{
			Marshal.FreeHGlobal(nimbleBridge_Data.bytes);
		}
		Marshal.FreeHGlobal(dataPtr);
	}

	internal static string[] ConvertPtrToArray(IntPtr ptr)
	{
		List<string> list = new List<string>();
		if (ptr == IntPtr.Zero)
		{
			return list.ToArray();
		}
		IntPtr intPtr = Marshal.ReadIntPtr(ptr);
		int num = 1;
		while (intPtr != IntPtr.Zero)
		{
			list.Add(Marshal.PtrToStringAuto(intPtr));
			intPtr = Marshal.ReadIntPtr(ptr, num * Marshal.SizeOf(typeof(IntPtr)));
			num++;
		}
		NimbleBridge_deleteStringArray(ptr);
		return list.ToArray();
	}

	internal static object ConvertJsonToObject(JSONNode jsonNode)
	{
		if (jsonNode == null)
		{
			return null;
		}
		JSONArray asArray = jsonNode.AsArray;
		if (asArray != null)
		{
			return ConvertJsonToList(asArray);
		}
		JSONClass asObject = jsonNode.AsObject;
		if (asObject != null)
		{
			return ConvertJsonToDictionary(asObject);
		}
		return jsonNode.Value;
	}

	internal static List<object> ConvertJsonToList(JSONArray jsonArray)
	{
		List<object> list = new List<object>();
		if (jsonArray != null)
		{
			foreach (JSONNode item in jsonArray)
			{
				list.Add(ConvertJsonToObject(item));
			}
		}
		return list;
	}

	internal static Dictionary<string, object> ConvertJsonToDictionary(JSONClass jsonObject)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (jsonObject != null)
		{
			foreach (KeyValuePair<string, JSONNode> item in jsonObject)
			{
				dictionary.Add(item.Key, ConvertJsonToObject(item.Value));
			}
		}
		return dictionary;
	}
}
