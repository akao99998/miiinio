using Kampai.Common.Service.Audio;
using Kampai.Game;
using Kampai.Main;
using UnityEngine;
using strange.extensions.mediation.impl;

public class EnvironmentAudioManagerMediator : EventMediator
{
	private CustomFMOD_StudioEventEmitter waterEmitter;

	private string prevHit;

	[Inject]
	public EnvironmentAudioManagerView view { get; set; }

	[Inject(MainElement.CAMERA)]
	public Camera mainCamera { get; set; }

	[Inject]
	public Environment environment { get; set; }

	[Inject]
	public IFMODService fmodService { get; set; }

	public override void OnRegister()
	{
		prevHit = string.Empty;
		view.checkHit.AddListener(checkHit);
		view.mainCamera = mainCamera;
		Init();
	}

	private CustomFMOD_StudioEventEmitter CreateAudioEmitter(GameObject go, string audioName)
	{
		CustomFMOD_StudioEventEmitter customFMOD_StudioEventEmitter = go.AddComponent<CustomFMOD_StudioEventEmitter>();
		customFMOD_StudioEventEmitter.shiftPosition = false;
		customFMOD_StudioEventEmitter.staticSound = false;
		customFMOD_StudioEventEmitter.path = fmodService.GetGuid(audioName);
		customFMOD_StudioEventEmitter.Play();
		return customFMOD_StudioEventEmitter;
	}

	private void Init()
	{
		CreateAudioEmitter(view.gameObject, "Play_environment_everglades_01");
		waterEmitter = CreateAudioEmitter(view.gameObject, "Play_water_stream_light_01");
	}

	private void hitLand()
	{
		waterEmitter.Fade(1f, 0f, 2f);
	}

	private void hitWater()
	{
		waterEmitter.Fade(0f, 1f, 2f);
	}

	public void checkHit(Vector3 point)
	{
		int num = Mathf.Max((Mathf.RoundToInt(point.x) < environment.Definition.DefinitionGrid.GetLength(0)) ? Mathf.RoundToInt(point.x) : (environment.Definition.DefinitionGrid.GetLength(0) - 1), 0);
		int num2 = Mathf.Max((Mathf.RoundToInt(point.z) < environment.Definition.DefinitionGrid.GetLength(1)) ? Mathf.RoundToInt(point.z) : (environment.Definition.DefinitionGrid.GetLength(1) - 1), 0);
		int num3 = 5;
		bool flag = false;
		for (int i = Mathf.Max(0, num - num3); i < Mathf.Min(num + num3, environment.Definition.DefinitionGrid.GetLength(0)); i += num3)
		{
			for (int j = Mathf.Max(0, num2 - num3); j < Mathf.Min(num2 + num3, environment.Definition.DefinitionGrid.GetLength(1)); j += num3)
			{
				if (environment.Definition.IsWater(i, j))
				{
					flag = true;
					if (!prevHit.Equals("water"))
					{
						hitWater();
						prevHit = "water";
						return;
					}
				}
			}
		}
		if (!flag && !prevHit.Equals("land"))
		{
			hitLand();
			prevHit = "land";
		}
	}
}
