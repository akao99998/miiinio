using Kampai.Game.Mignette.View;
using Kampai.Main;
using Kampai.Util.Audio;
using UnityEngine;

public class WaterSlideBuildingViewObject : MignetteBuildingViewObject
{
	public Vector3 SpinnerBallOffset = Vector3.forward;

	public GameObject ClimbPoint;

	public GameObject[] PartsDisabledDuringCooldown;

	public void Start()
	{
		base.gameObject.AddComponent<MignetteBuildingCooldownView>();
	}

	public override void ResetCooldownView(PlayLocalAudioSignal localAudioSignal)
	{
		GameObject[] partsDisabledDuringCooldown = PartsDisabledDuringCooldown;
		foreach (GameObject gameObject in partsDisabledDuringCooldown)
		{
			gameObject.SetActive(true);
		}
		localAudioSignal.Dispatch(GetAudioEmitter.Get(base.gameObject, "WaterslideBuilding"), "Play_waterslide_active_loop_01", null);
	}

	public override void UpdateCooldownView(PlayLocalAudioSignal localAudioSignal, int buildingData, float pctDone)
	{
		if (pctDone < 1f)
		{
			GameObject[] partsDisabledDuringCooldown = PartsDisabledDuringCooldown;
			foreach (GameObject gameObject in partsDisabledDuringCooldown)
			{
				if (gameObject != null)
				{
					gameObject.SetActive(false);
				}
			}
			CustomFMOD_StudioEventEmitter component = base.gameObject.GetComponent<CustomFMOD_StudioEventEmitter>();
			if (component != null)
			{
				component.Stop();
			}
		}
		else
		{
			localAudioSignal.Dispatch(GetAudioEmitter.Get(base.gameObject, "WaterslideBuilding"), "Play_waterslide_active_loop_01", null);
		}
	}
}
