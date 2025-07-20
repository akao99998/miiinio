using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Kampai.Util
{
	public static class ResourceQualityHelper
	{
		public enum LODQuality
		{
			LOD0 = 0,
			LOD1 = 1,
			LOD2 = 2,
			LOD3 = 3
		}

		public const string LODDirectoryPattern = "\\bLOD[0-3]\\b";

		public const string LODPatternInFullPath = "/LOD[0-3]";

		public const LODQuality defaultLODQuality = LODQuality.LOD0;

		public static string ConvertLODToQualityString(LODQuality quality)
		{
			switch (quality)
			{
			case LODQuality.LOD0:
				return "HIGH";
			case LODQuality.LOD1:
				return "MED";
			case LODQuality.LOD2:
				return "LOW";
			case LODQuality.LOD3:
				return "VERYLOW";
			default:
				Debug.LogError("error in LODQuality argument: returning empty string");
				return string.Empty;
			}
		}

		public static int ConvertQualityStringToLODlevel(string quality)
		{
			if (!string.IsNullOrEmpty(quality))
			{
				switch (quality.ToUpper())
				{
				case "HIGH":
					return 0;
				case "MED":
					return 1;
				case "LOW":
					return 2;
				case "VERYLOW":
					return 3;
				}
			}
			Debug.LogError("Invalid string argument: does not match an LOD quality.  Returning -1.");
			return -1;
		}

		public static int GetIntFromLODString(string lodString, bool fullPathSearch)
		{
			string empty = string.Empty;
			int result = -1;
			empty = ((!fullPathSearch) ? "\\bLOD[0-3]\\b" : "/LOD[0-3]");
			if (!string.IsNullOrEmpty(lodString))
			{
				Match match = Regex.Match(lodString, empty);
				if (match.Success && !int.TryParse(Regex.Match(match.Value, "\\d").Value, out result))
				{
					result = -1;
				}
			}
			return result;
		}

		public static HashSet<int> GetIntHashSetOfLevels()
		{
			HashSet<int> hashSet = new HashSet<int>();
			foreach (int value in Enum.GetValues(typeof(LODQuality)))
			{
				hashSet.Add(value);
			}
			return hashSet;
		}

		public static int GetLODQualityEnumCount()
		{
			return Enum.GetValues(typeof(LODQuality)).Length;
		}
	}
}
