using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Elevation.Logging;
using FMOD;
using FMOD.Studio;
using Kampai.Main;
using Kampai.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Common.Service.Audio
{
	public class FMODService : IFMODService
	{
		private struct PendingBank
		{
			public Bank Bank;

			public string Name;

			public override string ToString()
			{
				return Name ?? "uninitialized";
			}
		}

		private const string TAG = "FMODService";

		private const string SHARED_AUDIO_CONTENT_LOCATION = "Content/Shared/";

		private const string LOCAL_AUDIO_CONTENT_LOCATION = "Content/Resources/Local/";

		private const string DLC_AUDIO_CONTENT_LOCATION = "Content/DLC/";

		private const string RESOURCES_AUDIO_CONTENT_LOCATION = "Content/Resources/";

		public const string RAW_MAP_BUNDLE_NAME = "Raw_FMOD_GlobalMap";

		private readonly IManifestService _manifestService;

		private readonly Dictionary<string, string> _nameIdMap = new Dictionary<string, string>();

		private Queue<PendingBank> pendingBanks = new Queue<PendingBank>();

		private Stopwatch allBanksAsyncSW;

		public IKampaiLogger logger = LogManager.GetClassLogger("FMODService") as IKampaiLogger;

		[Inject]
		public ILocalContentService localContentService { get; set; }

		[Inject]
		public IRoutineRunner routineRunner { get; set; }

		public FMODService(IManifestService manifestService)
		{
			_manifestService = manifestService;
		}

		IEnumerator IFMODService.InitializeSystem()
		{
			logger.EventStart("FMODService.InitializeSystem");
			TimeProfiler.StartSection("fmod");
			TimeProfiler.StartSection("maps");
			LoadEventMapsFromRawBundle();
			TimeProfiler.EndSection("maps");
			TimeProfiler.StartSection("bundles async load start");
			LoadAllFromAssetBundles();
			TimeProfiler.EndSection("bundles async load start");
			TimeProfiler.StartSection("streaming");
			yield return LoadStreamingAudioBanks();
			TimeProfiler.EndSection("streaming");
			TimeProfiler.EndSection("fmod");
			logger.EventStop("FMODService.InitializeSystem");
		}

		string IFMODService.GetGuid(string eventName)
		{
			if (_nameIdMap.ContainsKey(eventName))
			{
				return _nameIdMap[eventName];
			}
			logger.Error("eventName '{0}' was not found in the dictionary.", eventName);
			return null;
		}

		private void LoadAllFromAssetBundles()
		{
			logger.Debug("Starting Load of Audio Assets from Bundles");
			allBanksAsyncSW = Stopwatch.StartNew();
			foreach (string audioBundle in _manifestService.GetAudioBundles())
			{
				LoadFromAssetBundleAsync(audioBundle);
			}
			StartAsyncBankLoadingProcessing();
		}

		private string GetEventMapFilePath()
		{
			string bundleByOriginalName = _manifestService.GetBundleByOriginalName("Raw_FMOD_GlobalMap");
			return GetRawAssetPathByOriginalName(bundleByOriginalName);
		}

		private void LoadEventMapsFromRawBundle(bool logErrors = false)
		{
			FmodGlobalEventMap fmodGlobalEventMap = null;
			try
			{
				string eventMapFilePath = GetEventMapFilePath();
				JsonTextReader reader = new JsonTextReader(new StreamReader(eventMapFilePath));
				fmodGlobalEventMap = FastJSONDeserializer.Deserialize<FmodGlobalEventMap>(reader);
			}
			catch (JsonSerializationException ex)
			{
				logger.Fatal(FatalCode.FMOD_EVENT_MAP_ERROR, "Json Parse Err: {0}", ex);
			}
			catch (JsonReaderException ex2)
			{
				logger.Fatal(FatalCode.FMOD_EVENT_MAP_ERROR, "Json Parse Err: {0}", ex2);
			}
			catch (Exception ex3)
			{
				logger.Fatal(FatalCode.FMOD_EVENT_MAP_ERROR, "FmodGlobalEventMap load error: {0}", ex3);
			}
			foreach (KeyValuePair<string, Dictionary<string, string>> map in fmodGlobalEventMap.maps)
			{
				foreach (KeyValuePair<string, string> item in map.Value)
				{
					if (_nameIdMap.ContainsKey(item.Key))
					{
						if (logErrors && _nameIdMap[item.Key] != item.Value)
						{
							logger.Error("LoadEventMapsFromRawBundle: key: {0}\tvalue: {1}", item.Key, item.Value);
						}
					}
					else
					{
						_nameIdMap.Add(item.Key, item.Value);
					}
				}
			}
		}

		public bool LoadFromAssetBundleAsync(string bundleName)
		{
			bool flag = _manifestService.IsStreamingBundle(bundleName);
			string bundleOriginalName = _manifestService.GetBundleOriginalName(bundleName);
			if (bundleOriginalName.StartsWith("Raw_FMOD_GlobalMap"))
			{
				return false;
			}
			logger.Verbose("Async loading audio data from bundle {0} [{1}]", bundleName, bundleOriginalName);
			string rawAssetPathByOriginalName = GetRawAssetPathByOriginalName(bundleName);
			if (!string.IsNullOrEmpty(rawAssetPathByOriginalName))
			{
				if (!File.Exists(rawAssetPathByOriginalName) && !flag)
				{
					return false;
				}
				Bank bank = LoadLocalBankAsync(rawAssetPathByOriginalName);
				pendingBanks.Enqueue(new PendingBank
				{
					Bank = bank,
					Name = rawAssetPathByOriginalName
				});
				return true;
			}
			return false;
		}

		public void StartAsyncBankLoadingProcessing()
		{
			routineRunner.StartCoroutine(ProcessAsyncBankLoading());
		}

		private IEnumerator ProcessAsyncBankLoading()
		{
			while (true)
			{
				int queueCount = pendingBanks.Count;
				while (queueCount-- != 0)
				{
					PendingBank pb = pendingBanks.Dequeue();
					if (pb.Bank == null)
					{
						logger.Error("{0}: bank was removed during async bank loading, bank {1}.", "FMODService", pb);
						continue;
					}
					LOADING_STATE bankState;
					RESULT r = pb.Bank.getLoadingState(out bankState);
					if (r == RESULT.OK)
					{
						switch (bankState)
						{
						case LOADING_STATE.LOADING:
							pendingBanks.Enqueue(pb);
							break;
						case LOADING_STATE.UNLOADING:
						case LOADING_STATE.UNLOADED:
							logger.Error("{0}: unexpected bank state {1} on async bank loading, bank {2}", "FMODService", bankState, pb);
							break;
						case LOADING_STATE.ERROR:
							logger.Error("{0}: error bank state {1}, bank {2}", "FMODService", bankState, pb);
							pb.Bank.unload();
							break;
						}
					}
					else
					{
						logger.Error("{0}: getLoadingState error {1}, bank {2}", "FMODService", r, pb);
						pb.Bank.unload();
					}
				}
				if (pendingBanks.Count == 0)
				{
					break;
				}
				yield return null;
			}
			logger.Debug("{0}: All banks are loaded asynchronously in : {1}", "FMODService", allBanksAsyncSW.Elapsed);
		}

		public bool BanksLoadingInProgress()
		{
			return pendingBanks.Count != 0;
		}

		private void LoadEventsMap(string json, bool logErrors = false)
		{
			Dictionary<string, string> dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
			foreach (KeyValuePair<string, string> item in dictionary)
			{
				if (_nameIdMap.ContainsKey(item.Key))
				{
					if (logErrors && _nameIdMap[item.Key] != item.Value)
					{
						logger.Error("key: {0}\tvalue: {1}", item.Key, item.Value);
					}
				}
				else
				{
					_nameIdMap.Add(item.Key, item.Value);
				}
			}
		}

		private IEnumerator LoadBanksFromFileSystem()
		{
			List<string> bankFiles = GetFiles(".bytes");
			if (bankFiles == null)
			{
				yield break;
			}
			foreach (string bankFile in bankFiles)
			{
				if (FMOD_StudioSystem.instance.IsPaused())
				{
					yield return null;
				}
				Bank bank = null;
				RESULT result = FMOD_StudioSystem.instance.System.loadBankFile(bankFile, LOAD_BANK_FLAGS.NORMAL, out bank);
				if (result == RESULT.ERR_VERSION)
				{
					UnityUtil.LogError("These banks were built with an incompatible version of FMOD Studio.");
				}
				UnityUtil.Log("bank load: " + ((!(bank != null)) ? "failed!!" : "succeeded"));
				yield return null;
			}
		}

		private Bank LoadLocalBankAsync(string bankFile)
		{
			Bank bank = null;
			RESULT rESULT = FMOD_StudioSystem.instance.System.loadBankFile(bankFile, LOAD_BANK_FLAGS.NONBLOCKING, out bank);
			if (rESULT != 0)
			{
				logger.Error("LoadLocalBankAsync: for async loading OK always expected but got: {0}. Bank is null: {1}", rESULT, bank == null);
			}
			else
			{
				logger.Verbose("LoadLocalBankAsync: Bank {0} scheduled for loading", bankFile);
			}
			return bank;
		}

		private string GetStreamingBankPath(string bank)
		{
			return "file:///android_asset/" + bank + ".bytes";
		}

		private IEnumerator LoadStreamingAudioBanks()
		{
			logger.Debug("Start Loading Streaming Audio Banks");
			List<string> streamingBanks = localContentService.GetStreamingAudioBanks();
			foreach (string bankName in streamingBanks)
			{
				if (string.IsNullOrEmpty(_manifestService.GetAssetLocation(bankName)))
				{
					if (FMOD_StudioSystem.instance.IsPaused())
					{
						yield return null;
					}
					string path = GetStreamingBankPath(bankName);
					LoadLocalBankAsync(path);
				}
			}
		}

		private string GetRawAssetPathByOriginalName(string bundleName)
		{
			string bundleLocation = _manifestService.GetBundleLocation(bundleName);
			return Path.Combine(bundleLocation, bundleName + ".unity3d");
		}

		private IEnumerator LoadMapsFromFileSystem()
		{
			List<string> mapFiles = GetFiles("_map.json");
			if (mapFiles == null)
			{
				yield break;
			}
			foreach (string file in mapFiles)
			{
				using (FileStream stream = File.OpenRead(file))
				{
					using (StreamReader reader = new StreamReader(stream))
					{
						string json = reader.ReadToEnd();
						LoadEventsMap(json, true);
					}
				}
				yield return null;
			}
		}

		private void GetFilesAtPath(List<string> files, string path, string pattern, string fileEnding, bool recursive)
		{
			if (!Directory.Exists(path))
			{
				UnityUtil.LogError(path + " not found, no banks loaded.");
			}
			string[] directories = Directory.GetDirectories(path, pattern, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
			string[] array = directories;
			foreach (string path2 in array)
			{
				IEnumerable<string> collection = from file in Directory.GetFiles(path2, "*.*", SearchOption.AllDirectories)
					where file.EndsWith(fileEnding)
					select file;
				files.AddRange(collection);
			}
		}

		private List<string> GetFiles(string fileEnding)
		{
			List<string> list = new List<string>();
			string path = Application.dataPath + Path.DirectorySeparatorChar + "Content/Shared/";
			GetFilesAtPath(list, path, "Shared_Audio_*", fileEnding, false);
			string path2 = Application.dataPath + Path.DirectorySeparatorChar + "Content/DLC/";
			GetFilesAtPath(list, path2, "Audio", fileEnding, true);
			string path3 = Application.dataPath + Path.DirectorySeparatorChar + "Content/Resources/";
			GetFilesAtPath(list, path3, "Audio", fileEnding, true);
			return list;
		}
	}
}
