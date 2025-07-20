using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Kampai.BuildingsSizeToolbox
{
	internal sealed class DefinitionsUpdater
	{
		private const string BUILDING_DEFINITIONS_SECTION = "buildingDefinitions";

		private List<string> lines = new List<string>(65536);

		private string inputFileName;

		private Regex BRACE_OPEN = new Regex("\\s*{");

		private Regex BRACKET_CLOSE = new Regex("\\s*\\]");

		private Regex READ_KEY_REGEX = new Regex("\\s*\\\"(\\w+)\\\":");

		private Regex READ_ID_VALUE = new Regex("\\s*\\\"\\w+\\\":\\s*(\\d+)");

		private Regex REPLACE_FLOAT_VALUE = new Regex("(\\s*\\\"\\w+\\\":\\s*)([-+]?[0-9]*\\.?[0-9]+(?:[eE][-+]?[0-9]+)?)(.*)");

		public DefinitionsUpdater(string defsPath = "_Kampai_/Resources/dev_definitions.json")
		{
			inputFileName = Path.Combine(Application.dataPath, defsPath);
		}

		public bool Update(Dictionary<int, BuildingsSizeToolboxManagerView.UIPositionInfo> updatedBuildings)
		{
			using (FileStream stream = new FileStream(inputFileName, FileMode.Open, FileAccess.Read))
			{
				using (StreamReader streamReader = new StreamReader(stream))
				{
					while (!streamReader.EndOfStream)
					{
						string text = streamReader.ReadLine();
						lines.Add(text);
						if (text.Contains("buildingDefinitions"))
						{
							break;
						}
					}
					while (!streamReader.EndOfStream)
					{
						string text = streamReader.ReadLine();
						lines.Add(text);
						if (BRACE_OPEN.IsMatch(text))
						{
							ProcessBuilding(streamReader, updatedBuildings);
						}
						else if (BRACKET_CLOSE.IsMatch(text))
						{
							break;
						}
					}
					while (!streamReader.EndOfStream)
					{
						string text = streamReader.ReadLine();
						lines.Add(text);
					}
				}
			}
			using (FileStream stream2 = new FileStream(inputFileName, FileMode.Open, FileAccess.Write))
			{
				using (StreamWriter streamWriter = new StreamWriter(stream2))
				{
					foreach (string line in lines)
					{
						streamWriter.WriteLine(line);
					}
				}
			}
			return true;
		}

		private void ProcessBuilding(StreamReader sr, Dictionary<int, BuildingsSizeToolboxManagerView.UIPositionInfo> updatedBuildings)
		{
			int result = -1;
			int num = -1;
			int num2 = -1;
			int num3 = 1;
			while (!sr.EndOfStream)
			{
				string text = sr.ReadLine();
				lines.Add(text);
				if (text.Contains("{"))
				{
					num3++;
				}
				if (text.Contains("}"))
				{
					num3--;
					if (num3 == 0)
					{
						break;
					}
				}
				Match match = READ_KEY_REGEX.Match(text);
				switch (match.Groups[1].Value.ToUpper())
				{
				case "ID":
				{
					Match match2 = READ_ID_VALUE.Match(text);
					if (match2.Success)
					{
						int.TryParse(match2.Groups[1].Value, out result);
					}
					break;
				}
				case "UIPOSITION":
					num = lines.Count - 1;
					break;
				case "UISCALE":
					num2 = lines.Count - 1;
					break;
				}
			}
			BuildingsSizeToolboxManagerView.UIPositionInfo value;
			if (updatedBuildings.TryGetValue(result, out value))
			{
				int index;
				string text2;
				if (num2 > 0)
				{
					replaceFloatInLine(num2, value.Scale);
				}
				else
				{
					int num4 = lines.Count - 1;
					string item = lines[num4];
					List<string> list;
					List<string> list2 = (list = lines);
					int index2 = (index = num4 - 1);
					text2 = list[index];
					list2[index2] = text2 + ",";
					lines[num4] = string.Format("            \"uiScale\": {0}", value.Scale);
					lines.Add(item);
				}
				if (num > 0)
				{
					Vector3 position = value.Position;
					replaceFloatInLine(num + 1, position.x);
					replaceFloatInLine(num + 2, position.y);
					replaceFloatInLine(num + 3, position.z);
					return;
				}
				Vector3 position2 = value.Position;
				int num5 = lines.Count - 1;
				string item2 = lines[num5];
				List<string> list3;
				List<string> list4 = (list3 = lines);
				int index3 = (index = num5 - 1);
				text2 = list3[index];
				list4[index3] = text2 + ",";
				lines[num5] = "            \"uiPosition\": {";
				lines.Add(string.Format("                \"x\": {0},", position2.x));
				lines.Add(string.Format("                \"y\": {0},", position2.y));
				lines.Add(string.Format("                \"z\": {0}", position2.z));
				lines.Add("            }");
				lines.Add(item2);
			}
		}

		private void replaceFloatInLine(int index, float value)
		{
			lines[index] = REPLACE_FLOAT_VALUE.Replace(lines[index], (Match m) => m.Groups[1].Value + value + m.Groups[3].Value);
		}
	}
}
