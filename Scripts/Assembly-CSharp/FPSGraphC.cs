using System;
using System.Collections;
using System.Diagnostics;
using Kampai.Util;
using UnityEngine;

public class FPSGraphC : MonoBehaviour
{
	private enum FPSGraphViewMode
	{
		graphing = 0,
		totalperformance = 1,
		assetbreakdown = 2
	}

	private Material mat;

	public bool showPerformanceOnClick = true;

	public bool showFPSNumber;

	public int graphMultiply = 2;

	public Vector2 graphPosition = new Vector2(0f, 0f);

	public int frameHistoryLength = 120;

	public Color CpuColor = new Color(0.20784314f, 8f / 15f, 0.654902f, 1f);

	public Color RenderColor = new Color(0.4392157f, 52f / 85f, 2f / 85f, 1f);

	public Color OtherColor = new Color(0.75686276f, 36f / 85f, 0.003921569f, 1f);

	private readonly int[] numberBits = new int[195]
	{
		1, 1, 1, 0, 1, 1, 1, 0, 1, 1,
		1, 0, 1, 1, 1, 0, 0, 0, 1, 0,
		1, 1, 1, 0, 1, 1, 1, 0, 0, 1,
		0, 0, 1, 1, 1, 0, 0, 0, 1, 1,
		0, 1, 0, 0, 1, 0, 0, 0, 1, 0,
		0, 0, 0, 1, 0, 0, 0, 1, 0, 0,
		0, 1, 0, 1, 0, 1, 0, 0, 1, 0,
		0, 1, 0, 1, 0, 0, 0, 1, 1, 0,
		1, 0, 0, 1, 0, 0, 0, 0, 1, 0,
		0, 1, 1, 0, 1, 1, 1, 0, 1, 1,
		1, 0, 1, 1, 1, 0, 0, 0, 1, 0,
		1, 1, 1, 0, 1, 1, 1, 1, 0, 1,
		0, 1, 1, 0, 0, 1, 0, 1, 0, 0,
		0, 1, 0, 1, 0, 1, 0, 1, 0, 0,
		0, 1, 0, 0, 0, 0, 0, 1, 0, 1,
		0, 1, 0, 1, 0, 1, 1, 1, 1, 0,
		0, 1, 0, 0, 0, 1, 0, 0, 1, 1,
		1, 0, 0, 0, 1, 0, 1, 1, 1, 0,
		1, 1, 1, 0, 1, 1, 1, 0, 1, 1,
		1, 0, 1, 1, 1
	};

	private readonly int[] fpsBits = new int[44]
	{
		1, 0, 0, 0, 1, 0, 0, 0, 1, 1,
		1, 1, 1, 0, 0, 1, 1, 1, 0, 0,
		1, 1, 1, 0, 0, 0, 1, 0, 1, 0,
		1, 1, 0, 0, 1, 1, 0, 1, 1, 1,
		0, 1, 1, 1
	};

	private readonly int[] mbBits = new int[36]
	{
		1, 0, 1, 0, 1, 0, 1, 1, 1, 1,
		0, 1, 0, 1, 0, 1, 0, 1, 1, 1,
		1, 1, 1, 0, 1, 1, 1, 0, 0, 0,
		0, 0, 0, 1, 0, 0
	};

	private readonly int[] gbBits = new int[28]
	{
		1, 1, 1, 0, 1, 1, 1, 1, 0, 1,
		0, 1, 0, 1, 1, 0, 0, 0, 1, 1,
		1, 1, 1, 1, 0, 1, 0, 0
	};

	private readonly float[] graphKeysAlpha = new float[475]
	{
		0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 1f,
		1f, 1f, 0f, 0f, 1f, 0f, 0f, 1f, 0f, 1f,
		0f, 1f, 1f, 1f, 0f, 1f, 0f, 0f, 0f, 0f,
		0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f,
		0f, 0f, 1f, 1f, 1f, 0f, 1f, 0f, 0f, 0f,
		1f, 1f, 1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f,
		0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 1f,
		0f, 0f, 0f, 1f, 1f, 1f, 0f, 1f, 0f, 1f,
		0f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 0f, 1f,
		0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f,
		0f, 0f, 0f, 0f, 1f, 0f, 1f, 0f, 0f, 1f,
		0f, 0f, 1f, 0f, 1f, 0f, 1f, 0f, 0f, 0f,
		1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f,
		0f, 0f, 0f, 0f, 0f, 0f, 0f, 1f, 0f, 0f,
		0f, 1f, 1f, 1f, 0f, 1f, 0f, 1f, 0f, 0f,
		0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f,
		0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 1f, 0f,
		0f, 0f, 1f, 0f, 1f, 0f, 1f, 0f, 1f, 0f,
		1f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 0f,
		0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 1f,
		0f, 1f, 0f, 0f, 1f, 0f, 0f, 1f, 0f, 1f,
		0f, 1f, 1f, 1f, 0f, 1f, 1f, 0f, 0f, 0f,
		0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f,
		0f, 0f, 1f, 0f, 0f, 0f, 1f, 0f, 1f, 0f,
		1f, 0f, 1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f,
		0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 1f,
		1f, 0f, 0f, 1f, 1f, 1f, 0f, 1f, 0f, 1f,
		0f, 1f, 0f, 1f, 0f, 1f, 1f, 1f, 0f, 1f,
		1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f,
		0f, 0f, 0f, 0f, 1f, 1f, 1f, 0f, 1f, 1f,
		1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 0f,
		1f, 0f, 1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f,
		0f, 0f, 0f, 0f, 0f, 0f, 0f, 1f, 1f, 1f,
		0f, 1f, 1f, 1f, 0f, 1f, 0f, 1f, 0f, 0f,
		0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f,
		0f, 0f, 0f, 0f, 1f, 0f, 1f, 0f, 1f, 1f,
		1f, 0f, 1f, 1f, 1f, 0f, 1f, 1f, 1f, 0f,
		1f, 1f, 1f, 0f, 1f, 0f, 1f, 0f, 0f, 0f,
		0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f,
		0f, 0f, 0f, 0f, 1f, 0f, 0f, 1f, 0f, 0f,
		0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f,
		0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f,
		0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f,
		0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f,
		0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f,
		0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f,
		0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 0f, 0f,
		0f, 0f, 0f, 0f, 0f
	};

	private Texture2D graphTexture;

	private int graphHeight = 100;

	private int[,] textOverlayMask;

	private float[,] dtHistory;

	private int[] gcHistory;

	private int i;

	private int j;

	private int x;

	private int y;

	private float val;

	private Color32 color32;

	private float maxFrame;

	private float actualFPS;

	private float lastUpdate;

	private float yMulti;

	private float beforeRender;

	private float[] fpsVals = new float[3];

	private float x1;

	private float x2;

	private float y1;

	private float y2;

	private float xOff;

	private float yOff;

	private int[] lineY = new int[3] { 25, 50, 99 };

	private int[] lineY2 = new int[3] { 21, 46, 91 };

	private int[] keyOffX = new int[3] { 61, 34, 1 };

	private string[] splitMb;

	private int first;

	private int second;

	private int third;

	private float lowestDt = 10000f;

	private float highestDt;

	private float totalDt;

	private int totalFrames;

	private float totalGPUTime;

	private float totalCPUTime;

	private float totalOtherTime;

	private float totalTimeElapsed;

	private float totalSeconds;

	private float renderSeconds;

	private float lateSeconds;

	private float dt;

	private int frameCount;

	private int frameIter = 1;

	private float eTotalSeconds;

	private int lastCollectionCount = -1;

	private float mem;

	private bool memGb;

	private static Color[] fpsColors;

	private static Color[] fpsColorsTo;

	private Color lineColor = new Color(1f, 1f, 1f, 0.25f);

	private Color darkenedBack = new Color(0f, 0f, 0f, 0.5f);

	private Color darkenedBackWhole = new Color(0f, 0f, 0f, 0.25f);

	private Color32[] colorsWrite;

	private Rect graphSizeGUI;

	private Stopwatch stopWatch;

	private float lastElapsed;

	private float fps;

	private int graphSizeMin;

	private FPSGraphViewMode viewMode;

	private int xExtern;

	private int yExtern;

	private int startAt;

	private int yOffset;

	private int xLength;

	private int k;

	private int z;

	private bool hasFormated;

	private Rect wRect;

	private GUIStyle backupLabel;

	private GUIStyle backupButton;

	private GUIStyle h1;

	private GUIStyle h2;

	private GUIStyle h3;

	private GUIStyle guiButton;

	private GUIStyle graphTitles;

	private Vector2[] circleGraphLabels;

	private float w;

	private float h;

	public void CreateLineMaterial()
	{
		mat = new Material(Shader.Find("GUI/Text Shader"));
	}

	private void Awake()
	{
		if (base.gameObject.GetComponent<Camera>() == null)
		{
			UnityEngine.Debug.LogWarning("FPS Graph needs to be attached to a Camera object");
		}
		CreateLineMaterial();
		fpsColors = new Color[3] { RenderColor, CpuColor, OtherColor };
		fpsColorsTo = new Color[3]
		{
			fpsColors[0] * 0.7f,
			fpsColors[1] * 0.7f,
			fpsColors[2] * 0.7f
		};
	}

	private IEnumerator Start()
	{
		graphSizeMin = ((frameHistoryLength <= 95) ? 95 : frameHistoryLength);
		textOverlayMask = new int[graphHeight, graphSizeMin];
		reset();
		graphTexture = new Texture2D(graphSizeMin, 7, TextureFormat.ARGB32, false, false);
		colorsWrite = new Color32[graphTexture.width * 7];
		graphTexture.filterMode = FilterMode.Point;
		graphSizeGUI = new Rect(0f, 0f, graphTexture.width * graphMultiply, graphTexture.height * graphMultiply);
		addFPSAt(14, 23);
		addFPSAt(14, 48);
		addFPSAt(14, 93);
		if (showFPSNumber)
		{
			addFPSAt(14, 0);
		}
		for (int x = 0; x < graphTexture.width; x++)
		{
			for (int y = 0; y < 7; y++)
			{
				Color color = Color.white;
				if (x < 95 && y < 5)
				{
					color.a = graphKeysAlpha[y * 95 + x];
				}
				else
				{
					color.a = 0f;
				}
				graphTexture.SetPixel(x, y, color);
				colorsWrite[y * graphTexture.width + x] = color;
			}
		}
		graphTexture.Apply();
		while (true)
		{
			yield return new WaitForEndOfFrame();
			eTotalSeconds = (float)stopWatch.Elapsed.TotalSeconds;
			dt = eTotalSeconds - lastElapsed;
		}
	}

	private void reset()
	{
		dtHistory = new float[3, frameHistoryLength];
		gcHistory = new int[frameHistoryLength];
		stopWatch = new Stopwatch();
		stopWatch.Start();
		lowestDt = 10000f;
		highestDt = 0f;
		totalDt = 0f;
		totalFrames = 0;
		totalGPUTime = 0f;
		totalCPUTime = 0f;
		totalOtherTime = 0f;
		totalTimeElapsed = 0f;
		frameIter = 0;
		frameCount = 1;
	}

	private void addFPSAt(int startX, int startY)
	{
		yExtern = startY;
		for (int i = 0; i < 4; i++)
		{
			xExtern = startX;
			yOffset = i * 11;
			for (int j = 0; j < 11; j++)
			{
				textOverlayMask[yExtern, xExtern] = fpsBits[yOffset + j];
				xExtern++;
			}
			yExtern++;
		}
	}

	private void addNumberAt(int startX, int startY, int num, bool isLeading)
	{
		if (isLeading && num == 0)
		{
			num = -1;
		}
		startAt = num * 4;
		xLength = startAt + 3;
		yExtern = startY;
		for (z = 0; z < 5; z++)
		{
			xExtern = startX;
			yOffset = z * 39;
			for (k = startAt; k < xLength; k++)
			{
				if (num != -1 && numberBits[yOffset + k] == 1)
				{
					x1 = (float)(xExtern * graphMultiply) + xOff;
					y1 = (float)(yExtern * graphMultiply) + yOff;
					GL.Vertex3(x1, y1, 0f);
					GL.Vertex3(x1, y1 + (float)(1 * graphMultiply), 0f);
					GL.Vertex3(x1 + (float)(1 * graphMultiply), y1 + (float)(1 * graphMultiply), 0f);
					GL.Vertex3(x1 + (float)(1 * graphMultiply), y1, 0f);
				}
				xExtern++;
			}
			yExtern++;
		}
	}

	private void addPeriodAt(int startX, int startY)
	{
		x1 = (float)(startX * graphMultiply) + xOff;
		x2 = (float)((startX + 1) * graphMultiply) + xOff;
		y1 = (float)(startY * graphMultiply) + yOff;
		y2 = (float)((startY - 1) * graphMultiply) + yOff;
		GL.Vertex3(x1, y1, 0f);
		GL.Vertex3(x1, y2, 0f);
		GL.Vertex3(x2, y2, 0f);
		GL.Vertex3(x2, y1, 0f);
	}

	private void Update()
	{
		if (viewMode != 0)
		{
			return;
		}
		lastElapsed = (float)stopWatch.Elapsed.TotalSeconds;
		if (frameCount > 4)
		{
			dtHistory[0, frameIter] = dt;
			if (dt < lowestDt)
			{
				lowestDt = dt;
			}
			else if (dt > highestDt)
			{
				highestDt = dt;
			}
			if (frameIter % 10 == 0)
			{
				actualFPS = 10f / (Time.realtimeSinceStartup - lastUpdate);
				lastUpdate = Time.realtimeSinceStartup;
			}
			totalGPUTime += dtHistory[0, frameIter] - dtHistory[1, frameIter];
			totalCPUTime += dtHistory[1, frameIter] - dtHistory[2, frameIter];
			totalOtherTime += dtHistory[2, frameIter];
			if (lastCollectionCount != GC.CollectionCount(0))
			{
				gcHistory[frameIter] = 1;
				lastCollectionCount = GC.CollectionCount(0);
			}
			totalDt += dt;
			totalFrames++;
			frameIter++;
			if (frameIter >= frameHistoryLength)
			{
				frameIter = 0;
			}
			beforeRender = (float)stopWatch.Elapsed.TotalSeconds;
		}
		frameCount++;
	}

	private void LateUpdate()
	{
		eTotalSeconds = (float)stopWatch.Elapsed.TotalSeconds;
		dt = eTotalSeconds - beforeRender;
		dtHistory[2, frameIter] = dt;
		beforeRender = eTotalSeconds;
	}

	private void OnPostRender()
	{
		GL.PushMatrix();
		mat.SetPass(0);
		GL.LoadPixelMatrix();
		GL.Begin(7);
		if (viewMode == FPSGraphViewMode.graphing)
		{
			xOff = graphPosition.x * (w - (float)(frameHistoryLength * graphMultiply));
			yOff = h - (float)(100 * graphMultiply) - graphPosition.y * (h - (float)(graphMultiply * 107));
			GL.Color(darkenedBackWhole);
			GL.Vertex3(xOff, yOff - (float)(8 * graphMultiply), 0f);
			GL.Vertex3(xOff, (float)(100 * graphMultiply) + yOff, 0f);
			GL.Vertex3((float)(graphSizeMin * graphMultiply) + xOff, 100f * (float)graphMultiply + yOff, 0f);
			GL.Vertex3((float)(graphSizeMin * graphMultiply) + xOff, yOff - (float)(8 * graphMultiply), 0f);
			maxFrame = 0f;
			for (x = 0; x < frameHistoryLength; x++)
			{
				totalSeconds = dtHistory[0, x];
				if (totalSeconds > maxFrame)
				{
					maxFrame = totalSeconds;
				}
				totalSeconds *= yMulti;
				fpsVals[0] = totalSeconds;
				renderSeconds = dtHistory[1, x];
				renderSeconds *= yMulti;
				fpsVals[1] = renderSeconds;
				lateSeconds = dtHistory[2, x];
				lateSeconds *= yMulti;
				fpsVals[2] = lateSeconds;
				i = x - frameIter - 1;
				if (i < 0)
				{
					i = frameHistoryLength + i;
				}
				x1 = (float)(i * graphMultiply) + xOff;
				x2 = (float)((i + 1) * graphMultiply) + xOff;
				for (j = 0; j < fpsVals.Length; j++)
				{
					y1 = ((j >= fpsVals.Length - 1) ? yOff : (fpsVals[j + 1] * (float)graphMultiply + yOff));
					y2 = fpsVals[j] * (float)graphMultiply + yOff;
					GL.Color(fpsColorsTo[j]);
					GL.Vertex3(x1, y1, 0f);
					GL.Vertex3(x2, y1, 0f);
					GL.Color(fpsColors[j]);
					GL.Vertex3(x2, y2, 0f);
					GL.Vertex3(x1, y2, 0f);
				}
				if (gcHistory[x] == 1)
				{
					y1 = (float)(0 * graphMultiply) + yOff;
					y2 = (float)(-2 * graphMultiply) + yOff;
					GL.Color(Color.red);
					GL.Vertex3(x1, y1, 0f);
					GL.Vertex3(x2, y1, 0f);
					GL.Vertex3(x2, y2, 0f);
					GL.Vertex3(x1, y2, 0f);
				}
			}
			if (maxFrame < 1f / 120f)
			{
				maxFrame = 1f / 120f;
			}
			else if (maxFrame < 1f / 60f)
			{
				maxFrame = 1f / 60f;
			}
			else if (maxFrame < 1f / 30f)
			{
				maxFrame = 1f / 30f;
			}
			else if (maxFrame < 1f / 15f)
			{
				maxFrame = 1f / 15f;
			}
			else if (maxFrame < 0.1f)
			{
				maxFrame = 0.1f;
			}
			else if (maxFrame < 0.2f)
			{
				maxFrame = 0.2f;
			}
			yMulti = (float)graphHeight / maxFrame;
			GL.Color(lineColor);
			x1 = (float)(28 * graphMultiply) + xOff;
			x2 = (float)(graphSizeMin * graphMultiply) + xOff;
			for (i = 0; i < lineY.Length; i++)
			{
				y1 = (float)(lineY[i] * graphMultiply) + yOff;
				y2 = (float)((lineY[i] + 1) * graphMultiply) + yOff;
				GL.Vertex3(x1, y1, 0f);
				GL.Vertex3(x1, y2, 0f);
				GL.Vertex3(x2, y2, 0f);
				GL.Vertex3(x2, y1, 0f);
			}
			GL.Color(darkenedBack);
			x2 = (float)(27 * graphMultiply) + xOff;
			for (i = 0; i < lineY.Length; i++)
			{
				y1 = (float)(lineY2[i] * graphMultiply) + yOff;
				y2 = (float)((lineY2[i] + 9) * graphMultiply) + yOff;
				GL.Vertex3(xOff, y1, 0f);
				GL.Vertex3(xOff, y2, 0f);
				GL.Vertex3(x2, y2, 0f);
				GL.Vertex3(x2, y1, 0f);
			}
			for (i = 0; i < keyOffX.Length; i++)
			{
				x1 = (float)(keyOffX[i] * graphMultiply) + xOff + (float)(1 * graphMultiply);
				x2 = (float)((keyOffX[i] + 4) * graphMultiply) + xOff + (float)(1 * graphMultiply);
				y1 = (float)(5 * graphMultiply) + yOff - (float)(9 * graphMultiply);
				y2 = (float)(1 * graphMultiply) + yOff - (float)(9 * graphMultiply);
				GL.Color(fpsColorsTo[i]);
				GL.Vertex3(x1, y1, 0f);
				GL.Vertex3(x1, y2, 0f);
				GL.Vertex3(x2, y2, 0f);
				GL.Vertex3(x2, y1, 0f);
			}
			for (i = 0; i < keyOffX.Length; i++)
			{
				x1 = (float)(keyOffX[i] * graphMultiply) + xOff;
				x2 = (float)((keyOffX[i] + 4) * graphMultiply) + xOff;
				y1 = (float)(5 * graphMultiply) + yOff - (float)(8 * graphMultiply);
				y2 = (float)(1 * graphMultiply) + yOff - (float)(8 * graphMultiply);
				GL.Color(fpsColors[i]);
				GL.Vertex3(x1, y1, 0f);
				GL.Vertex3(x1, y2, 0f);
				GL.Vertex3(x2, y2, 0f);
				GL.Vertex3(x2, y1, 0f);
			}
			GL.Color(Color.white);
			for (x = 0; x < graphTexture.width; x++)
			{
				for (y = 0; y < graphHeight; y++)
				{
					if (textOverlayMask[y, x] == 1)
					{
						x1 = (float)(x * graphMultiply) + xOff;
						x2 = (float)(x * graphMultiply + 1 * graphMultiply) + xOff;
						y1 = (float)(y * graphMultiply) + yOff;
						y2 = (float)(y * graphMultiply + 1 * graphMultiply) + yOff;
						GL.Vertex3(x1, y1, 0f);
						GL.Vertex3(x1, y2, 0f);
						GL.Vertex3(x2, y2, 0f);
						GL.Vertex3(x2, y1, 0f);
					}
				}
			}
			if (maxFrame > 0f)
			{
				fps = Mathf.Round(1f / maxFrame);
				if (showFPSNumber && actualFPS > 0f)
				{
					float num = Mathf.Round(actualFPS);
					addNumberAt(1, 0, (int)(num / 100f % 10f), true);
					addNumberAt(5, 0, (int)((double)num / 10.0 % 10.0), false);
					addNumberAt(9, 0, (int)(num % 10f), false);
				}
				addNumberAt(1, 93, (int)(fps / 100f % 10f), true);
				addNumberAt(5, 93, (int)((double)fps / 10.0 % 10.0), true);
				addNumberAt(9, 93, (int)(fps % 10f), false);
				fps *= 2f;
				addNumberAt(1, 48, (int)(fps / 100f % 10f), true);
				addNumberAt(5, 48, (int)(fps / 10f % 10f), true);
				addNumberAt(9, 48, (int)(fps % 10f), false);
				fps *= 1.5f;
				addNumberAt(1, 23, (int)(fps / 100f % 10f), true);
				addNumberAt(5, 23, (int)(fps / 10f % 10f), true);
				addNumberAt(9, 23, (int)(fps % 10f), false);
				if (frameIter % 30 == 1)
				{
					mem = (float)Native.GetMemoryUsage() / 1000000f;
					if (mem > 100f)
					{
						memGb = true;
						mem /= 1000f;
					}
					else
					{
						memGb = false;
					}
					lastCollectionCount = GC.CollectionCount(0);
				}
				if ((double)mem < 1.0)
				{
					splitMb = mem.ToString("F3").Split("."[0]);
					if (splitMb[1][0] == "0"[0])
					{
						first = 0;
						second = int.Parse(splitMb[1]);
						third = second % 10;
						second = second / 10 % 10;
					}
					else
					{
						first = int.Parse(splitMb[1]);
						third = first % 10;
						second = first / 10 % 10;
						first = first / 100 % 10;
					}
					addPeriodAt(96, -6);
					addNumberAt(98, -7, first, false);
					addNumberAt(102, -7, second, false);
					addNumberAt(106, -7, third, false);
				}
				else
				{
					splitMb = mem.ToString("F1").Split("."[0]);
					first = int.Parse(splitMb[0]);
					if (first >= 10)
					{
						addNumberAt(96, -7, first / 10, false);
					}
					second = first % 10;
					if (second < 0)
					{
						second = 0;
					}
					addNumberAt(100, -7, second, false);
					addPeriodAt(104, -6);
					addNumberAt(106, -7, int.Parse(splitMb[1]), false);
				}
			}
			if (memGb)
			{
				for (x = 0; x < 7; x++)
				{
					for (y = 0; y < 4; y++)
					{
						if (gbBits[y * 7 + x] == 1)
						{
							x1 = (float)(x * graphMultiply) + xOff + (float)(111 * graphMultiply);
							x2 = (float)(x * graphMultiply + 1 * graphMultiply) + xOff + (float)(111 * graphMultiply);
							y1 = (float)(y * graphMultiply) + yOff + (float)(-7 * graphMultiply);
							y2 = (float)(y * graphMultiply + 1 * graphMultiply) + yOff + (float)(-7 * graphMultiply);
							GL.Vertex3(x1, y1, 0f);
							GL.Vertex3(x1, y2, 0f);
							GL.Vertex3(x2, y2, 0f);
							GL.Vertex3(x2, y1, 0f);
						}
					}
				}
			}
			else
			{
				for (x = 0; x < 9; x++)
				{
					for (y = 0; y < 4; y++)
					{
						if (mbBits[y * 9 + x] == 1)
						{
							x1 = (float)(x * graphMultiply) + xOff + (float)(111 * graphMultiply);
							x2 = (float)(x * graphMultiply + 1 * graphMultiply) + xOff + (float)(111 * graphMultiply);
							y1 = (float)(y * graphMultiply) + yOff + (float)(-7 * graphMultiply);
							y2 = (float)(y * graphMultiply + 1 * graphMultiply) + yOff + (float)(-7 * graphMultiply);
							GL.Vertex3(x1, y1, 0f);
							GL.Vertex3(x1, y2, 0f);
							GL.Vertex3(x2, y2, 0f);
							GL.Vertex3(x2, y1, 0f);
						}
					}
				}
			}
		}
		else
		{
			if (circleGraphLabels == null)
			{
				circleGraphLabels = new Vector2[3];
			}
			Rect rect = new Rect(w * 0.05f, h * 0.05f, w * 0.9f, h * 0.9f);
			GL.Color(new Color(0f, 0f, 0f, 0.8f));
			GL.Vertex3(rect.x, rect.y, 0f);
			GL.Vertex3(rect.x + rect.width, rect.y, 0f);
			GL.Vertex3(rect.x + rect.width, rect.y + rect.height, 0f);
			GL.Vertex3(rect.x, rect.y + rect.height, 0f);
			if (viewMode == FPSGraphViewMode.totalperformance)
			{
				float num2 = totalCPUTime + totalGPUTime + totalOtherTime;
				float[] array = new float[3]
				{
					totalGPUTime / num2,
					totalCPUTime / num2,
					totalOtherTime / num2
				};
				float[] array2 = new float[3]
				{
					array[0],
					array[0] + array[1],
					1f
				};
				float num3 = w * 0.15f;
				float num4 = 0f;
				float num5 = (float)Math.PI / 120f;
				Vector2 vector = new Vector2(w * 0.7f, h * 0.5f);
				int num6 = 0;
				float num7 = 0f;
				for (num6 = 0; num6 < 3; num6++)
				{
					float f = (array2[num6] - array[num6] * 0.5f) * ((float)Math.PI * 2f);
					float num8 = num3 * 0.3f * Mathf.Cos(f);
					num8 = ((!(num8 < 0f)) ? num8 : (num8 + num8));
					num8 = vector.x + num8;
					float num9 = vector.y + num3 * 0.5f * Mathf.Sin(f) + 0.02f * h;
					circleGraphLabels[num6] = new Vector2(num8, (float)Screen.height - num9);
				}
				num6 = 0;
				while (num4 < (float)Math.PI * 2f)
				{
					float num10 = num4 / ((float)Math.PI * 2f);
					if (num10 > array2[num6])
					{
						num6++;
						num7 = 0f;
					}
					else
					{
						num7 += num5 / ((float)Math.PI * 2f);
					}
					Color color = fpsColors[num6] - fpsColors[num6] * 0.4f;
					float num11 = num7 / array[num6];
					GL.Color(fpsColors[num6] * 0.85f + color * num11);
					float num8 = vector.x + num3 * Mathf.Cos(num4);
					float num9 = vector.y + num3 * Mathf.Sin(num4);
					num4 += num5;
					GL.Vertex3(vector.x, vector.y, 0f);
					GL.Vertex3(num8, num9, 0f);
					num8 = vector.x + num3 * Mathf.Cos(num4);
					num9 = vector.y + num3 * Mathf.Sin(num4);
					GL.Vertex3(num8, num9, 0f);
					GL.Vertex3(vector.x, vector.y, 0f);
				}
			}
			rect = new Rect(w * 0.375f, h * 0.08f, w * 0.25f, h * 0.11f);
			GL.Color(fpsColorsTo[1]);
			GL.Vertex3(rect.x, rect.y, 0f);
			GL.Vertex3(rect.x + rect.width, rect.y, 0f);
			GL.Color(fpsColors[1]);
			GL.Vertex3(rect.x + rect.width, rect.y + rect.height, 0f);
			GL.Vertex3(rect.x, rect.y + rect.height, 0f);
			float num12 = ((viewMode != FPSGraphViewMode.assetbreakdown) ? (0.5f * w) : (w * 0.05f));
			rect = new Rect(num12, h * 0.84f, w * 0.45f, h * 0.11f);
			GL.Color(fpsColorsTo[1]);
			GL.Vertex3(rect.x, rect.y, 0f);
			GL.Vertex3(rect.x + rect.width, rect.y, 0f);
			GL.Color(fpsColors[1]);
			GL.Vertex3(rect.x + rect.width, rect.y + rect.height, 0f);
			GL.Vertex3(rect.x, rect.y + rect.height, 0f);
		}
		GL.End();
		GL.PopMatrix();
		dt = (float)stopWatch.Elapsed.TotalSeconds - beforeRender;
		dtHistory[1, frameIter] = dt;
		eTotalSeconds = (float)stopWatch.Elapsed.TotalSeconds;
		dt = eTotalSeconds - lastElapsed;
	}

	private void format()
	{
		if (!hasFormated)
		{
			hasFormated = true;
			h1 = GUI.skin.GetStyle("Label");
			backupLabel = new GUIStyle(h1);
			backupButton = new GUIStyle(GUI.skin.GetStyle("Button"));
			h1.alignment = TextAnchor.UpperLeft;
			h1.fontSize = (int)((float)Screen.height * 0.08f);
			h2 = new GUIStyle(h1);
			h2.fontSize = (int)((float)Screen.height * 0.05f);
			h3 = new GUIStyle(h1);
			h3.fontSize = (int)((float)Screen.height * 0.037f);
			graphTitles = new GUIStyle(h1);
			graphTitles.fontSize = (int)((float)Screen.height * 0.037f);
			guiButton = new GUIStyle(h1);
			guiButton.normal.background = null;
		}
	}

	private void OnGUI()
	{
		w = Screen.width;
		h = Screen.height;
		if (viewMode != 0)
		{
			Time.timeScale = 0f;
			format();
			Color color = GUI.color;
			GUI.color = Color.black;
			Rect rect = new Rect(w * 0.05f, h * 0.05f, w * 0.9f, h * 0.9f);
			GUI.color = Color.black;
			GUI.color = Color.white;
			GUI.skin.label = h2;
			GUI.Label(new Rect(w * 0.1f, h * 0.07f, w, h * 0.2f), "Performance Results");
			GUI.Label(new Rect(w * 0.62f, h * 0.07f, w, h * 0.2f), "Assets Used");
			if (viewMode == FPSGraphViewMode.totalperformance)
			{
				GUI.skin.label = h2;
				GUI.Label(new Rect(w * 0.1f, h * 0.2f, w, h * 0.2f), "Score:");
				GUI.skin.label = h1;
				GUI.Label(new Rect(w * 0.1f, h * 0.27f, w, h * 0.2f), (totalDt * 1000f).ToString("n0") + "ms");
				GUI.skin.label = h2;
				GUI.Label(new Rect(w * 0.1f, h * 0.38f, w, h * 0.2f), "Time Elapsed:");
				GUI.skin.label = h1;
				GUI.Label(new Rect(w * 0.1f, h * 0.43f, w, h * 0.2f), totalTimeElapsed.ToString("F1") + "s");
				GUI.skin.label = h3;
				float num = totalDt / (float)totalFrames;
				string[] array = new string[3]
				{
					"lowest: " + (1f / highestDt).ToString("n0") + "fps",
					"highest: " + (1f / lowestDt).ToString("n0") + "fps",
					"avg: " + (1f / num).ToString("n0") + "fps"
				};
				for (int i = 0; i < array.Length; i++)
				{
					GUI.Label(new Rect(w * 0.1f, h * 0.57f + w * 0.04f * (float)i, w, h * 0.2f), array[i]);
				}
				GUI.color = Color.black;
				GUI.skin.label = graphTitles;
				array = new string[3] { "Render", "CPU", "Other" };
				float[] array2 = new float[3] { 0.12f, 0.12f, 0.12f };
				float num2 = 0.0023f * w;
				int num3 = 0;
				while (circleGraphLabels != null && num3 < circleGraphLabels.Length)
				{
					GUI.color = Color.black;
					GUI.Label(new Rect(circleGraphLabels[num3].x + num2, circleGraphLabels[num3].y + num2, w * array2[num3], h * 0.047f), array[num3]);
					GUI.color = Color.white;
					GUI.Label(new Rect(circleGraphLabels[num3].x, circleGraphLabels[num3].y, w * array2[num3], h * 0.047f), array[num3]);
					num3++;
				}
			}
			else
			{
				GUILayout.BeginArea(new Rect(w * 0.08f, h * 0.175f, w * 0.9f, h * 0.7f));
				GUI.skin.label = h2;
				GUILayout.Label("All: " + Resources.FindObjectsOfTypeAll(typeof(UnityEngine.Object)).Length.ToString("n0"));
				GUILayout.Label("Textures: " + Resources.FindObjectsOfTypeAll(typeof(Texture)).Length.ToString("n0"));
				GUILayout.Label("AudioClips: " + Resources.FindObjectsOfTypeAll(typeof(AudioClip)).Length.ToString("n0"));
				GUILayout.Label("Meshes: " + Resources.FindObjectsOfTypeAll(typeof(Mesh)).Length.ToString("n0"));
				GUILayout.Label("Materials: " + Resources.FindObjectsOfTypeAll(typeof(Material)).Length.ToString("n0"));
				GUILayout.Label("GameObjects: " + Resources.FindObjectsOfTypeAll(typeof(GameObject)).Length.ToString("n0"));
				GUILayout.Label("Components: " + Resources.FindObjectsOfTypeAll(typeof(Component)).Length.ToString("n0"));
				GUILayout.EndArea();
			}
			GUI.skin.button = guiButton;
			if (GUI.Button(new Rect(w * 0.05f, h * 0.05f, w * 0.45f, h * 0.15f), string.Empty))
			{
				viewMode = FPSGraphViewMode.totalperformance;
			}
			if (GUI.Button(new Rect(w * 0.5f, h * 0.05f, w * 0.45f, h * 0.15f), string.Empty))
			{
				viewMode = FPSGraphViewMode.assetbreakdown;
			}
			if (GUI.Button(new Rect(w * 0.3f, h * 0.8f, w * 0.4f, h * 0.15f), string.Empty))
			{
				reset();
				viewMode = FPSGraphViewMode.graphing;
				Time.timeScale = 1f;
			}
			rect = new Rect(w * 0.435f, h * 0.83f, w * 0.25f, h * 0.11f);
			GUI.skin.label = h2;
			GUI.Label(rect, "Dismiss");
			GUI.skin.label = backupLabel;
			GUI.skin.button = backupButton;
			GUI.color = color;
		}
		else if (Time.frameCount > 4)
		{
			GUI.DrawTexture(new Rect(graphPosition.x * (w - (float)(graphMultiply * frameHistoryLength)), graphPosition.y * (h - (float)(graphMultiply * 107)) + (float)(100 * graphMultiply), graphSizeGUI.width, graphSizeGUI.height), graphTexture);
		}
		if (showPerformanceOnClick && didPressOnGraph() && highestDt > 0f)
		{
			showPerformance();
		}
	}

	public void showPerformance()
	{
		if (viewMode != FPSGraphViewMode.totalperformance)
		{
			totalTimeElapsed = Time.time;
			viewMode = FPSGraphViewMode.totalperformance;
		}
	}

	public bool didPressOnGraph()
	{
		if (Input.touchCount > 0 || Input.GetMouseButtonDown(0))
		{
			Rect rect = new Rect(graphPosition.x * (w - (float)(graphMultiply * frameHistoryLength)), graphPosition.y * (h - (float)(graphMultiply * 107)), graphSizeGUI.width, 107 * graphMultiply);
			if (Input.touchCount > 0)
			{
				for (int i = 0; i < Input.touchCount; i++)
				{
					if (Input.touches[i].phase == TouchPhase.Ended && checkWithinRect(Input.touches[i].position, rect))
					{
						return true;
					}
				}
			}
			else if (Input.GetMouseButtonDown(0) && checkWithinRect(Input.mousePosition, rect))
			{
				return true;
			}
		}
		return false;
	}

	public static bool checkWithinRect(Vector2 vec2, Rect rect)
	{
		vec2.y = (float)Screen.height - vec2.y;
		return vec2.x > rect.x && vec2.x < rect.x + rect.width && vec2.y > rect.y && vec2.y < rect.y + rect.height;
	}
}
