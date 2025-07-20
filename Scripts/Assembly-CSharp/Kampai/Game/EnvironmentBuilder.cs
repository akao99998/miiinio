using System;
using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Util;
using UnityEngine;

namespace Kampai.Game
{
	public class EnvironmentBuilder
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("EnvironmentBuilder") as IKampaiLogger;

		[Inject]
		public Environment environment { get; set; }

		public void Build(Dictionary<string, object> dict)
		{
			if (dict == null)
			{
				logger.Fatal(FatalCode.DS_NULL_DEF, "The environment dictionary is null");
				return;
			}
			int num = Convert.ToInt32(dict["x"]);
			int num2 = Convert.ToInt32(dict["y"]);
			List<int> list = null;
			list = dict["definitionLayout"] as List<int>;
			if (list.Count != num * num2)
			{
				logger.Fatal(FatalCode.DS_DEF_CORRUPTION, "The environment data in the json file has incorrect size.");
				return;
			}
			environment.OnDefinitionHotSwap(SetupDefinition(num, num2, list));
			environment.PlayerGrid = new EnvironmentGridSquare[num, num2];
			EnvironmentGridSquare[,] playerGrid = environment.PlayerGrid;
			EnvironmentGridSquareDefinition[,] definitionGrid = environment.Definition.DefinitionGrid;
			for (int num3 = num2 - 1; num3 >= 0; num3--)
			{
				for (int i = 0; i < num; i++)
				{
					playerGrid[i, num3] = new EnvironmentGridSquare();
					playerGrid[i, num3].Position = new Vector2(i, num3);
					int modifier = 0;
					if (definitionGrid[i, num3].CharacterPathable)
					{
						modifier = 8;
					}
					else if (!definitionGrid[i, num3].Water && definitionGrid[i, num3].Pathable)
					{
						modifier = ((!definitionGrid[i, num3].Usable) ? 4 : 5);
					}
					playerGrid[i, num3].Modifier = modifier;
				}
			}
		}

		private EnvironmentDefinition SetupDefinition(int xSize, int ySize, List<int> definitionLayout)
		{
			EnvironmentDefinition environmentDefinition = new EnvironmentDefinition();
			environmentDefinition.DefinitionGrid = new EnvironmentGridSquareDefinition[xSize, ySize];
			EnvironmentGridSquareDefinition[,] definitionGrid = environmentDefinition.DefinitionGrid;
			int num = 0;
			for (int num2 = ySize - 1; num2 >= 0; num2--)
			{
				for (int i = 0; i < xSize; i++)
				{
					definitionGrid[i, num2] = new EnvironmentGridSquareDefinition();
					definitionGrid[i, num2].SetModifiers(definitionLayout[num]);
					definitionGrid[i, num2].Position = new Vector2(i, num2);
					num++;
				}
			}
			return environmentDefinition;
		}

		public void DebugBuild(int xSize, int ySize, int definitionFlag, int playerFlag)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("x", xSize);
			dictionary.Add("y", ySize);
			List<int> list = new List<int>();
			for (int i = 0; i < xSize * ySize; i++)
			{
				list.Add(definitionFlag);
			}
			List<int> list2 = new List<int>();
			for (int j = 0; j < xSize * ySize; j++)
			{
				list2.Add(playerFlag);
			}
			dictionary.Add("definitionLayout", list);
			dictionary.Add("playerLayout", list2);
			Build(dictionary);
		}
	}
}
