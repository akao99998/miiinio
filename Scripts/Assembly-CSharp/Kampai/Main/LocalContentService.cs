using System.Collections.Generic;
using System.IO;
using Elevation.Logging;
using Kampai.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Kampai.Main
{
	public class LocalContentService : ILocalContentService
	{
		private IKampaiLogger logger = LogManager.GetClassLogger("LocalContentService") as IKampaiLogger;

		private Dictionary<string, string> resourceNamesMap;

		private Dictionary<int, Dictionary<string, string>> specificNamesMap;

		private HashSet<string> streamingAssets;

		private List<string> audioBanks;

		private int qualityLevel;

		[PostConstruct]
		public void PostConstruct()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("Manifest");
			if (null == textAsset)
			{
				logger.Error("Failed to load bundle resources manifest");
				return;
			}
			LocalResourcesManifest localResourcesManifest = JsonConvert.DeserializeObject<LocalResourcesManifest>(textAsset.text);
			Resources.UnloadAsset(textAsset);
			resourceNamesMap = new Dictionary<string, string>(localResourcesManifest.bundledAssets.Count);
			foreach (string bundledAsset in localResourcesManifest.bundledAssets)
			{
				string fileName = Path.GetFileName(bundledAsset);
				if (!resourceNamesMap.ContainsKey(fileName))
				{
					resourceNamesMap.Add(fileName, bundledAsset);
				}
			}
			specificNamesMap = FilterQualityLODFiles(localResourcesManifest.separatedAssets);
			streamingAssets = new HashSet<string>(localResourcesManifest.streamingAssets);
			audioBanks = localResourcesManifest.audioBanks;
		}

		public void SetDLCQuality(string levelName)
		{
			int num = ResourceQualityHelper.ConvertQualityStringToLODlevel(levelName);
			if (num > -1)
			{
				qualityLevel = num;
			}
			else
			{
				qualityLevel = 0;
			}
		}

		private Dictionary<int, Dictionary<string, string>> FilterQualityLODFiles(List<LODAsset> importedLODS)
		{
			Dictionary<int, Dictionary<string, string>> dictionary = new Dictionary<int, Dictionary<string, string>>();
			dictionary.Add(0, new Dictionary<string, string>());
			dictionary.Add(1, new Dictionary<string, string>());
			dictionary.Add(2, new Dictionary<string, string>());
			dictionary.Add(3, new Dictionary<string, string>());
			foreach (LODAsset importedLOD in importedLODS)
			{
				if (dictionary[importedLOD.level].ContainsKey(importedLOD.shortName))
				{
					logger.Error("LOD separated resource {0} is already mapped under level {1}", importedLOD.shortName, importedLOD.level);
				}
				else
				{
					dictionary[importedLOD.level].Add(importedLOD.shortName, importedLOD.path);
				}
			}
			return dictionary;
		}

		public bool IsLocalAsset(string name)
		{
			return resourceNamesMap.ContainsKey(name) || specificNamesMap[qualityLevel].ContainsKey(name);
		}

		public string GetAssetPath(string name)
		{
			if (!IsLocalAsset(name))
			{
				return string.Empty;
			}
			if (specificNamesMap[qualityLevel].ContainsKey(name))
			{
				return specificNamesMap[qualityLevel][name];
			}
			return resourceNamesMap[name];
		}

		public bool IsStreamingAsset(string name)
		{
			return streamingAssets.Contains(name);
		}

		public List<string> GetStreamingAudioBanks()
		{
			return audioBanks;
		}
	}
}
