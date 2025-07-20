using System.Collections.Generic;
using UnityEngine;

public class LocalPersistanceService : ILocalPersistanceService
{
	private struct CachedData
	{
		public string StringData;

		public int IntData;

		public CachedData(string data)
		{
			StringData = data;
			IntData = 0;
		}

		public CachedData(int data)
		{
			IntData = data;
			StringData = null;
		}
	}

	private string EnvPrefix;

	private Dictionary<string, CachedData> cache = new Dictionary<string, CachedData>();

	[PostConstruct]
	public void PostConstruct()
	{
		EnvPrefix = ".";
		PlayerPrefs.SetString("EnvPrefix", EnvPrefix);
		PlayerPrefs.Save();
	}

	public void PutData(string name, string data)
	{
		cache[name] = new CachedData(data);
		PlayerPrefs.SetString(EnvPrefix + name, data);
		PlayerPrefs.Save();
	}

	public void PutDataInt(string name, int data)
	{
		cache[name] = new CachedData(data);
		PlayerPrefs.SetInt(EnvPrefix + name, data);
		PlayerPrefs.Save();
	}

	public string GetData(string name)
	{
		if (cache.ContainsKey(name))
		{
			return cache[name].StringData;
		}
		string @string = PlayerPrefs.GetString(EnvPrefix + name);
		cache[name] = new CachedData(@string);
		return @string;
	}

	public int GetDataInt(string name)
	{
		if (cache.ContainsKey(name))
		{
			return cache[name].IntData;
		}
		int @int = PlayerPrefs.GetInt(EnvPrefix + name);
		cache[name] = new CachedData(@int);
		return @int;
	}

	public void PutDataPlayer(string name, string data)
	{
		PutData(name + GetData("UserID"), data);
	}

	public void PutDataIntPlayer(string name, int data)
	{
		PutDataInt(name + GetData("UserID"), data);
	}

	public string GetDataPlayer(string name)
	{
		return GetData(name + GetData("UserID"));
	}

	public int GetDataIntPlayer(string name)
	{
		return GetDataInt(name + GetData("UserID"));
	}

	public void DeleteAll()
	{
		cache.Clear();
		PlayerPrefs.DeleteAll();
		PlayerPrefs.Save();
	}

	public bool HasKey(string name)
	{
		return cache.ContainsKey(name) || PlayerPrefs.HasKey(EnvPrefix + name);
	}

	public void DeleteKey(string name)
	{
		if (cache.ContainsKey(name))
		{
			cache.Remove(name);
		}
		PlayerPrefs.DeleteKey(EnvPrefix + name);
		PlayerPrefs.Save();
	}

	public bool HasKeyPlayer(string name)
	{
		return HasKey(name + GetData("UserID"));
	}

	public void DeleteKeyPlayer(string name)
	{
		DeleteKey(name + GetData("UserID"));
	}
}
