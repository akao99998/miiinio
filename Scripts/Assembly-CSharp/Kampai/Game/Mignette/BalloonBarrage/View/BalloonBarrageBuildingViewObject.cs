using Kampai.Game.Mignette.View;
using Kampai.Main;
using UnityEngine;

namespace Kampai.Game.Mignette.BalloonBarrage.View
{
	public class BalloonBarrageBuildingViewObject : MignetteBuildingViewObject
	{
		public enum BalloonBarrageThrowTypes
		{
			Push = 0,
			Pull = 1
		}

		public BalloonBarrageThrowTypes BalloonBarrageThrowType = BalloonBarrageThrowTypes.Pull;

		public Transform BalloonPilotIntroLocator;

		public GameObject BasketPrefab;

		public GameObject MangoPrefab;

		public GameObject MangoHitBodyVFXPrefab;

		public GameObject MinionFaceSplatVFXPrefab;

		public GameObject MinionHitGroundVFXPrefab;

		public GameObject BalloonPopVFXPrefab;

		public GameObject MangoCaughtVfxPrefab;

		public Transform CameraTransform;

		public float FieldOfView;

		public float NearClipPlane;

		public bool BalloonIsTakingOff;

		public GameObject[] ObjectsDisabledDuringCooldown;

		public void Start()
		{
			base.gameObject.AddComponent<MignetteBuildingCooldownView>();
		}

		public override void ResetCooldownView(PlayLocalAudioSignal localAudioSignal)
		{
			for (int i = 0; i < ObjectsDisabledDuringCooldown.Length; i++)
			{
				ObjectsDisabledDuringCooldown[i].SetActive(true);
			}
		}

		public override void UpdateCooldownView(PlayLocalAudioSignal localAudioSignal, int buildingData, float pctDone)
		{
			int num = (int)(pctDone * (float)ObjectsDisabledDuringCooldown.Length);
			for (int i = 0; i < ObjectsDisabledDuringCooldown.Length; i++)
			{
				if (i < num)
				{
					ObjectsDisabledDuringCooldown[i].SetActive(true);
				}
				else
				{
					ObjectsDisabledDuringCooldown[i].SetActive(false);
				}
			}
		}

		public void BalloonTakingOff()
		{
			BalloonIsTakingOff = true;
		}
	}
}
