using System.Collections.Generic;
using Elevation.Logging;
using Kampai.Util;
using UnityEngine;
using UnityEngine.Rendering;
using strange.extensions.command.impl;

namespace Kampai.Game
{
	public class PopulateEnvironmentCommand : Command
	{
		public IKampaiLogger logger = LogManager.GetClassLogger("PopulateEnvironmentCommand") as IKampaiLogger;

		private GameObject parent;

		private Color darkGreen = new Color(0f, 0.5f, 0f, 1f);

		private Color darkBlue = new Color(0f, 0f, 0.5f, 1f);

		private Color lightBlue = new Color(0f, 1f, 1f, 1f);

		private Color brown = new Color(0.5f, 0.4f, 0.3f, 1f);

		private Color green = new Color(0f, 1f, 0f, 1f);

		private Color red = new Color(1f, 0f, 0f, 1f);

		private Color blue = new Color(0f, 0f, 1f, 1f);

		private Color locked = new Color(1f, 1f, 0f, 1f);

		private Color sidewalk = new Color(0.8f, 0f, 1f, 1f);

		private Color occupied = new Color(0.6f, 0f, 0f, 1f);

		private Color error = new Color(1f, 0f, 0.8f, 1f);

		[Inject]
		public Environment environment { get; set; }

		[Inject]
		public EnvironmentBuilder environmentBuilder { get; set; }

		[Inject]
		public DebugUpdateGridSignal DebugUpdateGridSignal { get; set; }

		[Inject]
		public IDefinitionService definitionService { get; set; }

		[Inject]
		public bool display { get; set; }

		[Inject]
		public EnvironmentState state { get; set; }

		public override void Execute()
		{
			logger.EventStart("PopulateEnvironmentCommand.Execute");
			if (!state.EnvironmentBuilt)
			{
				IList<string> environemtDefinition = definitionService.GetEnvironemtDefinition();
				definitionService.ReclaimEnfironmentDefinitions();
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				dictionary.Add("x", environemtDefinition[0].Length);
				dictionary.Add("y", environemtDefinition.Count);
				List<int> list = new List<int>(environemtDefinition[0].Length * environemtDefinition.Count);
				foreach (string item in environemtDefinition)
				{
					foreach (char c in item)
					{
						if (c >= '0' && c <= '9')
						{
							list.Add(c - 48);
						}
					}
				}
				dictionary.Add("definitionLayout", list);
				environmentBuilder.Build(dictionary);
				state.EnvironmentBuilt = true;
			}
			else
			{
				logger.Debug("PopulateEnvironmentCommand: Environment already built.");
			}
			if (display)
			{
				logger.Debug("PopulateEnvironmentCommand: Setting up grid and textures.");
				SetupGrid();
				UpdateTextures();
				DebugUpdateGridSignal.AddListener(UpdateTextures);
				state.DisplayOn = true;
			}
			else if (state.DisplayOn)
			{
				RemoveGrid();
				DebugUpdateGridSignal.RemoveListener(UpdateTextures);
				state.DisplayOn = false;
			}
			logger.EventStop("PopulateEnvironmentCommand.Execute");
		}

		private void SetupGrid()
		{
			if (!state.GridConstructed || state.EnvironmentObject == null)
			{
				logger.EventStart("PopulateEnvironmentCommand.SetupGrid");
				Shader shader = Shader.Find("Kampai/Standard/Texture");
				if (shader == null)
				{
					shader = Shader.Find("Diffuse");
				}
				parent = GameObject.CreatePrimitive(PrimitiveType.Quad);
				parent.isStatic = true;
				parent.name = "Environment";
				Object.Destroy(parent.GetComponent<Collider>());
				parent.transform.localScale = new Vector3(250f, 250f, 0f);
				parent.transform.Rotate(new Vector3(90f, 0f, 0f));
				Renderer component = parent.GetComponent<Renderer>();
				component.shadowCastingMode = ShadowCastingMode.Off;
				component.receiveShadows = false;
				component.material = new Material(shader);
				Material material = component.material;
				material.EnableKeyword("TEXTURE_ALPHA");
				material.SetFloat("_Alpha", 0f);
				material.color = new Color(1f, 1f, 1f, 0.5f);
				material.SetFloat("_OffsetFactor", -6f);
				material.SetFloat("_OffsetUnits", -1f);
				int length = environment.Definition.DefinitionGrid.GetLength(0);
				int length2 = environment.Definition.DefinitionGrid.GetLength(1);
				parent.transform.position = new Vector3((float)length / 2f - 0.5f, 0f, (float)length2 / 2f - 0.5f);
				Texture2D texture2D = new Texture2D(length, length2, TextureFormat.ARGB32, false);
				texture2D.filterMode = FilterMode.Point;
				texture2D.wrapMode = TextureWrapMode.Clamp;
				material.mainTexture = texture2D;
				state.GridConstructed = true;
				state.EnvironmentObject = parent;
				AddGridLines(parent.transform, length, length2);
				logger.EventStop("PopulateEnvironmentCommand.SetupGrid");
			}
			else if (state.EnvironmentObject != null)
			{
				state.EnvironmentObject.SetActive(true);
			}
		}

		private void RemoveGrid()
		{
			if (state.EnvironmentObject != null)
			{
				state.EnvironmentObject.SetActive(false);
			}
		}

		private void AddGridLines(Transform parent, int rows, int cols)
		{
			Color color = new Color(0.4f, 0.4f, 0.4f, 1f);
			Shader shader = Shader.Find("Kampai/Standard/Texture");
			if (shader == null)
			{
				shader = Shader.Find("Diffuse");
			}
			Material material = new Material(shader);
			material.color = color;
			material.SetFloat("_OffsetFactor", -6f);
			material.SetFloat("_OffsetUnits", -6f);
			GameObject gameObject = new GameObject("Grid Lines");
			gameObject.transform.parent = parent;
			LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
			lineRenderer.material = material;
			lineRenderer.SetWidth(0.025f, 0.025f);
			lineRenderer.SetColors(color, color);
			int num = 0;
			lineRenderer.SetVertexCount(2 * cols + 2);
			for (int i = 0; i <= cols; i++)
			{
				float x = (float)i - 0.5f;
				float z = -0.5f;
				float z2 = (float)rows - 0.5f;
				if (i % 2 == 1)
				{
					z = (float)rows - 0.5f;
					z2 = -0.5f;
				}
				lineRenderer.SetPosition(num++, new Vector3(x, 0f, z));
				lineRenderer.SetPosition(num++, new Vector3(x, 0f, z2));
			}
			gameObject = new GameObject("Grid Lines");
			gameObject.transform.parent = parent;
			lineRenderer = gameObject.AddComponent<LineRenderer>();
			lineRenderer.material = material;
			lineRenderer.SetWidth(0.025f, 0.025f);
			lineRenderer.SetColors(color, color);
			num = 0;
			lineRenderer.SetVertexCount(2 * rows + 2);
			for (int j = 0; j <= rows; j++)
			{
				float z3 = (float)j - 0.5f;
				float x2 = -0.5f;
				float x3 = (float)cols - 0.5f;
				if (j % 2 == 1)
				{
					x2 = (float)cols - 0.5f;
					x3 = -0.5f;
				}
				lineRenderer.SetPosition(num++, new Vector3(x2, 0f, z3));
				lineRenderer.SetPosition(num++, new Vector3(x3, 0f, z3));
			}
		}

		private void UpdateTextures()
		{
			if (parent == null)
			{
				return;
			}
			logger.EventStart("PopulateEnvironmentCommand.UpdateTextures");
			int length = environment.Definition.DefinitionGrid.GetLength(0);
			int length2 = environment.Definition.DefinitionGrid.GetLength(1);
			EnvironmentGridSquareDefinition[,] definitionGrid = environment.Definition.DefinitionGrid;
			EnvironmentGridSquare[,] playerGrid = environment.PlayerGrid;
			Texture2D texture2D = parent.GetComponent<Renderer>().material.mainTexture as Texture2D;
			for (int i = 0; i < length2; i++)
			{
				for (int j = 0; j < length; j++)
				{
					if (definitionGrid[j, i].Water)
					{
						if (definitionGrid[j, i].Usable)
						{
							if (definitionGrid[j, i].Pathable)
							{
								texture2D.SetPixel(j, i, blue);
							}
							else
							{
								texture2D.SetPixel(j, i, lightBlue);
							}
						}
						else if (definitionGrid[j, i].Pathable)
						{
							texture2D.SetPixel(j, i, red);
						}
						else
						{
							texture2D.SetPixel(j, i, darkBlue);
						}
					}
					else if (!definitionGrid[j, i].Usable && !playerGrid[j, i].Occupied)
					{
						if (definitionGrid[j, i].Pathable)
						{
							texture2D.SetPixel(j, i, brown);
						}
						else
						{
							texture2D.SetPixel(j, i, darkGreen);
						}
					}
					else if (!playerGrid[j, i].Unlocked)
					{
						if (playerGrid[j, i].Walkable)
						{
							texture2D.SetPixel(j, i, sidewalk);
						}
						else
						{
							texture2D.SetPixel(j, i, locked);
						}
					}
					else if (playerGrid[j, i].Occupied && playerGrid[j, i].Walkable)
					{
						texture2D.SetPixel(j, i, sidewalk);
					}
					else if (playerGrid[j, i].Occupied)
					{
						texture2D.SetPixel(j, i, occupied);
					}
					else if (!playerGrid[j, i].Occupied)
					{
						texture2D.SetPixel(j, i, green);
					}
					else
					{
						texture2D.SetPixel(j, i, error);
					}
				}
			}
			texture2D.Apply();
			parent.GetComponent<Renderer>().material.mainTexture = texture2D;
			logger.EventStop("PopulateEnvironmentCommand.UpdateTextures");
		}
	}
}
